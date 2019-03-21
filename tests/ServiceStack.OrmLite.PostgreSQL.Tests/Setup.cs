using NUnit.Framework;
using ServiceStack.OrmLite.Tests;

[assembly: NonParallelizable]

namespace ServiceStack.OrmLite.PostgreSQL.Tests
{
    
    public class LoggerSetup : LogSetup
    {
    }

    public class DbSetup : DbFactorySetup
    {
    }
}