using System;

namespace Log4Net.FluentNHibernateAppender
{
    public class LogEntry
    {
        public virtual string StackTrace { get; set; }
        public virtual int Id { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string Thread { get; set; }

        public virtual string Level { get; set; }
        public virtual string Logger { get; set; }
        public virtual string Method { get; set; }

        public virtual string Message { get; set; }
        public virtual string Exception { get; set; }

        public virtual string MachineName { get; set; }
        public virtual string Domain { get; set; }
        public virtual string UserName { get; set; }
    }
}