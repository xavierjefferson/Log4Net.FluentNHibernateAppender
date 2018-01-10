using System;
using System.Configuration;
using FluentNHibernate.Cfg.Db;
using NHibernate.Driver;

namespace Snork.FluentNHibernateTools
{
    public sealed class FluentNHibernatePersistenceBuilder
    {
        private static T ConfigureProvider<T, U>(Func<PersistenceConfiguration<T, U>> createFunc,
            string connectionString, string defaultSchema) where T : PersistenceConfiguration<T, U>
            where U : ConnectionStringBuilder, new()
        {
            var provider = createFunc().ConnectionString(connectionString);

            if (!string.IsNullOrWhiteSpace(defaultSchema))
            {
                provider.DefaultSchema(defaultSchema);
            }
            return provider;
        }

        /// <summary>
        ///     Factory method.  Return simple configuration info based on the given provider type, connection string, and default
        ///     schema
        /// </summary>
        /// <param name="nameOrConnectionString">Connection string or its name</param>
        /// <param name="providerType">Provider type from enumeration</param>
        /// <param name="options"></param>
        public static ConfigurationInfo Build(ProviderTypeEnum providerType, string nameOrConnectionString,
            FluentNHibernatePersistenceBuilderOptions options = null)
        {
            options = options ?? new FluentNHibernatePersistenceBuilderOptions();
            var configurer = GetPersistenceConfigurer(providerType, nameOrConnectionString, options);
            return new ConfigurationInfo(configurer, options, providerType);
        }

        private static string GetConnectionString(string nameOrConnectionString)
        {
            if (IsConnectionString(nameOrConnectionString))
            {
                return nameOrConnectionString;
            }

            if (IsConnectionStringInConfiguration(nameOrConnectionString))
            {
                return ConfigurationManager.ConnectionStrings[nameOrConnectionString].ConnectionString;
            }

            throw new ArgumentException(
                $"Could not find connection string with name '{nameOrConnectionString}' in application config file");
        }


        private static bool IsConnectionString(string nameOrConnectionString)
        {
            return nameOrConnectionString.Contains(";");
        }

        private static bool IsConnectionStringInConfiguration(string connectionStringName)
        {
            var connectionStringSetting = ConfigurationManager.ConnectionStrings[connectionStringName];

            return connectionStringSetting != null;
        }


        /// <summary>
        ///     Return an NHibernate persistence configurerTells the bootstrapper to use a FluentNHibernate provider as a job
        ///     storage,
        ///     that can be accessed using the given connection string or
        ///     its name.
        /// </summary>
        /// <param name="nameOrConnectionString">Connection string or its name</param>
        /// <param name="providerType">Provider type from enumeration</param>
        /// <param name="options"></param>
        public static IPersistenceConfigurer GetPersistenceConfigurer(ProviderTypeEnum providerType,
            string nameOrConnectionString, FluentNHibernatePersistenceBuilderOptions options = null)
        {
            options = options ?? new FluentNHibernatePersistenceBuilderOptions();
            var connectionString = GetConnectionString(nameOrConnectionString);

            IPersistenceConfigurer configurer;
            switch (providerType)
            {
                case ProviderTypeEnum.JetDriver:
                    configurer = ConfigureProvider(() => JetDriverConfiguration.Standard, connectionString,
                        options.DefaultSchema);
                    break;
                case ProviderTypeEnum.MsSqlCe40:
                    configurer = ConfigureProvider(() => MsSqlCeConfiguration.MsSqlCe40, connectionString,
                        options.DefaultSchema);
                    break;

                case ProviderTypeEnum.OracleClient10Managed:
                    configurer = ConfigureProvider(() => OracleClientConfiguration.Oracle10, connectionString,
                            options.DefaultSchema)
                        .Driver<OracleManagedDataClientDriver>();

                    break;
                case ProviderTypeEnum.OracleClient9Managed:
                    configurer = ConfigureProvider(() => OracleClientConfiguration.Oracle9, connectionString,
                            options.DefaultSchema)
                        .Driver<OracleManagedDataClientDriver>();
                    break;

                case ProviderTypeEnum.OracleClient10:
                    configurer = ConfigureProvider(() => OracleClientConfiguration.Oracle10, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.OracleClient9:
                    configurer = ConfigureProvider(() => OracleClientConfiguration.Oracle9, connectionString,
                        options.DefaultSchema);
                    break;
                case ProviderTypeEnum.PostgreSQLStandard:
                    configurer = ConfigureProvider(() => PostgreSQLConfiguration.Standard, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.PostgreSQL81:
                    configurer = ConfigureProvider(() => PostgreSQLConfiguration.PostgreSQL81, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.PostgreSQL82:
                    configurer = ConfigureProvider(() => PostgreSQLConfiguration.PostgreSQL82, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.Firebird:
                    configurer = ConfigureProvider(() => new FirebirdConfiguration(), connectionString,
                        options.DefaultSchema);

                    break;

                case ProviderTypeEnum.DB2Informix1150:
                    configurer = ConfigureProvider(() => DB2Configuration.Informix1150, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.DB2Standard:
                    configurer = ConfigureProvider(() => DB2Configuration.Standard, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.MySQL:
                    configurer = ConfigureProvider(() => MySQLConfiguration.Standard, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.MsSql2008:
                    configurer = ConfigureProvider(() => MsSqlConfiguration.MsSql2008, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.MsSql2012:
                    configurer = ConfigureProvider(() => MsSqlConfiguration.MsSql2012, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.MsSql2005:
                    configurer = ConfigureProvider(() => MsSqlConfiguration.MsSql2005, connectionString,
                        options.DefaultSchema);

                    break;
                case ProviderTypeEnum.MsSql2000:
                    configurer = ConfigureProvider(() => MsSqlConfiguration.MsSql2000, connectionString,
                        options.DefaultSchema);

                    break;
                default:
                    throw new ArgumentException("type");
            }
            return configurer;
        }
    }
}