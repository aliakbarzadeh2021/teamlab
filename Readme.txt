
                                           TeamLab v5.2 

General Information 

TeamLab v5.2 source code can be found here: http://sourceforge.net/projects/teamlab/

License information regarding third-party code can be found in the License folder.

To compile and run TeamLab you´ll need Microsoft .NET Framework 3.5 SP1 installed.

To get TeamLab running on your local machine follow the instructions below: 
    •    Build TeamLab source code with the use of \redistributable\asc\BuildAndDeploy.bat
    •    Run TeamLab core service with integrated web-server located at \_ci\deploy\service\TeamLabSvc.exe
    •    Browse to your TeamLab portal at http://localhost:8082/ 
    •    Follow the wizard instructions to enter the administrator data. 

NOTE! 

TeamLab has the page visit tracking feature enabled. To track page visits, a 1-px image is loaded
to every page of the portal from our server at the following address: https://track.teamlab.com/stat/onepixel.gif?src=surceforge&ver=2.0.3&page={current-page}
We only need this to get data about the number of created pages and page visits.  
To disable the tracking, please go \web.studio\web.appsettings.config and set the stat.enable key to false.  


----------------------------------------------------------------------------------

Updating TeamLab 3.1 to TeamLab 5.2

Step 1. Updating Database Structure

The database structure of the new TeamLab v5.2 has changed. To update your databases, follow the
instructions below depending on the database type you use to store the portal data – SQLite or MySQL.

Updating SQLite Databases

You'll need to backup the portal data stored in SQLite databases first. To create the backup, copy the
following database files to a safe place:
    • Core Database File
            services\core.db3
    • Web Studio Database File
            web.studio\App_Data\WebStudio\webstatistic.db3
            web.studio\App_Data\WebStudio\webstudio.db3
    • Blogs Database File
            web.studio\Products\Community\Modules\Blogs\App_Data\ASC.Blogs.Data.db3
    • Bookmarks Database File
            web.studio\Products\Community\Modules\Bookmarking\App_Data\BookmarkingDB\bookmarking.db3
    • Forums Database File
            web.studio\Products\Community\Modules\Forum\App_Data\ASC.Forum.Database.db3• Events Database File
    • Events Database File
            web.studio\Products\Community\Modules\News\App_Data\feeds.db3
    • Photos Database File
            web.studio\Products\Community\Modules\PhotoManager\App_Data\images.db3
    • Wiki Database File
            web.studio\Products\Community\Modules\Wiki\App_Data\wiki.db3
    • Projects Database File
            web.studio\Products\Projects\App_Data\ASC.Projects.db3
    • Documents Database File
            web.studio\Products\Files\App_Data\ASC.Files.db3

Once the backup is performed, update database structure. To do that, follow these steps:
    1. Go to SQL_Scripts\SQLite\Update_3.1_to_5.2
    2. In the selected directory, run the following scripts consecutively:
        • Core.sql to update the services\core.db3 database structure
        • Blogs.sql to update the web.studio\Products\Community\Modules\Blogs\App_Data\ASC.Blogs.Data.db3 database structure
        • Files.sql to update the web.studio\Products\Files\App_Data\ASC.Files.db3 database structure

Updating MySQL Databases

First, backup the MySQL TeamLab database using one of the available ways.
Next, update the database structure. To do that, go to 
SQL_Scripts\MySql\Update_3.1_to_5.2 and run Core.sql, Blogs.sql and Files.sql scripts.

Step 2. Updating Software Version

After you have updated the database structure (see Step 1 above), proceed with updating the TeamLab version:
    1. Build the Teamlab v5.2 project by running the .bat file located at \redistributable\asc\BuildAndDeploy.bat.
       This will create 2 folders with files in the following directory: _ci\deploy\
    2. Replace your current TeamLab version files by the newely created ones from _ci\deploy\

ATTENTION: if you use SQLite to store the portal data, ignore replacing the following database files:

    • services\core.db3
    • web.studio\App_Data\WebStudio\webstatistic.db3
    • web.studio\App_Data\WebStudio\webstudio.db3
    • web.studio\Products\Community\Modules\Blogs\App_Data\ASC.Blogs.Data.db3
    • web.studio\Products\Community\Modules\Bookmarking\App_Data\BookmarkingDB\bookmarking.db3
    • web.studio\Products\Community\Modules\Forum\App_Data\ASC.Forum.Database.db3
    • web.studio\Products\Community\Modules\News\App_Data\feeds.db3• web.studio\Products\Community\Modules\PhotoManager\App_Data\images.db3
    • web.studio\Products\Community\Modules\Wiki\App_Data\wiki.db3
    • web.studio\Products\Projects\App_Data\ASC.Projects.db3
    • web.studio\Products\Files\App_Data\ASC.Files.db3

These are the database files with all your portal data. If you replace them, the portal data will be lost.
If the portal data are stored in the MySQL database, the above mentioned files can be copied with no problem.


----------------------------------------------------------------------------------

Updating TeamLab 2.2 to TeamLab 3.1

Step 1. Updating Database Structure

The database structure of the new TeamLab v3.1 has changed. To update your databases, follow the
instructions below depending on the database type you use to store the portal data – SQLite or MySQL.

Updating SQLite Databases

You'll need to backup the portal data stored in SQLite databases first. To create the backup, copy the
following database files to a safe place:
    • Core Database File
            services\core.db3
    • Web Studio Database File
            web.studio\App_Data\WebStudio\webstatistic.db3
            web.studio\App_Data\WebStudio\webstudio.db3
    • Blogs Database File
            web.studio\Products\Community\Modules\Blogs\App_Data\ASC.Blogs.Data.db3
    • Bookmarks Database File
            web.studio\Products\Community\Modules\Bookmarking\App_Data\BookmarkingDB\bookmarking.db3
    • Forums Database File
            web.studio\Products\Community\Modules\Forum\App_Data\ASC.Forum.Database.db3• Events Database File
            web.studio\Products\Community\Modules\News\App_Data\feeds.db3
    • Photos Database File
            web.studio\Products\Community\Modules\PhotoManager\App_Data\images.db3
    • Wiki Database File
            web.studio\Products\Community\Modules\Wiki\App_Data\wiki.db3
    • Projects Database File
            web.studio\Products\Projects\App_Data\ASC.Projects.db3
    • Documents Database File
            web.studio\Products\Files\App_Data\ASC.Files.db3

Once the backup is performed, update the core, web studio and project database structure. To do that, follow these steps:
    1. Go to SQL_Scripts\SQLite\Update_2.2_to_3.1
    2. In the selected directory, run the following scripts consecutively:
        • Core.sql to update the services\core.db3 database structure
        • Projects.sql to update the web.studio\Products\Projects\App_Data\ASC.Projects.db3 database structure
        • Bookmarking.sql to update the web.studio\Products\Community\Modules\Bookmarking\App_Data\BookmarkingDB\bookmarking.db3 database structure
        • Events.sql to update the web.studio\Products\Community\Modules\News\App_Data\feeds.db3 database structure
        • Forum.sql to update the web.studio\Products\Community\Modules\Forum\App_Data\ASC.Forum.Database.db3 database structure
        • Files.sql to update the web.studio\Products\Files\App_Data\ASC.Files.db3 database structure

Updating MySQL Databases

First, backup the MySQL TeamLab database using one of the available ways.
Next, update the core, web studio and project database structure. To do that, go to 
SQL_Scripts\MySql\Update_2.2_to_3.1 and run Core.sql, Projects.sql, Bookmarking.sql, Events.sql, Forum.sql and Files.sql scripts.

Step 2. Updating Software Version

After you have updated the database structure (see Step 1 above), proceed with updating the TeamLab version:
    1. Build the Teamlab v3.1 project by running the .bat file located at \redistributable\asc\BuildAndDeploy.bat.
	   This will create 2 folders with files in the following directory: _ci\deploy\
    2. Replace your current TeamLab version files by the newely created ones from _ci\deploy\

ATTENTION: if you use SQLite to store the portal data, ignore replacing the following database files:

    • services\core.db3
    • web.studio\App_Data\WebStudio\webstatistic.db3
    • web.studio\App_Data\WebStudio\webstudio.db3
    • web.studio\Products\Community\Modules\Blogs\App_Data\ASC.Blogs.Data.db3
    • web.studio\Products\Community\Modules\Bookmarking\App_Data\BookmarkingDB\bookmarking.db3
    • web.studio\Products\Community\Modules\Forum\App_Data\ASC.Forum.Database.db3
    • web.studio\Products\Community\Modules\News\App_Data\feeds.db3• web.studio\Products\Community\Modules\PhotoManager\App_Data\images.db3
    • web.studio\Products\Community\Modules\Wiki\App_Data\wiki.db3
    • web.studio\Products\Projects\App_Data\ASC.Projects.db3

These are the database files with all your portal data. If you replace them, the portal data will be lost.
If the portal data are stored in the MySQL database, the above mentioned files can be copied with no problem.


----------------------------------------------------------------------------------

Updating TeamLab 2.0 to TeamLab 2.2 

Step 1. Updating Database Structure 

The database structure of the new TeamLab v2.2 has changed. To update your databases, follow the instructions
below depending on the database type you use to store the portal data – SQLite or MySQL.

Updating SQLite Databases 

You´ll need to backup the portal data stored in SQLite databases first. To create the backup, copy the following database files to a safe place: 
    •    Core Database File 
         services\coredb.db 
    •    Web Studio Database File 
         web.studio\App_Data\WebStudio\webstatistic.db3 
         web.studio\App_Data\WebStudio\webstudio.db3 
    •    Blogs Database File          
         web.studio\Products\Community\Modules\Blogs\App_Data\ASC.Blogs.Data.db3 
    •    Bookmarks Database File   
         web.studio\Products\Community\Modules\Bookmarking\App_Data\BookmarkingDB\bookmarking.db3 
    •    Forums Database File 
         web.studio\Products\Community\Modules\Forum\App_Data\ASC.Forum.Database.db3
    •    Photos Database File 
         web.studio\Products\Community\Modules\PhotoManager\App_Data\images.db3 
    •    Projects Database File 
         web.studio\Products\Projects\App_Data\ASC.Projects.db3  

Once the backup is performed, update the core, web studio and project database structure. To do that, follow these steps: 
          1.   Go SQL_Scripts\SQLite\Update_2.0_to_2.2 
          2.   In the selected directory, run the  Core.sql  script. This will update the  coredb.db  database structure. 
          3.   Rename the coredb.db database file at services\coredb.db to core.db3 
          4.   In the same SQL_Scripts\SQLite\Update_2.0_to_2.2 directory, run the WebStudio.sql script.  
               This will update the structure of the webstudio.db3 database at web.studio\App_Data\WebStudio\webstudio.db3 
          5.   In the same directory, run the Projects.sql script. This will update the structure of the ASC.Projects.db3 database 
			   at web.studio\Products\Projects\App_Data\ASC.Projects.db3

Updating MySQL Databases 
       
First, backup the MySQL TeamLab database using one of the available ways. 
Next, update the core, web studio and project database structure. To do that, go SQL_Scripts\MySql\Update_2.0_to_2.2 and run the Core.sql, WebStudio.sql and Projects.sql scripts.

Step 2. Updating Software Version 

After you have updated the database structure (see  Step 1  above), proceed to updating the TeamLab version: 
     1.   Build the Teamlab v2.2 project by running the .bat file located at \redistributable\asc\BuildAndDeploy.bat
	      This will create 2 folders with files in the following directory: _ci\deploy\  
     2.   Replace your current TeamLab version files by the newely created ones from _ci\deploy\ 


ATTENTION: if you use SQLite to store the portal data, ignore replacing the following database files:
     •   services\core.db3 
     •   web.studio\App_Data\WebStudio\webstatistic.db3 
     •   web.studio\App_Data\WebStudio\webstudio.db3 
     •   web.studio\Products\Community\Modules\Blogs\App_Data\ASC.Blogs.Data.db3 
     •   web.studio\Products\Community\Modules\Bookmarking\App_Data\BookmarkingDB\bookmarking.db3  
     •   web.studio\Products\Community\Modules\Forum\App_Data\ASC.Forum.Database.db3  
     •   web.studio\Products\Community\Modules\PhotoManager\App_Data\images.db3  
     •   web.studio\Products\Projects\App_Data\ASC.Projects.db3  

These are the database files with all your portal data. If you replace them, the portal data will be lost. 

If the portal data are stored in the  MySQL  database,  the above mentioned files can be copied with no problem. 
