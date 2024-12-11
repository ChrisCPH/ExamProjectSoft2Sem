### .NET

.NET SDK https://dotnet.microsoft.com/en-us/download
 
### RabbitMQ

Download chocolatey

run in cmd as admin: choco install rabbitmq
https://www.rabbitmq.com/docs/install-windows#chocolatey 

### Database

Create databases and tables using /Setup/DB_Scripts.sql

create db user with db_owner permissions from the /Setup/DB_User.sql file

If it doesn't work check these things:

Enable TCP/IP in SQL: https://www.youtube.com/watch?v=Yi9bTbGHznM

Right click the server in ssms and click properties -> security and change from Windows Authentication mode to SQL Server and Windows Authentication mode