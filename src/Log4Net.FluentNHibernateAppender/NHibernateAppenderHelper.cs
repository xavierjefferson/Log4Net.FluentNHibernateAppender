using log4net;
using log4net.Repository.Hierarchy;

namespace Log4Net.FluentNHibernateAppender
{
    public static class NHibernateAppenderHelper
    {
        public static NHibernateAppender AddToRootHierarchy(this NHibernateAppender appender)
        {
            var hierarchy = (Hierarchy) LogManager.GetRepository();
            appender.ActivateOptions();
            hierarchy.Root.AddAppender(appender);
            hierarchy.Configured = true;
            return appender;
        }
    }
}