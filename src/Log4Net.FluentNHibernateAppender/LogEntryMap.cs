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
            Map(i => i.TimeStamp).Column("`TimeStamp`").Not.Nullable();
            Map(i => i.ThreadName).Column("`ThreadName`").Length(64).Not.Nullable();
            Map(i => i.Level).Column("`Level`").Length(10).Not.Nullable();
            Map(i => i.LoggerName).Column("`Logger`").Length(512).Not.Nullable();
            Map(i => i.MethodName).Column("`Method`").Length(200).Not.Nullable();
            Map(i => i.Message).Column("`Message`").Length(1000).Not.Nullable();
            Map(i => i.Exception).Column("`Exception`").Length(4000).Not.Nullable();
            Map(i => i.StackTrace).Column("`StackTrace`").Length(4000).Nullable();
            Map(i => i.MachineName).Column("`MachineName`").Length(64).Not.Nullable();
            Map(i => i.Domain).Column("`Domain`").Length(64).Not.Nullable();
            Map(i => i.UserName).Column("`UserName`").Length(64).Nullable();
            Map(i => i.ClassName).Column("ClassName").Nullable();
            Map(i => i.LineNumber).Column("LineNumber").Length(64).Nullable();
            Map(i => i.FileName).Column("FileName").Length(255).Nullable();
        }
    }
}