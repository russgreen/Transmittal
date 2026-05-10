---
layout: page
title: "Settings"
description: "Configure Transmittal settings for file naming, ISO19650 compliance, database management, and custom parameters in Revit projects"
permalink: /settings/
---
Settings for Transmittal are stored in the Autodesk® Revit® model in extensible storage. If multiple models are used on a project then settings need to be configured correctly in each model file.  If Transmittal is being used without Revit then some settings can be edited from the Transmittal.Desktop settings window.  If Revit is being used then settings can only be configured via the Revit addin.

The settings window is organized into three tabs: **Project Settings**, **Database / Reports**, and **Shared Parameters**.

## Project Settings

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

### CDE filename filter/rule (Revit addin only)
When this options is selected copies of the exported sheet files are made using a separate filename rule that can conform to project CDE requirements and making it easier to follow internal and external filename standards. If the output location rule includes the <Format> tag then the format is replaced with "CDE".  If the <Format> tag is not used then an CDE sub-folder is created in the output path. e.g.
```
Rule:   P:\ProjectFolder\Output\<Format>\<DateYY><DateMM><DateDD>
Folder: P:\ProjectFolder\Output\CDE\220702

Rule:   P:\ProjectFolder\Output
Folder: P:\ProjectFolder\Output\CDE

Rule:   P:\ProjectFolder\Output\<Format>\<Package>\<DateYY><DateMM><DateDD>
Folder: P:\ProjectFolder\Output\CDE\220702

Rule:   P:\ProjectFolder\Output\<Format>\<SheetCollection>\<DateYY><DateMM><DateDD>
Folder: P:\ProjectFolder\Output\CDE\220702

Rule:   P:\ProjectFolder\Output\<DateYY><DateMM><DateDD>
Folder: P:\ProjectFolder\Output\220702\CDE
```

### CDE Output location (Revit addin only)
When this option is select the CDE copies are saved to the specified location. 

### Use file transfer service (Revit addin only)
Enable this option to automatically transfer exported files using a file transfer service after the transmittal is created.

### File transfer service (Revit addin only)
Select the file transfer service to use for automatically uploading exported files. Available options depend on your configuration and installed services.

### Issue Formats and Document Statuses
Editable lists of transmittal issue format codes and document status codes used in the transmittal process. These lists can be customized to match your project requirements:
- **Issue Formats**: Define the codes and descriptions for different transmittal types (e.g., "S1" for "Preliminary", "S2" for "Suitable for Coordination")
- **Document Statuses**: Define the codes and descriptions for document status codes used on drawings

## Database / Reports 

### Use Project Database (Revit addin only)
Enables transmittals to be recorded so transmittal issue sheets can be generated. When enabled, a "Load settings from database" button becomes available to load previously saved settings from the project database.
<!--
### Template database
The template database defaults to the C:\Program Files\Transmittal\Data folder.-->

### Project database (Revit addin only) 
Location of the project database file. Each project should have its own database but where multiple Revit files are used on a project they should all be set to use the same database.  

The database engine used by Transmittal is [SQLite](https://www.sqlite.org/index.html) which allows concurrent reads of the database but not concurrent writes.  

if required the database can be opened and edited in [DB Browser for SQLite](https://sqlitebrowser.org/).

### Report templates
The default report templates are embedded in the application but it is possible to customise the report templates and share them on a network location. [For more information on reports](/Transmittal/settings/reports/).

### Issue sheet store
The path to the folder where transmittal reports should be saved when exported from the report viewer.

### Directory store
The path to the folder where the project directory report should be saved when exported from the report viewer.

### Report Document Codes and Numbers (Revit addin only)
Configure the document type codes and starting document numbers for automatically generated reports. These settings control the naming convention for report documents:

- **Project Directory**: Document type code and starting number for project directory reports
- **Transmittal Sheet**: Document type code and starting number for individual transmittal sheets
- **Transmittal Summary**: Document type code and starting number for transmittal summary reports
- **Master Document List**: Document type code and starting number for master document list reports

The starting numbers will be incremented automatically as new reports are generated, ensuring unique document numbers for each report.

## Shared Parameters (Revit addin only)
### Add standard transmittal parameters to your project
Adds the default shared parameters required by Transmittal into your project. These can be added into a Revit template to avoid the need to do these on all new projects. The shared parameters are supplied in C:\Program Files\Transmittal\Resources\TransmittalParameters.txt so that can be manually edited as required.

This button is disabled when "Use your own custom shared parameters" is enabled.

### Use your own custom shared parameters
Where shared parameters are already in use the GUID's of the parameters can be entered in each box. Transmittal will then use the existing parameters instead of the defaults. These settings can be configured and saved into a Revit template.

The following shared parameter GUIDs can be configured:

**Project Information Category Parameters:**
- **Project Identifier**: Unique identifier for the project
- **Originator**: Organization or company originating the documents
- **Role**: Role code for the document originator

**Sheet Category Parameters:**
- **Sheet Volume / Functional**: Volume or functional breakdown parameter
- **Sheet Level / Spatial**: Level or spatial breakdown parameter
- **Document Type**: Type of document (e.g., drawing, specification)
- **Sheet Status Code**: Current status code of the sheet
- **Sheet Status Description**: Description of the current status
- **Sheet Package**: Package or collection that the sheet belongs to
