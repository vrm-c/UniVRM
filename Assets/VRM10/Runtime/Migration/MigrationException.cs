using System;

namespace UniVRM10
{
    public class MigrationException : Exception
    {
        public MigrationException(string key, string value) : base($"{key}: {value}")
        {
        }
    }
}
