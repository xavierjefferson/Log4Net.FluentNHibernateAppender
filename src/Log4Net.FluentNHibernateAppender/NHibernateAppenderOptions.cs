using Snork.FluentNHibernateTools;

namespace Log4Net.FluentNHibernateAppender
{
    public class NHibernateAppenderOptions : FluentNHibernatePersistenceBuilderOptions
    {
        public int MaxRetries { get; set; } = 10;
    }
}