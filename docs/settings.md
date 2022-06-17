---
layout: page
title: Settings
permalink: /settings/
---
## Basic Settings

Settings for Transmittal are stored in the Revit model in extensisible storage. If multiple models are used on a project then settings need to be configured correctly in each model file.

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

### Use extranet
Feature not enabled yet. See [roadmap](https://github.com/russgreen/Transmittal#roadmap).

### Issue Formats and Document Statuses
Lists of transmittal types and status codes used in the transmittal process

## Database
### Use Project Database 
Enables transmittals to be recorded so transmittal issue sheets can be generated. 

### Template database
The template database defaults to the C:\Program Files\Transmittal\Data folder.  At the current time this should be left.

### Project database 
Location of the project database file. Each project should have its own database but where multiple Revit files are used on a project they should all be set to use the same database.  

The database engine used by Transmittal is [SQLite](https://www.sqlite.org/index.html) which allows concurrent reads of the dataabse but not concurrent writes.  To protect the database from this during write operations a lock file is generated in the same location as the database. The lock file should only exist momentarily.

if required the database can be opened and edited in [DB Browser for SQLite](https://sqlitebrowser.org/).

### Report templates
The default report templates are embedded in the application but it is possible to customise the report templates and share them on a network location. [For more information on reports](/Transmittal/settings/reports/).

## Advanced Settings
