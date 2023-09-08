---
layout: page
title: Customise Reports
permalink: /settings/reports/
---
Reports used by the addin default to embedded report templates.  Reports can be customised and placed in a shared location where multiple users can use the same report templates.

Sample report templates can be found in C:\Program Files\Transmittal\Reports when installed or on the [Github repo](https://github.com/russgreen/Transmittal/tree/main/SampleReports).

Report names __must__ be:
- ProjectDirectory.rdlc
- TransmittalSheet.rdlc
- TransmittalSummary.rdlc

Report files can be edited in either [Syncfusion Standalone Report Designer](https://www.boldreports.com/standalone-report-designer) or [Microsoft Report Builder](https://www.microsoft.com/en-us/download/details.aspx?id=53613). Both tools are free.

Microsoft Report builder only appears to support RDL files.  However, if you right click on an RDLC file and choose Open with > Choose another app, the Report Builder can be chosen and the RDLC files can be opened, edited and saved.

![image](https://user-images.githubusercontent.com/1886088/175804963-6846002a-68fc-49c8-9987-27133c3c7763.png)

Instructions for upgrading older reports to the latest version of the report template that supports version 2.* can be found [here](/Transmittal/settings/reports_v2upgrade/)