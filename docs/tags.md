# Tags in name filters

Transmittal allows folder and file naming rules to be stored as filters made up of text with optional tags.  The available tags are listed below.

Tag strings are case sensitive

| Tag | File | Folder | Value replace from |
| :--- | :---: | :---: | :--- |
| \<ProjNo> | :heavy_check_mark: |  | The Revit project number paramater |
| \<ProjId> | :heavy_check_mark: |  | Custom parameter for ISO19650 project ID where this needs to be different from the project number (Project Information) |
| \<Originator> | :heavy_check_mark: |  | Custom parameter for ISO19650 originator (Project Information) |
| \<Volume> | :heavy_check_mark: |  | Custom parameter for ISO19650 volume (Sheet) |
| \<Level> | :heavy_check_mark: |  | Custom parameter for ISO19650 level (Sheet) |
| \<Type> | :heavy_check_mark: |  | Custom parameter for ISO19650 type (Sheet) |
| \<Role> | :heavy_check_mark: |  | Custom parameter for ISO19650 role (Project Information) |
| \<ProjName> | :heavy_check_mark: |  | The Revit project name paramater  |
| \<SheetNo> | :heavy_check_mark: |  | The Revit sheet number paramater  |
| \<SheetName> | :heavy_check_mark: |  | The Revit sheet name paramater in PascalCase |
| \<SheetName2> | :heavy_check_mark: |  | The Revit sheet name paramater unchanged |
| \<Status> | :heavy_check_mark: |  | Custom parameter for ISO19650 status code (Sheet)  |
| \<StatusDescription> | :heavy_check_mark: |  | Custom parameter for status description  (Sheet)  |
| \<Rev> | :heavy_check_mark: |  | The Revit current sheet revision value |
| \<DateYY> | :heavy_check_mark: | :heavy_check_mark: | The issue date as a 2 digit year  |
| \<DateYYYY> | :heavy_check_mark: | :heavy_check_mark: | The issue date as a 4 digit year |
| \<DateMM> | :heavy_check_mark: | :heavy_check_mark: | The issue date as a 2 digit month number |
| \<DateDD> | :heavy_check_mark: | :heavy_check_mark: | The issue date as a 2 digit day of the month |
| \<Format> |  | :heavy_check_mark: | The format of the export e.g PDF, DWG, DWF  |
