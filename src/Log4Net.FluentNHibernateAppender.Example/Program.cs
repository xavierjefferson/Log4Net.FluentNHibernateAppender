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
