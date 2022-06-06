# Tags in name filters

Transmittal allows folder and file naming rules to be stored as filters made up of text with optional tags.  The available tags are listed below.

Tag strings are case sensitive

| Tag | File | Folder | 
Value replace from |
| :--- | :---: | :---: | :--- |
| <ProjNo> | &check; |  | The Revit project number paramater |
| <ProjId> | &check; |  | Custom parameter for ISO19650 project ID where this needs to be different from the project number (Project Information) |
| <Originator> | &check; |  | Custom parameter for ISO19650 originator (Project Information) |
| <Volume> | &check; |  | Custom parameter for ISO19650 volume (Sheet) |
| <Level> | &check; |  | Custom parameter for ISO19650 level (Sheet) |
| <Type> | &check; |  | Custom parameter for ISO19650 type (Sheet) |
| <Role> | &check; |  | Custom parameter for ISO19650 role (Project Information) |
| <ProjName> | &check; |  | The Revit project name paramater  |
| <SheetNo> | &check; |  | The Revit sheet number paramater  |
| <SheetName> | &check; |  | The Revit sheet name paramater in PascalCase |
| <SheetName2> | &check; |  | The Revit sheet name paramater unchanged |
| <Status> | &check; |  | Custom parameter for ISO19650 status code (Sheet)  |
| <StatusDescription> | &check; |  | Custom parameter for status description  (Sheet)  |
| <Rev> | &check; |  | The Revit current sheet revision value |
| <DateYY> | &check; | &check; | The issue date as a 2 digit year  |
| <DateYYYY> | &check; | &check; | The issue date as a 4 digit year |
| <DateMM> | &check; | &check; | The issue date as a 2 digit month number |
| <DateDD> | &check; | &check; | The issue date as a 2 digit day of the month |
| <Format> |  | &check; | The format of the export e.g PDF, DWG, DWF  |