using NHibernate;

namespace Snork.FluentNHibernateTools
{
    public class SessionFactoryInfo
    {
        public SessionFactoryInfo(string key, ISessionFactory sessionFactory, ProviderTypeEnum providerType,
            FluentNHibernatePersistenceBuilderOptions options)
        {
            Key = key;
            SessionFactory = sessionFactory;
            Options = options;
            ProviderType = providerType;
        }

        public ProviderTypeEnum ProviderType { get; }
        public FluentNHibernatePersistenceBuilderOptions Options { get; }

        public ISessionFactory SessionFactory { get; }
        public string Key { get; }
    }
}