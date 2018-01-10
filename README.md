# Log4Net.FluentNHibernateAppender Storage - An Implementation for MS SQL Server, MySQL, PostgreSQL, Oracle, Firebird, and DB/2
[![Latest version](https://img.shields.io/nuget/v/Log4Net.FluentNHibernateAppender.svg)](https://www.nuget.org/packages/Log4Net.FluentNHibernateAppender/) 

A simple-to-configure FluentNHibernate storage implementation for Log4Net. Supports  MS SQL Server, MySQL, PostgreSQL, Oracle, Firebird, DB/2, SQLite, MS Access (Jet), and SQL Server Compact Edition.


Run the following command in the NuGet Package Manager console to install Hangfire.FluentNHibernateStorage:

```
Install-Package Log4Net.FluentNHibernateAppender
```

IMPORTANT!  For the use of any database other than SQL Server, you must also install an appropriate driver package:

### For MySQL:
```
Install-Package MySql.Data
```

### For PostgreSQL:
```
Install-Package Npgsql
```

### For Oracle Managed Provider):
```
Install-Package Oracle.ManagedDataAccess
```

### For Firebird:
```
Install-Package FirebirdSql.Data.FirebirdClient
```


## Database Implementation Notes
The package includes an enumeration of database providers AND their specific flavors of SQL across various SQL versions:
```
    public enum ProviderTypeEnum
    {
        None = 0,
      
        OracleClient10 = 3,
        OracleClient9 = 4,
        PostgreSQLStandard = 5,
        PostgreSQL81 = 6,
        PostgreSQL82 = 7,
        Firebird = 8,
       
        DB2Informix1150 = 10,
        DB2Standard = 11,
        MySQL = 12,
        MsSql2008 = 13,
        MsSql2012 = 14,
        MsSql2005 = 15,
        MsSql2000 = 16,
        OracleClient10Managed = 17,
        OracleClient9Managed = 18,
    }
```
The enumeration values correspond to the list of providers NHibernate supports.  When you instantiate a provider, you'll pass the best enumeration value to the FluentNHibernateStorageFactory.For method, along with your connection string.  I wrote it this way so you don't have to be concerned with the underlying implementation details for the various providers, which can be a little messy.  


### Oracle
Be advised that two of the four Oracle options (OracleClient9Managed, OracleClient10Managed) use the **Oracle.ManagedDataAccess** client library internally, while the other two (OracleClient9, OracleClient10) use the **System.Data.OracleClient** client library.  I'm not Oracle savvy, and I could only get **Oracle.ManagedDataAccess** to work properly (the other is NHibernate's default).  You may have a different experience.  This implementation was tested against Oracle 11g Express on Oracle Linux.


## Usage  
```
using System;
using System.Configuration;
using System.Transactions;
using Log4Net.FluentNHibernateAppender;

namespace Log4Net.FluentNHibernateAppender.SampleApplication
{
    public class DemoClass
    {
        private static BackgroundJobServer _backgroundJobServer;

        private static void Main(string[] args)
        {
			NHibernateAppender.Configure(ProviderTypeEnum.MySQL,
                ConfigurationManager.ConnectionStrings["mysql"].ConnectionString, "mysqlappender").AddToRootHierarchy();

        }
    }
}
```
