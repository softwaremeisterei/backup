## BACKUP - Developers Backup Utility (C# / WPF) 

***Special developer feature: ignoring of files in compliance with .gitignore***

***Main usecase***

- Add source folders
- Set options
- Choose the destination folder
- Start the backup

***What it does***

For each of the ***selected*** source folders a zip archive (applying *optimal* compression level) will be created in the destination folder.

The archive name will get tagged by a date suffix (e.g. foo-20200508.zip).

In case an archive of the same name already exists, another name will be chosen for the archive by appending a sequential number (e.g. foo-20200508-(1).zip)

The status bar will display the currently compressed files.

On completion the total execution time will be displayed in the status bar.

**The source folder list, options and destination folder remain persistent over sessions.**
      
***Screenshots***

![ScreenShot1](https://github.com/softwaremeisterei/backup/blob/master/screenshot.png?raw=true)
![ScreenShot2](https://github.com/softwaremeisterei/backup/blob/master/screenshot2.png?raw=true)
