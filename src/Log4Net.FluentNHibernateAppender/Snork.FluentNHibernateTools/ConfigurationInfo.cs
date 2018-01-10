using FluentNHibernate.Cfg.Db;

namespace Snork.FluentNHibernateTools
{
    public class ConfigurationInfo
    {
        internal ConfigurationInfo(IPersistenceConfigurer configurer, string defaultSchema,
            ProviderTypeEnum providerType)
        {
            PersistenceConfigurer = configurer;
            ProviderType = providerType;
            DefaultSchema = defaultSchema;
        }

        public string DefaultSchema { get; }
        public ProviderTypeEnum ProviderType { get; }
        public IPersistenceConfigurer PersistenceConfigurer { get; }
    }
}