# Log4Net.FluentNHibernateAppender Storage - An Implementation for MS SQL Server, MySQL, PostgreSQL, Oracle, Firebird, and DB/2
[![Latest version](https://img.shields.io/nuget/v/Log4Net.FluentNHibernateAppender.svg)](https://www.nuget.org/packages/Log4Net.FluentNHibernateAppender/) 

A simple-to-configure FluentNHibernate storage implementation for [Log4Net] (http://logging.apache.org/log4net/). Supports  MS SQL Server, MySQL, PostgreSQL, Oracle, Firebird, DB/2, SQLite, MS Access (Jet), and SQL Server Compact Edition.


Run the following command in the NuGet Package Manager console to install Hangfire.FluentNHibernateStorage:

```
Install-Package Log4Net.FluentNHibernateAppender
```
The Log4Net and FluentNHibernate packages will be installed as dependencies.

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
Be advised that two of the four Oracle options (OracleClient9Managed, OracleClient10Managed) use the **Oracle.ManagedDataAccess** client library internally, while the other two (OracleClient9, OracleClient10) use the **System.Data.OracleClient** client library.  I'm not Oracle savvy, and I could only get **Oracle.ManagedDataAccess** to work properly (the other is NHibernate's default).  You may have a different experience. 


## Configuration Code  

As-is, this will create the appropriate tables in your database but there's a little more you'll need to do (read below) so you won't get more in the logs than you actually want.  This example would log to a MySQL database (and would required the MySQL.Data package installed), but you can easily point it to other databases.
```
using System;
using System.Configuration;
using System.Transactions;
using Log4Net.FluentNHibernateAppender;
using Snork.FluentNHibernateTools;

namespace Log4Net.FluentNHibernateAppender.SampleApplication
{
    public class DemoClass
    {
		//copy this snippet to each class you want to log from
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static void Main(string[] args)
        {
		    //do this configuration ONCE in your application (unless you're logging to multiple databases). 
			NHibernateAppender.Configure(ProviderTypeEnum.MySQL,
                ConfigurationManager.ConnectionStrings["mysql"].ConnectionString, 
				"mysqlappender").AddToRootHierarchy();
            
        }
    }

	public class AnotherClass 
	{
	    //copy of snippet
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public static void SomeMethod(){
			log.Info("Now I'm logging from another class.'")
		}
	}
}
```

## Addition Configuration Needed

### Step 1 / 2
In your AssemblyInfo.cs file, you'll need to add this snippet so Log4Net will read its config info from either your app.config or web.config file:

```
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
```
The config info could be read from a different file, and you can read about how to do that [here] (https://www.codeproject.com/Articles/34287/log-net-C-Code-Snippets).

### Step 2 / 2
You need to make a few changes to your app.config or web.config file:

 1. Add a `configSections` entry
 2. Add a `log4net` section
 3. In the `log4net` section, add the two `logger` entries as shown below.  This will keep NHibernate from spewing its internal logging into your database (unless you *want* that!).
 4. In the `log4net` section, add a `root` entry that sets your default minimum log level (DEBUG, INFO, WARN, ERROR).
 5. You don't need to add an `appender`-type entry because the code above is doing that for you.  But you're free to add more - follow the examples shown [here] (https://logging.apache.org/log4net/release/config-examples.html).

```
<configuration>
  <configSections>
    <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net"/>
  </configSections>
  <log4net>
	<logger name="NHibernate">
      <level value="ERROR" />
    </logger>
    <logger name="NHibernate.SQL">
      <level value="ERROR" />
    </logger>
	<root>
      <level value="INFO" />
    </root>
  </log4net>
  ```
## Consuming the Log from Inside Your Application, and Deleting Entries

To be able to delete entries or read the log from within your code, you need to keep a reference to the instance of `NHibernateAppender` that you create.  In the example below, that reference is being kept around as `_appender`.  Sample code is below.

### GetCount() method
The GetCount() method will tell you how many entries are in the log.

### GetEntriesFromIQueryable method
This method will read entries from the log.  You will pass a LINQ expression to specify which ones you want.  Don't get too fancy, because certain expressions (especially any with inline code) will not work with the underlying NHibernate ORM.  You can also use .Skip and .Take to paginate your results.

### DeleteEntries method
This method will delete entries from the log.  Again, you'll pass a LINQ expression to specify.

```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snork.FluentNHibernateTools;

namespace Log4Net.FluentNHibernateAppender.Example
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static NHibernateAppender _appender;

        static void Main(string[] args)
        {
            //Configure using Sqlite
            var sqliteFile = System.IO.Path.Combine(System.Environment.CurrentDirectory, "log.db");
            var connectionString = String.Format(@"Data Source={0};Version=3;", sqliteFile);

            _appender = NHibernateAppender.Configure(ProviderTypeEnum.SQLite,
                connectionString,
                "MyAppender").AddToRootHierarchy();

            for (int t = 0; t < 10; t++)
            {
                log.InfoFormat("Adding line {0} to appender", t);
            }

            //get count of entries
            var count = _appender.GetEntryCount();
            Console.WriteLine("Appender has {0} entries", count);

            //get ALL entries.  Use a LINQ expression to specify
            List<LogEntry> entries1 = _appender.GetEntriesFromIQueryable(i => i.ToList());
            foreach (var entry in entries1)
            {
                Console.WriteLine("Timestamp = {0} Message = {1}", entry.TimeStamp, entry.Message);
            }

            //get entries.  Use a LINQ expression to specify
            List<LogEntry> entries2 = _appender.GetEntriesFromIQueryable(i => i.Where(j => j.Level == "INFO").ToList());
            foreach (var entry in entries2)
            {
                Console.WriteLine("Timestamp = {0} Message = {1}", entry.TimeStamp, entry.Message);
            }

            //delete entries.  Use a LINQ expression to specify
            _appender.DeleteEntries(i=>i.TimeStamp < DateTime.Now);
        }
    }
}
```