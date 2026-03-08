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

If a template is missing from the configured report folder, Transmittal falls back to the embedded/default report layout for that engine.

Report files can be edited in either [Syncfusion Standalone Report Designer](https://www.boldreports.com/standalone-report-designer) or [Microsoft Report Builder](https://www.microsoft.com/en-us/download/details.aspx?id=53613). Both tools are free.

Microsoft Report builder only appears to support RDL files.  However, if you right click on an RDLC file and choose Open with > Choose another app, the Report Builder can be chosen and the RDLC files can be opened, edited and saved.

![image](https://user-images.githubusercontent.com/1886088/175804963-6846002a-68fc-49c8-9987-27133c3c7763.png)

## OpenXML (Excel) template contract

The OpenXML engine supports a simple token + header detection contract to make templates customisable while still tolerant of layout changes.

### Supported text tokens

These are replaced in all worksheet text cells:
- {{Project}}
- {{ProjectDisplay}}
- {{ProjectName}}
- {{ProjectNumber}}
- {{ProjectIdentifier}}
- {{ClientName}}
- {{ReportTitle}}
- {{ReportDate}}
- {{TransmittalDate}}

### Project directory template

- Optional named range support:
	- ProjectDirectory
	- ProjectDirectoryRange
- If a named range is present, rows are written into that range and expanded when needed.
- If no named range is present, the engine falls back to header detection (Name / Company / Email / Tel / Role style columns).

### Transmittal sheet template

- Items table is located by header keywords such as Drawing / Document / Rev.
- Distribution table is located by headers such as Format / Copies (or Company / Person variants).
- The engine clears and rewrites data rows while preserving surrounding formatting/layout.

### Transmittal summary template

- Supports multiple summary layouts (including top or bottom distribution blocks) by detecting section anchors.
- Date matrix columns are inferred from Year / Month / Day rows.
- Document matrix rows are mapped by drawing/document number and populated per transmittal column.
- Distribution matrix rows are mapped by recipient, with per-transmittal cells populated from distribution entries.
- If there are more rows than template capacity, rows are inserted to preserve complete output.

### Master documents template

- Table location is found via Date / Transmittal / Document-style headers.
- Data rows are appended in transmittal-date order.

Instructions for upgrading older reports to the latest version of the report template that supports version 2.* can be found [here](/Transmittal/settings/reports_v2upgrade/)