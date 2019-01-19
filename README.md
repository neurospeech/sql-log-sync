# SQL Backup/Log Synchronization
SQL Backup/Log synchronization

1. Takes full backup at 12 in the night
2. Takes log backup every 5 minute
3. Compresses backup
4. Uploads backup to Azure Storage
5. Works with SQL Express, Web

# Installation

1. Build and Publish it on local folder
2. Change config to update connection string to database and azure storage
3. Change config to ignore database names you don't want to take backup  