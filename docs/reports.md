---
layout: page
title: Customise Reports
permalink: /settings/reports/
---
Reports used by the addin default to embedded report templates.  Reports can be customised and placed in a shared location where multiple users can use the same report templates.

Sample report templates can be found in C:\Program Files\Transmittal\Reports when installed or on the [Github repo](https://github.com/russgreen/Transmittal/tree/main/SampleReports).

Report engine selection is automatic based on template files in the configured Report templates folder.

If the corresponding **.xlsx** template exists, Transmittal uses the OpenXML (Excel) report engine for that report.

If the corresponding **.xlsx** template does not exist, Transmittal uses the RDLC report engine for that report.

RDLC report names __must__ be:
- ProjectDirectory.rdlc
- TransmittalSheet.rdlc
- TransmittalSummary.rdlc
- MasterDocumentsList.rdlc

XLSX report names __must__ be:
- ProjectDirectory.xlsx
- TransmittalSheet.xlsx
- TransmittalSummary.xlsx
- MasterDocumentsList.xlsx

If a template is missing from the configured report folder, Transmittal falls back to the embedded/default report layout using the RDLC engine.

RDLC report template files can be edited in either [Syncfusion Standalone Report Designer](https://www.boldreports.com/standalone-report-designer) or [Microsoft Report Builder](https://www.microsoft.com/en-us/download/details.aspx?id=53613). Both tools are free.

Microsoft Report builder only appears to support RDL files.  However, if you right click on an RDLC file and choose Open with > Choose another app, the Report Builder can be chosen and the RDLC files can be opened, edited and saved.

![image](https://user-images.githubusercontent.com/1886088/175804963-6846002a-68fc-49c8-9987-27133c3c7763.png)

## OpenXML (Excel) template contract

The OpenXML report engine is designed to let you customise layout and branding in Excel while keeping data binding resilient.

It works by combining:
- **global text token replacement** (`{{...}}` placeholders), and
- **table/section detection** (finding template rows using named ranges).

This means templates can be visually redesigned (logo, fonts, spacing, extra notes, page setup) as long as required anchors and headers remain present.

### How OpenXML template resolution works

For each report type:
- If `<ReportName>.xlsx` exists in the configured reports folder, OpenXML is used.
- If it does not exist, Transmittal automatically falls back to the RDLC report of the same logical report.
- If neither custom file exists, Transmittal uses the embedded/default report.

Report selection is per-report, so you can mix engines (for example, `ProjectDirectory.xlsx` with `TransmittalSummary.rdlc`).

### Workbook and worksheet guidance

Templates should be normal Excel workbooks (`.xlsx`) with at least one visible worksheet.

General recommendations:
- Keep required headers as plain text in cells (not images/shapes).
- Avoid merged cells inside dynamic data tables where rows will be inserted/rewritten.
- Keep one clear header row per dynamic table.
- Apply formatting to the template row(s) that data will be copied/written into.
- Place logos, titles, borders, page setup, print area, and other static design elements anywhere outside (or around) the dynamic table ranges.

The engine preserves worksheet layout as much as possible and writes data into detected regions.

### Supported text tokens
{% raw %}
These placeholders are replaced in **all worksheet text cells**:
- `{{Project}}`
- `{{ProjectDisplay}}`
- `{{ProjectName}}`
- `{{ProjectNumber}}`
- `{{ProjectIdentifier}}`
- `{{ClientName}}`
- `{{ReportTitle}}`
- `{{ReportDate}}`
- `{{TransmittalDate}}`
{% endraw %}
Token notes:
- Tokens are intended to be used exactly as shown (including braces).
- Tokens can be used in headings, footers/notes cells, cover blocks, etc.
- You can include static text with tokens, for example: `Project: {{ProjectDisplay}}`.

### Header detection principles

When named ranges are not used, the engine finds data regions by scanning for expected column headers/anchors.

To ensure reliable detection:
- Use clear, conventional header labels (for example `Document`, `Drawing`, `Rev`, `Date`, `Company`, `Person`, `Format`, `Copies`).
- Keep related headers on the same row.
- Avoid duplicating the same full header set in multiple unrelated places unless intentional.
- Keep decorative text near table headers distinct from actual header names.

### Project directory template (`ProjectDirectory.xlsx`)

Supported approaches:

1. **Named range mode** (preferred)
   - Optional named ranges:
     - `ProjectDirectory`
     - `ProjectDirectoryRange`
   - If either named range exists, rows are written into that range and expanded as required.

2. **Header detection mode**
   - If no named range exists, the engine detects a directory table by `Name / Company / Email / Tel / Role` style headers.

Authoring tips:
- Place the directory header row immediately above the first data row.
- Put desired row formatting (font, border, wrap, alignment) on the first data row so inserted rows keep a consistent look.

### Transmittal sheet template (`TransmittalSheet.xlsx`)

The sheet contains two dynamic sections:
- **Items table** (documents/drawings included in the transmittal)
- **Distribution table** (who receives what/format/copies)

Detection rules:
- Items table is found by headers such as `Drawing` / `Document` / `Rev`.
- Distribution table is found by headers such as `Format` / `Copies` (or `Company` / `Person` variants).

Write behavior:
- Existing data rows in these sections are cleared/replaced.
- Table structure is rewritten for current transmittal content.
- Surrounding layout and styling are preserved where possible.

Authoring tips:
- Keep these two table header blocks clearly separated.
- Do not place unrelated mini-tables with similar headers directly adjacent to avoid ambiguity.

### Transmittal summary template (`TransmittalSummary.xlsx`)

This template displays a matrix of all documents across multiple transmittals, plus a distribution summary showing who received copies.

**Required named ranges:**
- `SheetListData` – Marks the template row for the document/item matrix
- `DistributionListData` – Marks the template row for the distribution matrix
- `SummaryColumnData` – Defines the column(s) to use as a formatting template for transmittal date columns

**Optional named ranges:**
- `TransmittalFormatData` – If present, marks the row where the most common transmittal format (e.g., "PDF", "Email") will be written for each transmittal column

**Date row detection:**
{% raw %}
The engine locates the Year/Month/Day rows using token markers:
- `{{DateYear}}` – Marks the row that should display the transmittal year (YY format)
- `{{DateMonth}}` – Marks the row that should display the transmittal month
- `{{DateDay}}` – Marks the row that should display the transmittal day

If these tokens are not found, the engine falls back to detecting rows labeled `Year`, `Month`, and `Day` in column A (legacy mode).

**Engine behavior:**
1. Resolves date rows from tokens (or legacy labels if tokens are missing)
2. Creates/expands transmittal date columns based on `SummaryColumnData` formatting
3. Writes year/month/day values into the date rows for each transmittal
4. Populates the document matrix by matching document numbers and writing revision codes per transmittal
5. Populates the distribution matrix by matching recipients and writing copy counts (or formats) per transmittal
6. Inserts additional rows if the data exceeds template capacity

**Authoring tips:**
- Define all required named ranges (`SheetListData`, `DistributionListData`, `SummaryColumnData`)
- Place `{{DateYear}}`, `{{DateMonth}}`, and `{{DateDay}}` tokens on the rows where you want date values written
- Ensure these three date rows span the same column range as your transmittal date columns
- Apply the desired cell formatting (borders, alignment, number format) to the `SummaryColumnData` range; this formatting will be copied to all transmittal columns
- Format the template rows referenced by `SheetListData` and `DistributionListData` with the desired row style (fonts, borders, etc.)
- Use `{{DrgNumber}}`, `{{DrgName}}`, `{{DrgRev}}`, etc. tokens in the `SheetListData` row for dynamic document fields
- Use `{{PersonName}}`, `{{CompanyName}}`, etc. tokens in the `DistributionListData` row for dynamic recipient fields
- If using `TransmittalFormatData`, place it on a row that aligns with your transmittal date columns (typically above or below the date rows)
- Leave visual space around the matrix regions to accommodate row/column growth
{% endraw %}
### Master documents template (`MasterDocumentsList.xlsx`)

This template displays a chronological list of all documents across all transmittals in the project.

**Named range:**
- `MasterDocumentsList` or `MasterDocumentsListData` – Marks the template row for the master list

**Engine behavior:**
1. Applies global tokens to the workbook
2. Locates the master list template row using the named range
3. Sorts all transmittal items by transmittal date and ID
4. Writes one row per document/transmittal combination
5. Inserts additional rows as needed
{% raw %}
**Available tokens for the template row:**
Template rows can use any of the following tokens:
- Document fields: `{{DrgNumber}}`, `{{DrgRev}}`, `{{DrgName}}`, `{{DrgPaper}}`, `{{DrgScale}}`, `{{DrgDrawn}}`, `{{DrgChecked}}`
- Extended document fields: `{{DrgProj}}`, `{{DrgOriginator}}`, `{{DrgVolume}}`, `{{DrgLevel}}`, `{{DrgType}}`, `{{DrgRole}}`, `{{DrgStatus}}`, `{{DrgPackage}}`
- Transmittal fields: `{{TransID}}`, `{{TransDate}}`, `{{DateYear}}`, `{{DateMonth}}`, `{{DateDay}}`
- Project fields: `{{ProjectID}}`, `{{ProjectNumber}}`, `{{ProjectName}}`
{% endraw %}
**Authoring tips:**
- Define the `MasterDocumentsList` or `MasterDocumentsListData` named range on the template row
- Place tokens in the template row cells where you want document/transmittal data displayed
- Apply desired formatting (borders, fonts, alignment, number formats) to the template row; this formatting will be applied to all inserted rows
- The report is sorted chronologically with the transmittal date, so consider adding date columns for easy reference
- Consider using table formatting or banded rows for better readability of long lists

### Formatting and layout best practices

To make templates robust across future data volumes:
- Prefer unmerged cells in dynamic regions.
- Use consistent number/date formats on dynamic columns.
- Keep print titles/repeating header rows configured in Excel if required for paper/PDF output.
- Test with both small and large transmittals to confirm row growth behavior.
- Keep a copy of the original sample template and iterate from that baseline.

### Practical workflow for custom templates

1. Copy sample `.xlsx` templates to your shared reports folder.
2. Apply branding/layout changes first (logo, colors, fonts, title blocks).
3. Keep required headers/anchors intact.
4. Add or move tokens where needed.
5. Generate each report from Transmittal and validate output.
6. Repeat until formatting and data placement are correct.

### Common issues to check

If data does not appear where expected:
- Confirm exact template filename (`ProjectDirectory.xlsx`, etc.).
- Confirm the file is in the configured report templates folder.
- Verify required headers are still present and not converted to images/shapes.
- Verify named ranges (if used) still exist and refer to valid worksheet ranges.
- Check that dynamic sections are not fully merged/protected in a way that blocks row operations.

### Creating and managing named ranges in Excel

Named ranges are essential for OpenXML templates. To create or modify them:

1. **Creating a named range:**
   - Select the cell or range you want to name
   - In the Name Box (to the left of the formula bar), type the range name
   - Press Enter
   
   Or:
   - Select the cell or range
   - Go to Formulas tab > Define Name
   - Enter the name and click OK

2. **Viewing existing named ranges:**
   - Go to Formulas tab > Name Manager
   - All defined names in the workbook will be listed

3. **Named range naming rules:**
   - Must start with a letter or underscore
   - Cannot contain spaces (use camelCase or underscores)
   - Cannot conflict with cell references (e.g., avoid "A1")
   - Are case-insensitive

4. **Best practices:**
   - For single-row templates, name just the first cell of the template row
   - For multi-row templates, name the entire range
   - Use descriptive names that match the expected data purpose
   - Keep names consistent with the documented naming conventions

### Token reference summary

{% raw %}
**Global tokens** (available in all templates):
- `{{Project}}` or `{{ProjectDisplay}}` – Full project display name
- `{{ProjectName}}` – Project name only
- `{{ProjectNumber}}` – Project number only
- `{{ProjectIdentifier}}` or `{{ProjectID}}` – Project identifier
- `{{ClientName}}` – Client name
- `{{ReportTitle}}` – Report title (auto-generated)
- `{{ReportDate}}` – Report generation date
- `{{TransmittalDate}}` – Transmittal date (when applicable)

**Document/Drawing tokens** (available in transmittal and master lists):
- `{{DrgNumber}}` – Document/drawing number
- `{{DrgRev}}` – Document revision
- `{{DrgName}}` – Document name/description
- `{{DrgPaper}}` – Paper size
- `{{DrgScale}}` – Drawing scale
- `{{DrgDrawn}}` – Drawn by
- `{{DrgChecked}}` – Checked by
- `{{DrgProj}}`, `{{DrgOriginator}}`, `{{DrgVolume}}`, `{{DrgLevel}}`, `{{DrgType}}`, `{{DrgRole}}`, `{{DrgStatus}}`, `{{DrgPackage}}` – Extended document metadata

**Distribution tokens** (available in transmittal reports):
- `{{PersonName}}` or `{{ContactName}}` – Full name of recipient
- `{{FirstName}}` – First name
- `{{LastName}}` – Last name
- `{{CompanyName}}` – Company name
- `{{Email}}` – Email address
- `{{Tel}}` – Telephone number
- `{{Mobile}}` – Mobile number
- `{{Role}}` – Person's role/position
- `{{TransFormat}}` – Transmittal format (e.g., "PDF", "Email")
- `{{TransCopies}}` – Number of copies

**Date tokens** (special use in summary templates):
- `{{DateYear}}` – Year value
- `{{DateMonth}}` – Month value
- `{{DateDay}}` – Day value
{% endraw %}



