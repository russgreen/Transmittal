---
layout: page
title: Tags in name filters
permalink: /settings/tags/
---
Transmittal allows folder and file naming rules to be stored as filters made up of text with optional tags.  The available tags are listed below.

Tag strings are case sensitive

| Tag | File | Folder | Value replace from |
| :--- | :---: | :---: | :--- |
| \<ProjNo> | &#10003; |  | The Revit project number paramater |
| \<ProjId> | &#10003; |  | Custom parameter for ISO19650 project ID where this needs to be different from the project number (Project Information) |
| \<Originator> | &#10003; |  | Custom parameter for ISO19650 originator (Project Information) |
| \<Volume> | &#10003; |  | Custom parameter for ISO19650 volume (Sheet) |
| \<Level> | &#10003; |  | Custom parameter for ISO19650 level (Sheet) |
| \<Type> | &#10003; |  | Custom parameter for ISO19650 type (Sheet) |
| \<Role> | &#10003; |  | Custom parameter for ISO19650 role (Project Information) |
| \<ProjName> | &#10003; |  | The Revit project name paramater  |
| \<SheetNo> | &#10003; |  | The Revit sheet number paramater  |
| \<SheetName> | &#10003; |  | The Revit sheet name paramater in PascalCase |
| \<SheetName2> | &#10003; |  | The Revit sheet name paramater unchanged |
| \<Status> | &#10003; |  | Custom parameter for ISO19650 status code (Sheet)  |
| \<StatusDescription> | &#10003; |  | Custom parameter for status description  (Sheet)  |
| \<Rev> | &#10003; |  | The Revit current sheet revision value |
| \<DateYY> | &#10003; | &#10003; | The issue date as a 2 digit year  |
| \<DateYYYY> | &#10003; | &#10003; | The issue date as a 4 digit year |
| \<DateMM> | &#10003; | &#10003; | The issue date as a 2 digit month number |
| \<DateDD> | &#10003; | &#10003; | The issue date as a 2 digit day of the month |
| \<Format> |  | &#10003; | The format of the export e.g PDF, DWG, DWF  |
| \<Package> |  | &#10003; |  Custom parameter for package (Sheet)  |
| \<SheetCollection> |  | &#10003; | The sheet collection name (Revit 2025+)  |
| %UserProfile% |  | &#10003; | The path to use the users profile folder.  e.g. C:\Users\my_username  |
| %OneDriveConsumer% |  | &#10003; | The path to use the users private One Drive folder.  e.g. C:\Users\my_username\OneDrive  |
| %OneDriveCommercial% |  | &#10003; | The path to use the users commercial One Drive folder.  e.g. C:\Users\my_username\OneDrive - COMPANY |