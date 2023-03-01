using System;

namespace UniVRM10
{
    internal class MigrationException : Exception
    {
        public MigrationException(string key, string value) : base($"{key}: {value}")
        {
        }
    }
}
