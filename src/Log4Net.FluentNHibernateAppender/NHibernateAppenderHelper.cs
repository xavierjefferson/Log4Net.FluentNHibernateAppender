using FluentNHibernate.Cfg.Db;
using log4net;
using log4net.Repository.Hierarchy;
using Snork.FluentNHibernateTools;

namespace Log4Net.FluentNHibernateAppender
{
    public static class NHibernateAppenderHelper
    {
        public static NHibernateAppender AddToRootHierarchy(this NHibernateAppender appender)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();
           
            appender.ActivateOptions();
            hierarchy.Root.AddAppender(appender);
            hierarchy.Configured = true;
            return appender;
        }
        public static NHibernateAppender Configure(ProviderTypeEnum providerType, string nameOrConnectionString,
            string appenderName, string defaultSchema = null)
        {
            var tmp = Snork.FluentNHibernateTools.FluentNHibernatePersistenceBuilder.Build(providerType,
                nameOrConnectionString, defaultSchema);
            return Configure(tmp.PersistenceConfigurer, appenderName);
        }

        public static NHibernateAppender Configure(IPersistenceConfigurer persistenceConfigurer, string appenderName)
        {
            var hierarchy = (Hierarchy) LogManager.GetRepository();
            var appender = new NHibernateAppender(persistenceConfigurer)
            {
                Name = appenderName
            };
            appender.ActivateOptions();
            hierarchy.Root.AddAppender(appender);
            hierarchy.Configured = true;
            return appender.AddToRootHierarchy();
        }
    }
}

