using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;
using FluentNHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;

namespace Snork.FluentNHibernateTools
{
    public static class SessionFactoryBuilder
    {
        private static readonly object Mutex = new object();

        private static readonly Dictionary<string, SessionFactoryInfo> _sessionFactoryInfos =
            new Dictionary<string, SessionFactoryInfo>();

        private static string CalculateMD5Hash(string input)

        {
            // step 1, calculate MD5 hash from input

            var md5 = MD5.Create();

            var inputBytes = Encoding.ASCII.GetBytes(input);

            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string

            var sb = new StringBuilder();

            for (var i = 0; i < hash.Length; i++)

            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }

        public static SessionFactoryInfo GetByKey(string key)
        {
            return _sessionFactoryInfos[key];
        }

        public static SessionFactoryInfo GetFromAssemblyOf<T>(ProviderTypeEnum providerType,
            string nameOrConnectionString,
            FluentNHibernatePersistenceBuilderOptions options)
        {
            var keyInfo = new {providerType, nameOrConnectionString, options};
            var key = CalculateMD5Hash(new JavaScriptSerializer().Serialize(keyInfo));
            lock (Mutex)
            {
                if (!_sessionFactoryInfos.ContainsKey(key))
                {
                    var configurationInfo = FluentNHibernatePersistenceBuilder.Build(providerType,
                        nameOrConnectionString, options);

                    var fluentConfiguration = Fluently.Configure()
                        .Mappings(x => x.FluentMappings.AddFromAssemblyOf<T>())
                        .Database(configurationInfo.PersistenceConfigurer);
                    fluentConfiguration.ExposeConfiguration(cfg =>
                    {
                        if (options.UpdateSchema)
                        {
                            var a = new SchemaUpdate(cfg);
                            using (var stringWriter = new StringWriter())
                            {
                                try
                                {
                                    a.Execute(i => stringWriter.WriteLine(i), true);
                                }
                                catch (Exception ex)
                                {
                                    throw;
                                }
                                var d = stringWriter.ToString();
                            }
                        }
                    });
                    fluentConfiguration.BuildConfiguration();
                    _sessionFactoryInfos[key] = new SessionFactoryInfo(key, fluentConfiguration.BuildSessionFactory(),
                        providerType, options);
                }
                return _sessionFactoryInfos[key];
            }
        }
    }
}