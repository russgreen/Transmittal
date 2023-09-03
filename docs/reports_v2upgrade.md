---
layout: page
title: Upgrade Reports to V2
permalink: /settings/reports_v2upgrade/
---
Transmittal reports in version 2 of the app can now use an additional parameter to identify a work package of the report. This could be used so sort the documents in the reports by work package. If you have existing reports that you would like to upgrade to use this feature, you can do so by following these steps:
1. Open the report in a text editor e.g. Notepad, [VS Code](https://code.visualstudio.com/)
2. Navigate to the XML element ```<DataSets>```. In TransmittalSummary.rdlc find ```<DataSet Name="dsSummaryItems">```, and in TransmittalSheet.rdlc find ```<DataSet Name="dsTransmittalItems">```.
3. Add the following field after DrgStatus
```
<Field Name="DrgPackage">
    <DataField>DrgPackage</DataField>
    <rd:TypeName>System.String</rd:TypeName>
</Field>
```
![Screenshot 2023-09-03 090822](https://github.com/russgreen/Transmittal/assets/1886088/1ea084c8-a57a-423c-9f67-40cce6ef3c58)

4.
