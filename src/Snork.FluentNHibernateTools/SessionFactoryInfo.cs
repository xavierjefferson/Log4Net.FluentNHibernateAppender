using NHibernate;

namespace Snork.FluentNHibernateTools
{
    public class SessionFactoryInfo
    {
        public SessionFactoryInfo(string key, ISessionFactory sessionFactory)
        {
            Key = key;
            SessionFactory = sessionFactory;
        }

        public ISessionFactory SessionFactory { get; }
        public string Key { get; }
    }
}