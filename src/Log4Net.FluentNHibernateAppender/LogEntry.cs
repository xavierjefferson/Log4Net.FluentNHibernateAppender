using System;

namespace Log4Net.FluentNHibernateAppender
{
    public class LogEntry
    {
        public virtual string StackTrace { get; set; }
        public virtual int Id { get; set; }
        public virtual DateTime TimeStamp { get; set; }
        public virtual string ThreadName { get; set; }

        public virtual string Level { get; set; }
        public virtual string LoggerName { get; set; }
        public virtual string MethodName { get; set; }

        public virtual string Message { get; set; }
        public virtual string Exception { get; set; }

        public virtual string MachineName { get; set; }
        public virtual string Domain { get; set; }
        public virtual string UserName { get; set; }
        public virtual string LineNumber { get; set; }
        public virtual string FileName { get; set; }
        public virtual string ClassName { get; set; }
    }
}