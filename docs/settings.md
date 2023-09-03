---
layout: page
title: Settings
permalink: /settings/
---
## Basic Settings

Settings for Transmittal are stored in the Autodesk® Revit® model in extensible storage. If multiple models are used on a project then settings need to be configured correctly in each model file.  If Transmittal is being used without Revit then some settings can be edited from the Transmittal.Desktop settings window.  If Revit is being used then settings can only be configured via the Revit addin.

### Filename filter/rule
The rule used to construct the filename of exported sheet files.  The [available tags](/Transmittal/settings/tags) get replaced with parameter values. The filename rule should not include file extensions.

### Output location
Folder to save exported files. Can include dates and the format of the exported file using some of the [available tags](/Transmittal/settings/tags). 

### Date format string
Format of the date when creating new revisions.

### Enforce ISO19650 parameter values
Sheet parameters with missing values shown on the sheets list with red warning flags.  
![image](https://user-images.githubusercontent.com/1886088/173241746-1d1680d3-8e0b-4662-9726-86dbd51569e9.png)

Selecting this option will prevent sheets with missing values from being issued.

### Use extranet (Revit addin only)
When this options is selected copies of the exported sheet files are mading using a seperate filename rule that can conform to project extranet requirements and making it easier to follow internal and external filename standards. If the output location rule includes the <Format> tag then the format is replaced with "Extranet".  If the <Format> tag is not used then an Extranet sub-folder is created in the output path. e.g.
```
Rule:   P:\ProjectFolder\Output\<Format>\<DateYY><DateMM><DateDD>
Folder: P:\ProjectFolder\Output\Extranet\220702

Rule:   P:\ProjectFolder\Output\<DateYY><DateMM><DateDD>
Folder: P:\ProjectFolder\Output\220702\Extranet
```

### Issue Formats and Document Statuses
Lists of transmittal types and status codes used in the transmittal process

## Database
### Use Project Database (Revit addin only)
Enables transmittals to be recorded so transmittal issue sheets can be generated. 
<!--
### Template database
The template database defaults to the C:\Program Files\Transmittal\Data folder.-->

### Project database (Revit addin only) 
Location of the project database file. Each project should have its own database but where multiple Revit files are used on a project they should all be set to use the same database.  

The database engine used by Transmittal is [SQLite](https://www.sqlite.org/index.html) which allows concurrent reads of the database but not concurrent writes.  To protect the database from this during write operations a lock file is generated in the same location as the database. The lock file should only exist momentarily.

if required the database can be opened and edited in [DB Browser for SQLite](https://sqlitebrowser.org/).

### Report templates
The default report templates are embedded in the application but it is possible to customise the report templates and share them on a network location. [For more information on reports](/Transmittal/settings/reports/).

### Issue sheet store
The path to the folder where transmittal reports should be saved when exported from the report viewer.

### Directory store
The path to the folder where the project directory report should be saved when exported from the report viewer.

## Advanced Settings (Revit addin only)
### Add standard transmittal parameters to your project
Adds the default shared parameters required by Transmittal into your project. These can be added into a Revit template to avoid the need to do these on all new projects. The shared parameters are supplied in C:\Program Files\Transmittal\Resources\TransmittalParameters.txt so that can be manually edited as required.

### Use your own custom shared parameters
Where shared parameters are already in use the GUID's of the paramaters can be entered in each box. Transmittal will then use the existing paramaters instead of the defaults. These settings can be configured and saved into a Revit template.
