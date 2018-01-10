using FluentNHibernate.Mapping;

namespace Log4Net.FluentNHibernateAppender
{
    public class LogEntryMap : ClassMap<LogEntry>
    {
        public LogEntryMap()
        {
            Table("LogEntry");
            LazyLoad();
            Id(i => i.Id).GeneratedBy.Identity().Unique().Column("`Id`");
            Map(i => i.Date).Column("`Date`").Not.Nullable();
            Map(i => i.Thread).Column("`Thread`").Length(64).Not.Nullable();
            Map(i => i.Level).Column("`Level`").Length(10).Not.Nullable();
            Map(i => i.Logger).Column("`Logger`").Length(512).Not.Nullable();
            Map(i => i.Method).Column("`Method`").Length(200).Not.Nullable();
            Map(i => i.Message).Column("`Message`").Length(1000).Not.Nullable();
            Map(i => i.Exception).Column("`Exception`").Length(4000).Not.Nullable();
            Map(i => i.StackTrace).Column("`StackTrace`").Length(4000).Nullable();
            Map(i => i.MachineName).Column("`MachineName`").Length(64).Not.Nullable();
            Map(i => i.Domain).Column("`Domain`").Length(64).Not.Nullable();
            Map(i => i.UserName).Column("`UserName`").Length(64).Nullable();
        }
    }
}