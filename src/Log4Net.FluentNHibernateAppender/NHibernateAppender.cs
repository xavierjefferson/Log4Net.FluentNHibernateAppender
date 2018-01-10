using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using log4net.Appender;
using log4net.Core;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Util;
using Snork.FluentNHibernateTools;

namespace Log4Net.FluentNHibernateAppender
{
    public class NHibernateAppender : AppenderSkeleton
    {
        private readonly string _cacheKey = Guid.NewGuid().ToString();
        private readonly IPersistenceConfigurer _configurer;

        private readonly Dictionary<IPersistenceConfigurer, ISessionFactory> _sessionFactories =
            new Dictionary<IPersistenceConfigurer, ISessionFactory>();

        private bool _testBuild;


        public NHibernateAppender(IPersistenceConfigurer persistenceConfigurer)
        {
            _configurer = persistenceConfigurer;
        }

        public static NHibernateAppender Configure(ProviderTypeEnum providerType, string nameOrConnectionString,
            string appenderName, string defaultSchema = null)
        {
            var tmp = FluentNHibernatePersistenceBuilder.Build(providerType,
                nameOrConnectionString, defaultSchema);
            return Configure(tmp.PersistenceConfigurer, appenderName);
        }

        public static NHibernateAppender Configure(IPersistenceConfigurer persistenceConfigurer, string appenderName)
        {
            var appender = new NHibernateAppender(persistenceConfigurer)
            {
                Name = appenderName
            };
            return appender;
        }

        private ISessionFactory GetSessionFactory(IPersistenceConfigurer configurer)
        {
            //SINGLETON!
            if (_sessionFactories.ContainsKey(configurer) && _sessionFactories[configurer] != null)
            {
                return _sessionFactories[configurer];
            }
            var mappings = GetMappings();
            var fluentConfiguration = Fluently.Configure().Mappings(mappings);

            _sessionFactories[configurer] = fluentConfiguration
                .Database(configurer)
                .BuildSessionFactory();
            return _sessionFactories[configurer];
        }

        private IPersistenceConfigurer GetConfigurer()
        {
            return _configurer;
        }

        public IStatelessSession GetStatelessSession(IPersistenceConfigurer c)
        {
            return GetSessionFactory(c).OpenStatelessSession();
        }

        public IStatelessSession GetStatelessSession()
        {
            DoTestBuild();
            return GetSessionFactory(GetConfigurer()).OpenStatelessSession();
        }

        public ISession GetSession()
        {
            DoTestBuild();
            return GetSessionFactory(GetConfigurer()).OpenSession();
        }

        private void DoTestBuild()
        {
            if (!_testBuild)
            {
                BuildSchemaIfNeeded(GetConfigurer());
                _testBuild = true;
            }
        }

        public void BuildSchemaIfNeeded(IPersistenceConfigurer configurer)
        {
            var mappings = GetMappings();
            var fluentConfiguration = Fluently.Configure()
                .Database(configurer)
                .Mappings(mappings)
                .ExposeConfiguration(cfg =>
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
                        var c = stringWriter.ToString();
                    }
                })
                .BuildConfiguration();
        }

        private Action<MappingConfiguration> GetMappings()
        {
            return x => x.FluentMappings.AddFromAssemblyOf<LogEntryMap>();
        }


        protected override void Append(LoggingEvent loggingEvent)
        {
            Append(new[] {loggingEvent});
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            try
            {
                var cache = MemoryCache.Default;
                var queue = cache[_cacheKey] as Queue<LoggingEvent>;
                if (queue == null)
                {
                    queue = new Queue<LoggingEvent>(loggingEvents);
                }
                else
                {
                    foreach (var loggingEvent in loggingEvents)
                    {
                        if (queue.Count < 1000)
                        {
                            queue.Enqueue(loggingEvent);
                        }
                    }
                }
                using (var statelessSession = GetStatelessSession())
                {
                    LoggingEvent loggingEvent = null;
                    try
                    {
                        while (queue.Any())
                        {
                            loggingEvent = queue.Dequeue();

                            statelessSession.Insert(new LogEntry
                            {
                                StackTrace =
                                    loggingEvent.ExceptionObject?.StackTrace,
                                Date = loggingEvent.TimeStamp,
                                Thread = loggingEvent.ThreadName,
                                Level = loggingEvent.Level.ToString(),
                                Logger = loggingEvent.LoggerName,
                                Method = loggingEvent.LocationInformation.MethodName,
                                Message = loggingEvent.RenderedMessage,
                                Exception = loggingEvent.GetExceptionString(),
                                MachineName = Environment.MachineName,
                                Domain = loggingEvent.Domain,
                                UserName = MonoDetection.IsRunningMono ? null : loggingEvent.UserName
                            });
                            loggingEvent = null;
                        }
                    }
                    catch (Exception)
                    {
                        if (loggingEvent != null)
                        {
                            queue.Enqueue(loggingEvent);
                        }
                        cache.Set(_cacheKey, queue, DateTimeOffset.Now.AddMinutes(5));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Issue with {0} : {1}",this.GetType().Name, ex.Message);
            }
        }
    }
}