## Plan: Revit V4 Settings Schema + Report Controls

Add a new Revit extensible storage schema version (V4) and propagate the new settings through shared settings model, Revit/Desktop settings flows, and SQLite upgrade logic so values are persisted consistently across Revit and Desktop. Scope includes: online transfer service visibility + selected service, and per-report string pairs for 4 reports using the names DocumentTypeCode and FirstNumber.

**Steps**
1. Phase 1 - Define new settings contract (blocks all later steps)
1. Add new properties to SettingsModel for transfer-service controls and per-report values:
1. Online transfer controls: show/hide toggle + selected service enum value.
1. Report pairs (8 strings total): ProjectDirectoryDocumentTypeCode, ProjectDirectoryFirstNumber, TransmittalSheetDocumentTypeCode, TransmittalSheetFirstNumber, TransmittalSummaryDocumentTypeCode, TransmittalSummaryFirstNumber, MasterDocumentsListDocumentTypeCode, MasterDocumentsListFirstNumber.
1. Confirm defaults: empty string for report fields, existing default service value for file transfer type.

1. Phase 2 - Revit extensible storage V4 migration (depends on Phase 1)
1. Introduce V4 constants and new GUID in SettingsServiceRvt, keep V0-V3 read paths intact.
1. Update schema detection flow in GetSettingsRvt to prefer V4 and migrate from V3 when V4 missing.
1. Extend CreateSchema to V4 by adding all new fields with matching names from GlobalSettings.
1. Add GetSettingsFromSchemaV4 and update SaveSettingsToSchema to read/write new fields.
1. Keep previous version readers unchanged except migration ordering; V3 should still be readable for upgrade.

1. Phase 3 - SQLite settings persistence + DB upgrade (parallel with Phase 4 after Phase 1)
1. Extend SettingsService.GetSettings and UpdateSettings SQL mappings with all new columns.
1. Extend SQLiteDataAccess.UpgradeDatabase with ALTER TABLE checks for each new Settings column.
1. Ensure defaults when reading older DBs before migration-complete path is hit.

1. Phase 4 - Revit and Desktop settings UI/viewmodels (parallel with Phase 3 after Phase 1)
1. Revit SettingsViewModel: load/save new properties in SetPropertiesFromGlobalSettings, SetPropertiesFromImportedSettings, and SaveSettings.
1. Desktop SettingsViewModel: load/save new properties in SetPropertiesFromGlobalSettings and SaveSettings.
1. Revit SettingsView.xaml: add controls for transfer service visibility/selection and 4 report sections with DocumentTypeCode + FirstNumber inputs.
1. Desktop SettingsView.xaml: add matching controls for the same settings set.
1. Keep existing UseCDE behavior unchanged; new online transfer visibility should be independent unless product decision says otherwise.

1. Phase 5 - Runtime usage alignment (depends on Phases 2-4)
1. Wire transfer-service visibility and selected service to the parts of transmittal UI currently reading file transfer type.
1. Remove or de-prioritize temporary registry fallback for file transfer service once persisted value is available, while preserving backward compatibility fallback if value is missing.
1. Validate that both Revit and Desktop launch paths set GlobalSettings.FileTransferType from persisted settings.

1. Phase 6 - Import/export settings compatibility (depends on Phase 1, parallel with Phases 3-4)
1. Since ImportSettingsModel inherits SettingsModel, confirm JSON import pipeline carries new properties.
1. Update import template/documentation examples to include new settings keys so users can seed values.

1. Phase 7 - Tests and verification hardening (depends on all prior phases)
1. Add/extend tests for database upgrade behavior and settings read/write roundtrip for new fields.
1. Add/extend tests for OpenXML report contexts only if these new report fields are consumed in template token generation now.
1. Manual migration test matrix: V3 Revit schema -> V4, older DB -> upgraded DB, Revit save -> Desktop read.

**Relevant files**
- d:/Development/Source/Repos/Transmittal/source/Transmittal.Library/Models/SettingsModel.cs - add canonical properties used by all persistence and UI layers.
- d:/Development/Source/Repos/Transmittal/source/Transmittal/Services/SettingsServiceRvt.cs - add V4 schema constants, migration path, V4 read/write methods.
- d:/Development/Source/Repos/Transmittal/source/Transmittal.Library/Services/SettingsService.cs - map new properties in SELECT/UPDATE settings persistence.
- d:/Development/Source/Repos/Transmittal/source/Transmittal.Library/DataAccess/SQLiteDataAccess.cs - add new UpgradeDatabase column checks and ALTER statements.
- d:/Development/Source/Repos/Transmittal/source/Transmittal/ViewModels/SettingsViewModel.cs - Revit settings load/save bindings for new properties.
- d:/Development/Source/Repos/Transmittal/source/Transmittal/Views/SettingsView.xaml - Revit settings controls.
- d:/Development/Source/Repos/Transmittal/source/Transmittal.Desktop/ViewModels/SettingsViewModel.cs - Desktop settings load/save bindings for new properties.
- d:/Development/Source/Repos/Transmittal/source/Transmittal.Desktop/Views/SettingsView.xaml - Desktop settings controls.
- d:/Development/Source/Repos/Transmittal/source/Transmittal/ViewModels/TransmittalViewModel.cs - runtime file-transfer behavior currently tied to GlobalSettings.FileTransferType.
- d:/Development/Source/Repos/Transmittal/source/Transmittal/Commands/CommandImportSettings.cs - import JSON compatibility check for new settings.
- d:/Development/Source/Repos/Transmittal/ImportTemplate.json - include new keys for import/export usability.

**Verification**
1. Build affected projects: Transmittal, Transmittal.Desktop, Transmittal.Library, and tests.
1. Revit migration test: open model with V3 schema, verify values are migrated to V4 and persisted after save/reopen.
1. DB migration test: open with older DB lacking new columns, verify UpgradeDatabase adds columns and values are retained after save/reload.
1. Cross-app persistence test: set values in Revit settings, save, open Desktop on same DB, verify all new settings match.
1. Transfer service behavior test: toggle visibility and selected service, verify UI visibility and upload service choice align with saved values.
1. Regression test around existing settings: UseCDE, IssueFormats, DocumentStatuses, SheetPackageParamGuid, and path settings remain unchanged.

**Decisions**
- Persist new settings in both Revit extensible storage and database/Desktop path.
- Report scope is exactly 4 report types: Project Directory, Transmittal Sheet, Transmittal Summary, Master Documents List.
- Transfer service persistence includes both show/hide and selected service.
- Report field names are DocumentTypeCode and FirstNumber for each report.
- Included scope: persistence, UI binding, migration paths, compatibility checks.
- Excluded scope: redesigning report template token architecture unless requested in a follow-up.

**Further Considerations**
1. Recommendation: keep registry read as fallback only for one release cycle, then remove after confirming migration stability.
2. Recommendation: if report fields will be consumed in report output immediately, define explicit token names now to avoid later naming churn.
3. Recommendation: add lightweight migration logging in SettingsServiceRvt for V3->V4 to simplify support diagnostics.
