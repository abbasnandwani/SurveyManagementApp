CREATE USER [surveymanagementpoc] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [surveymanagementpoc];
ALTER ROLE db_datawriter ADD MEMBER [surveymanagementpoc];
ALTER ROLE db_ddladmin ADD MEMBER [surveymanagementpoc];
GO


https://learn.microsoft.com/en-us/azure/app-service/tutorial-connect-msi-sql-database?tabs=windowsclient%2Cef%2Cdotnet