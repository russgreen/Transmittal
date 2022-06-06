# Transmittal
![Revit Version](https://img.shields.io/badge/Revit%20Version-2022_--_2023-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)
![.NET](https://img.shields.io/badge/.NET-4.8-blue.svg)
[![AppStore](https://img.shields.io/badge/Autodesk-AppStore-blue)](https://apps.autodesk.com/en/Publisher/PublisherHomepage?ID=200910140805021)

![GitHub last commit](https://img.shields.io/github/last-commit/russgreen/transmittal)

Transmittal adds a wizard interface to Revit to assist with setting revisions and the status of drawings sheets and in publishing selected sheets to PDF, DWF, DWG formats.  Transmittal can also record the transmittal history for a project in a SQLite database and product transmittal reports and historic document issue sheets.

## Building the Solution

The UI used Syncfusion controls and so a license key should be obtained (free community versions are available). The key should be stored in a text file in the route of the repo called SyncfusionKey.txt (excluded from Git). 
 
Version numbering is manually by editing Directory.Build.props <VersionPrefix> and <VersionSuffix> properties prior to a release build.

Major.Minor.Build

InformationalVersion	$(Version) (this is a combination of VersionPrefix and VersionSuffix)
Assembly Version        $(VersionPrefix).0
Assembly File Version   $(VersionPrefix).0

## Roadmap

- [x] Migrate UI from older project - transmittal wizard UI, revisions, status, settings, project directory, transmittal history
- [x] WPF UI with .NET CommunityToolkit.MVVM
- [x] Build template SQLite database
- [x] Create ribbon buttons and test UI
- [x] Load projects sheets to datagrid
- [x] Set status of sheets
- [x] Create revisions
- [x] Apply revisions to sheets
- [x] Export files to PDF, DWG, etc with settings
- [x] Save and load settings in extensible storage.  
- [x] Create new SQLite database from template
- [x] Get current SharedParameter file....replace with custom file....set back to original
- [x] Create, edit, save project directory in database
- [x] Load project directory from db
- [x] Assign people to transmittal
- [x] Record transmittal in database
- [x] Merge and edit transmittal history
- [x] Display reports
- [x] Build and test installer
- [x] Desktop app to manage transmittal history outside of Revit and record issue of non-Revit documents
- [ ] Add people to directory from revit wizard
- [ ] Associate .tdb files with Transmittal.Desktop.exe so database files can be opened by double clicking
- [ ] Create transmittal wizard inside Transmittal.Desktop.exe for recording non-revit document transmittals
