---
layout: page
title: "Revit Add-in"
description: "Transmittal Revit add-in for managing drawiing revisions and multi-format export"
permalink: /revit-addin/
---
Transmittal for Autodesk® Revit® provides a wizard style UI to export drawing sheets to PDF, DWG or DWF while optionally recording the transmittal in database for the purpose of reporting.
<img src="../assets/images/TransmittalAddinPage1.png" width="850" >

Selected sheets can have revisions and status values set. 

New revisions in the model can be created from within Transmittal while setting the revisions to selected sheets.
![image](https://user-images.githubusercontent.com/1886088/176995586-5ae120ed-89dc-44fe-a017-f2e5b4b9082f.png)

Sheet status can be set. Both the status code and status description parameter values will be set.
![image](https://user-images.githubusercontent.com/1886088/176995607-054e6236-2352-420d-b5ff-0ec5cec5ce1b.png)

Select the export formats and the export options for each format.
![image](https://user-images.githubusercontent.com/1886088/176995613-9f557596-013f-4e5b-aa43-ec3ddf33f2fc.png)

Select the recipients from the available project directory membrs on the left side and set copies of format of transmittal before adding to the transmittal on the right side.  Unticking Record issue and generate issue sheet will still allow the sheets to be exported to the selected formats but simply not record the transmittal in the database. Uses of this might be draft internal issues for review.
![image](https://user-images.githubusercontent.com/1886088/176995621-01552177-3b57-43a0-8ea6-3c79c19dfa90.png)

After clicking Finish the progress will be displayed.

If the transmittal was recorded in the database the Transmittal Sheet report will be displayed.
![image](https://user-images.githubusercontent.com/1886088/176995644-b5646a98-d28b-421d-986b-6b92be854c6e.png)

Sample transmittal reports: 
- [Sample Report 1](https://github.com/russgreen/Transmittal/blob/f418fa67eaad57dc255b875e8f9a25bd31aad8c7/SampleReports/Alternative%20Sample%201/TransmittalSheet.pdf)
- [Sample Report 2](https://github.com/russgreen/Transmittal/blob/f418fa67eaad57dc255b875e8f9a25bd31aad8c7/SampleReports/Alternative%20Sample%202/TransmittalSheet.pdf)


Exporting the report will result in it being automatically named and saved to the Issue Sheet Store configured in Settings.

![image](https://user-images.githubusercontent.com/1886088/178138968-4c3bbb61-646c-4297-9fb6-587450b135a2.png)

To use Transmittal with Revit 2021 the freeware PDF24 printer must be installed.  This is not required for Revit 2022 and later. The PDF24 printer can be downloaded from [PDF24.org](https://download.pdf24.org/pdf24-creator-11.11.1-x64.msi)

