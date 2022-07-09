---
layout: page
title: Update database
permalink: /settings/databaseupdate
---
Database files created using Transmittal versions prior to 1.2.0 are missing a column from the settings table that will result in this error when trying to save settings.

![image](https://user-images.githubusercontent.com/1886088/178101409-41e3a247-1d88-43f6-994a-d724ba411f44.png)

To fix this open the database file in [DB Browser for SQLite](https://sqlitebrowser.org/) by clicking Open Database then browsing for the location where the .tdb file is stored.  Change the file type filter on the Choose a database file dialog to All files (*)

![image](https://user-images.githubusercontent.com/1886088/178101522-9c0798c2-de4f-47ee-b623-b30a5689aca1.png)

Right click on the Settings table and click Modify Table

![image](https://user-images.githubusercontent.com/1886088/178101647-e648e24b-0099-4c74-9685-57b6e97b0949.png)

Add a TEXT column called ClientName

![image](https://user-images.githubusercontent.com/1886088/178101691-6dd4c523-0626-4a96-bf55-0ea4d5a13caa.png)

Close DB Browser and try saving your settings again.
