namespace Snork.FluentNHibernateTools
{
    public class FluentNHibernatePersistenceBuilderOptions
    {
        public string DefaultSchema { get; set; }
        public bool UpdateSchema { get; set; }

        public FluentNHibernatePersistenceBuilderOptions()
        {
            UpdateSchema = true;
        }
    }
}