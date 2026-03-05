# Plan: WeTransfer Send Feature for Transmittal Export

**TL;DR** — After exporting transmittal sheets (PDF/DWG/DWF), add a new optional wizard step that prepares files for WeTransfer by opening the WeTransfer site in the user's default browser. Configuration is stored in Revit extensible storage and SQLite .tdb settings. The feature is toggled via a checkbox in the Revit add-in Settings UI. Uses Selenium.WebDriver (C#) to handle browser automation that opens WeTransfer with exported files pre-staged locally (reusing the transmittal distribution list as recipients).

## Feature Requirements

**What:** Post-export wizard step to prepare transmittal files for WeTransfer upload without actually sending.

**Who:** Project teams who export transmittals before CDE is set up; need to share files via WeTransfer.

**Why:** Automate the manual process of uploading files to WeTransfer and extracting the link; reduce friction in document distribution workflow.

**Scope:**
- Revit add-in export pipeline (PDF/DWG/DWF)
- New wizard page after distribution list step
- WeTransfer only (v1)
- Browser-based (not headless; visible to user)
- Settings stored in Revit extensible storage + SQLite .tdb
- Reuses transmittal distribution list as recipients

## Implementation Steps

### 1. Extend Settings Model & Storage

**Files to modify:**
- [source/Transmittal.Library/Models/SettingsModel.cs](source/Transmittal.Library/Models/SettingsModel.cs)
- [source/Transmittal/Services/SettingsServiceRvt.cs](source/Transmittal/Services/SettingsServiceRvt.cs)
- [source/Transmittal.Library/Services/SettingsService.cs](source/Transmittal.Library/Services/SettingsService.cs)
- [source/Transmittal.Library/DataAccess/SQLiteDataAccess.cs](source/Transmittal.Library/DataAccess/SQLiteDataAccess.cs)

**Changes:**
- Add `EnableWeTransferSend` boolean property to `SettingsModel`
- Add schema field in `SettingsServiceRvt.CreateSchema()` for the new setting (alongside existing fields)
- Update `SettingsServiceRvt.SaveSettingsToSchema()` and `SettingsServiceRvt.GetSettingsFromSchemaV3()` to persist/load the flag
- Add SQLite column migration in `SQLiteDataAccess.UpgradeDatabase()` (e.g., "EnableWeTransferSend INTEGER DEFAULT 0")
- Update `SettingsService.GetSettings()` and `SettingsService.UpdateSettings()` for SQLite read/write

### 2. Create WeTransfer Service

**New file:** `source/Transmittal/Services/WeTransferService.cs`

**Interface:**
```csharp
public class WeTransferService
{
    public async Task<WeTransferResult> PrepareWeTransferUploadAsync(
        List<string> filePaths, 
        List<string> recipients)
    {
        // Returns: link extracted from upload, or error message
    }
}

public class WeTransferResult
{
    public bool Success { get; set; }
    public string LinkOrError { get; set; }
}
```

**Implementation notes:**
- Uses `Selenium.WebDriver` to open browser and navigate to `https://wetransfer.com`
- Automates: consent dialogs acceptance, file upload via file input, link extraction once upload completes
- Handles failures: browser not found, upload timeout (300s or configurable), network issues
- Returns shareable link and/or error message for UI display

### 3. Wire New Wizard Step

**Files to modify:**
- [source/Transmittal/Views/TransmittalView.xaml](source/Transmittal/Views/TransmittalView.xaml)
- [source/Transmittal/Views/TransmittalView.xaml.cs](source/Transmittal/Views/TransmittalView.xaml.cs) (if needed for code-behind bindings)

**Changes:**
- Add new `wizardPage4` ("Send by WeTransfer") after Distribution page (`wizardPage3`)
- Update `wizardPage3` to set `NextVisible="True"` (currently likely `NextVisible="False"` with `FinishVisible="True"`)
- Move `FinishVisible="True"` to the new `wizardPage4`
- Page content:
  - Summary: "Exported files ready for WeTransfer"
  - Display file count and recipient email count from transmittal distribution
  - Button: "Prepare WeTransfer Upload" (or "Open WeTransfer")
  - Result display: link (copyable) or error message

### 4. Update Export Workflow

**Files to modify:**
- [source/Transmittal/ViewModels/TransmittalViewModel.cs](source/Transmittal/ViewModels/TransmittalViewModel.cs)

**Changes:**
- In `ProcessSheets()` command method (lines ~840–1000):
  - After `GenerateCDEImportFile()`, add check: if `EnableWeTransferSend` is true in settings
  - If true, call `WeTransferService.PrepareWeTransferUploadAsync(exportedFilePaths, recipientEmails)`
  - Capture result (link or error) and display to user before wizard closes
  - Offer option to copy link or retry on error
- Alternative: add separate command for wizard page button (instead of blocking `ProcessSheets()`)

### 5. Update Settings UI (Revit Add-in)

**Files to modify:**
- [source/Transmittal/ViewModels/SettingsViewModel.cs](source/Transmittal/ViewModels/SettingsViewModel.cs)
- [source/Transmittal/Views/SettingsView.xaml](source/Transmittal/Views/SettingsView.xaml) (or `SettingsView.xaml` equivalent)

**Changes:**
- Add property `EnableWeTransferSend` to `SettingsViewModel` with getter/setter binding to `SettingsModel`
- Add checkbox in settings XAML: "Prepare WeTransfer upload after export"
- Checkbox binding: `IsChecked="{Binding EnableWeTransferSend, Mode=TwoWay}"`
- Persist via existing settings service pattern (already wires Revit extensible storage + SQLite)

### 6. Add NuGet Dependency

**Files to modify:**
- [source/Transmittal/Transmittal.csproj](source/Transmittal/Transmittal.csproj)

**Changes:**
- Add package reference: `Selenium.WebDriver` (latest stable, e.g., 4.x)
- Decide on ChromeDriver distribution:
  - Option A: Bundle chromedriver.exe in installer and reference from packed folder
  - Option B: Rely on system PATH or user's Chrome installation (risky)
  - Recommended: Option A for reliability

### 7. Testing & Verification

**Unit tests:**
- Mock `IWebDriver` to test `WeTransferService` logic (link extraction, error handling)
- Add to [source/Transmittal.Tests](source/Transmittal.Tests)

**Integration tests:**
- Settings persist to Revit extensible storage and SQLite
- Wizard navigation shows new page when feature enabled
- File paths and recipients passed correctly to service

**Manual verification:**
- Enable setting in Revit add-in, export transmittal
- Verify new wizard page appears after Distribution
- Click "Prepare WeTransfer Upload" and confirm:
  - WeTransfer.com opens in default browser
  - Files are queued/visible in browser
  - Link is extracted and displayed in wizard upon completion
- Verify recipients list matches transmittal distribution

## Key Decisions

| Decision | Rationale |
|----------|-----------|
| **Selenium.WebDriver in C#** | Keeps dependency in C# ecosystem; avoids external Python script dependency. |
| **Visible browser** | Users see upload progress and can verify before sending; aligns with "prepare but don't send" requirement. |
| **New wizard step** | Gives users control and visibility; allows skip if not wanted; cleaner UX than post-export hook. |
| **Settings in Revit + SQLite** | Follows existing pattern; enables settings sharing between Revit add-in and desktop app. |
| **Reuse distribution list** | No separate config; reduces UI complexity; recipients already validated in transmittal workflow. |
| **No auto-send** | Link is extracted but not shared or sent; user retains control. |

## Related Files (Reference)

- Export completion flow: [source/Transmittal/ViewModels/TransmittalViewModel.cs](source/Transmittal/ViewModels/TransmittalViewModel.cs#L840-L1000) (`ProcessSheets()`)
- Wizard pages: [source/Transmittal/Views/TransmittalView.xaml](source/Transmittal/Views/TransmittalView.xaml#L300-L360)
- Existing settings pattern (Revit + SQLite): [source/Transmittal/Services/SettingsServiceRvt.cs](source/Transmittal/Services/SettingsServiceRvt.cs#L40-L175), [source/Transmittal.Library/Services/SettingsService.cs](source/Transmittal.Library/Services/SettingsService.cs#L23-L140)
- Distribution handling: [source/Transmittal.Library/Models/SettingsModel.cs](source/Transmittal.Library/Models/SettingsModel.cs) (reference distribution list structure)

## Optional Future Enhancements

- **SharePoint/OneDrive support** (v2) — Extend service to upload to SharePoint modern team sites via MS Graph.
- **Headless mode toggle** — Settings option to run browser in background vs. visible.
- **Link expiry** — WeTransfer premium feature; allow configuration in settings.
- **Upload history** — Persist links and timestamps in .tdb for audit trail.
