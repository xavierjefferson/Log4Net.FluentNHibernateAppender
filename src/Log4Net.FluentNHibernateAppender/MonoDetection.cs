using System;

namespace Log4Net.FluentNHibernateAppender
{
    public static class MonoDetection
    {
        private static bool? _isRunningMono;

        public static bool IsRunningMono
        {
            get
            {
                if (_isRunningMono == null)
                {
                    _isRunningMono = Type.GetType("Mono.Runtime") != null;
                }
                return _isRunningMono.Value;
            }
        }
    }
}