using FluentNHibernate.Cfg.Db;

namespace Snork.FluentNHibernateTools
{
    public class ConfigurationInfo
    {
        internal ConfigurationInfo(IPersistenceConfigurer configurer, FluentNHibernatePersistenceBuilderOptions options,
            ProviderTypeEnum providerType)
        {
            PersistenceConfigurer = configurer;
            ProviderType = providerType;
            Options = options;
        }

        public FluentNHibernatePersistenceBuilderOptions Options { get; }
        public ProviderTypeEnum ProviderType { get; }
        public IPersistenceConfigurer PersistenceConfigurer { get; }
    }
}