using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using log4net.Appender;
using log4net.Core;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using Snork.FluentNHibernateTools;

namespace Log4Net.FluentNHibernateAppender
{
    public class NHibernateAppender : AppenderSkeleton
    {
        private readonly string _cacheKey = Guid.NewGuid().ToString();
        private readonly IPersistenceConfigurer _configurer;
        private readonly NHibernateAppenderOptions _options;

        private readonly Dictionary<IPersistenceConfigurer, ISessionFactory> _sessionFactories =
            new Dictionary<IPersistenceConfigurer, ISessionFactory>();

        private bool _testBuild;

        public NHibernateAppender(IPersistenceConfigurer persistenceConfigurer,
            NHibernateAppenderOptions options = null)

        {
            _options = options ?? new NHibernateAppenderOptions();
            _configurer = persistenceConfigurer;
        }

        public static NHibernateAppender Configure(ProviderTypeEnum providerType, string nameOrConnectionString,
            string appenderName, NHibernateAppenderOptions options = null)
        {
            options = options ?? new NHibernateAppenderOptions();
            var tmp = FluentNHibernatePersistenceBuilder.Build(providerType,
                nameOrConnectionString, options);
            return Configure(tmp.PersistenceConfigurer, appenderName, options);
        }

        public static NHibernateAppender Configure(IPersistenceConfigurer persistenceConfigurer, string appenderName,
            NHibernateAppenderOptions options = null)
        {
            options = options ?? new NHibernateAppenderOptions();
            var appender = new NHibernateAppender(persistenceConfigurer, options)
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

        public int GetEntryCount()
        {
            using (var statelessSession = GetStatelessSession())
            {
                return statelessSession.Query<LogEntry>().Count();
            }
        }

        public void DeleteEntries(Expression<Func<LogEntry, bool>> matchExpression)
        {
            using (var statelessSession = GetStatelessSession())
            {
                var ids = statelessSession.Query<LogEntry>().Where(matchExpression).Select(i => i.Id).ToList();
                var qry = string.Format("delete from `{0}` where {1} in (:ids)", nameof(LogEntry), nameof(LogEntry.Id));
                for (var batchIndex = 0; batchIndex < ids.Count; batchIndex += 100)
                {
                    statelessSession.CreateQuery(qry)
                        .SetParameterList("ids", ids.Skip(batchIndex).Take(100))
                        .ExecuteUpdate();
                }
            }
        }

        public T GetEntriesFromIQueryable<T>(Func<IQueryable<LogEntry>, T> transformFunc)
        {
            using (var statelessSession = GetStatelessSession())
            {
                return transformFunc(statelessSession.Query<LogEntry>());
            }
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            try
            {
                var cache = MemoryCache.Default;
                var queue = cache[_cacheKey] as Queue<WrappedEvent>;
                if (queue == null)
                {
                    queue = new Queue<WrappedEvent>(loggingEvents.Select(i => new WrappedEvent {LoggingEvent = i}));
                }
                else
                {
                    foreach (var loggingEvent in loggingEvents)
                    {
                        if (queue.Count < 1000)
                        {
                            queue.Enqueue(new WrappedEvent {LoggingEvent = loggingEvent});
                        }
                    }
                }
                using (var statelessSession = GetStatelessSession())
                {
                    WrappedEvent wrappedEvent = null;
                    try
                    {
                        while (queue.Any())
                        {
                            wrappedEvent = queue.Dequeue();
                            var logEntry = new LogEntry
                            {
                                StackTrace =
                                    wrappedEvent.LoggingEvent.ExceptionObject?.StackTrace,
                                TimeStamp = wrappedEvent.LoggingEvent.TimeStamp,
                                ThreadName = wrappedEvent.LoggingEvent.ThreadName,
                                Level = wrappedEvent.LoggingEvent.Level.ToString(),
                                LoggerName = wrappedEvent.LoggingEvent.LoggerName,
                                MethodName = wrappedEvent.LoggingEvent.LocationInformation?.MethodName,
                                LineNumber = wrappedEvent.LoggingEvent.LocationInformation?.LineNumber,
                                ClassName = wrappedEvent.LoggingEvent.LocationInformation?.ClassName,
                                FileName = wrappedEvent.LoggingEvent.LocationInformation?.FileName,
                                Message = wrappedEvent.LoggingEvent.RenderedMessage,
                                Exception = wrappedEvent.LoggingEvent.GetExceptionString(),
                                MachineName = Environment.MachineName,
                                Domain = wrappedEvent.LoggingEvent.Domain,
                                UserName = MonoDetection.IsRunningMono ? null : wrappedEvent.LoggingEvent.UserName
                            };

                            statelessSession.Insert(logEntry);
                            wrappedEvent = null;
                        }
                    }
                    catch (Exception)
                    {
                        if (wrappedEvent != null)
                        {
                            wrappedEvent.RetryCount++;
                            if (wrappedEvent.RetryCount < _options.MaxRetries)
                            {
                                queue.Enqueue(wrappedEvent);
                            }
                        }
                        cache.Set(_cacheKey, queue, DateTimeOffset.Now.AddMinutes(5));
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Issue with {0} : {1}", GetType().Name, ex.Message);
            }
        }

        private class WrappedEvent
        {
            public int RetryCount { get; set; }
            public LoggingEvent LoggingEvent { get; set; }
        }
    }
}