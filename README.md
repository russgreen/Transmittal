# Transmittal
![Revit Version](https://img.shields.io/badge/Revit%20Version-2021_--_2024-blue.svg) ![.NET](https://img.shields.io/badge/.NET-4.8-blue.svg) <br>
![Revit Version](https://img.shields.io/badge/Revit%20Version-2025_--_2026-blue.svg) ![.NET](https://img.shields.io/badge/.NET-8-blue.svg)

![GitHub last commit](https://img.shields.io/github/last-commit/russgreen/transmittal) 


Transmittal adds a wizard interface to Revit to assist with setting revisions and the status of drawings sheets and in publishing selected sheets to PDF, DWF, DWG formats.  Transmittal can also record the transmittal history for a project in a SQLite database and produce transmittal reports and historic document issue sheets.

To use Transmittal with Revit 2021 the freeware PDF24 printer must be installed.  This is not required for Revit 2022 and later. The PDF24 printer can be downloaded from https://download.pdf24.org/pdf24-creator-11.11.1-x64.msi

![Screenshot 2022-06-06 064221](https://user-images.githubusercontent.com/1886088/172102241-c7e597ad-ac73-45c0-ad63-7f65f5f0eddb.png)

<a href="https://russgreen.github.io/Transmittal/"><img src="https://img.shields.io/badge/-READ%20MORE-blue" /></a>

## Building the Solution

The UI used Syncfusion controls and so a license key should be obtained (free community versions are available). The key should be stored in a text file in the route of the repo called SyncfusionKey.txt (excluded from Git). 
 
Version numbering is manually by editing Directory.Build.props <VersionPrefix> and <VersionSuffix> properties prior to a release build.

NUKE should be called when in the main branch to build the solution and create the installer.  

Major.Minor.Build

InformationalVersion	$(Version) (this is a combination of VersionPrefix and VersionSuffix)
Assembly Version        $(VersionPrefix).0
Assembly File Version   $(VersionPrefix).0
