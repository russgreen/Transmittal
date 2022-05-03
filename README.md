# Transmittal
![Revit Version](https://img.shields.io/badge/Revit%20Version-2022_--_2023-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-blue.svg)
![.NET](https://img.shields.io/badge/.NET-4.8-blue.svg)
[![AppStore](https://img.shields.io/badge/Autodesk-AppStore-blue)](https://apps.autodesk.com/en/Publisher/PublisherHomepage?ID=200910140805021)

![GitHub last commit](https://img.shields.io/github/last-commit/russgreen/IsolateWarnings)

## Building the Solution

The UI used Syncfusion controls and so a license key should be obtained (free community versions are available). The key should be stored in a text file in the route of the repo called SyncfusionKey.txt (excluded from Git). 
 
Version numbering is manually by editing Directory.Build.props <VersionPrefix> and <VersionSuffix> properties prior to a release build.

Major.Minor.Build

InformationalVersion	$(Version) (this is a combination of VersionPrefix and VersionSuffix)
Assembly Version        $(VersionPrefix).0
Assembly File Version   $(VersionPrefix).0

## Roadmap

- [x] Build UI - transmittal wizard UI, revisions, status, settings, project directory, transmittal history
- [x] Build template SQLite database
- [x] Create ribbon buttons and test UI
- [x] Load projects sheets to datagrid
- [x] Set status of sheets
- [x] Create revisions
- [x] Apply revisions to sheets
- [x] Export files to PDF, DWG, etc with settings
- [ ] Save and load settings in using shared parameters.  dbPath; filenamefilter; sharedparametersfilepath; parameters to use for StatusCode, StatusDescription, ???
- [ ] Save and load settings in extensible storage instead of parameters.  dbPath; filenamefilter; sharedparametersfilepath; parameters to use for StatusCode, StatusDescription, ???
- [ ] Check if model has already been configured to use transmittal
- [ ] Create new SQLite database from template
- [ ] Get current SharedParameter file....replace with custom file....set back to original
- [ ] Create, edit, save project directory in database
- [ ] Load project directory from db
- [ ] Assign users to transmittal
- [ ] Record transmittal in database
- [ ] Merge and edit transmittal history
- [ ] Display reports
- [ ] Build and test installer
- [ ] Desktop app to manage transmittal hiostory outside of Revit and issue non-Revit documents