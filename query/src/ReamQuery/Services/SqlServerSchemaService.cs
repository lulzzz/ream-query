namespace ReamQuery
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using ReamQuery.Services;
    using Microsoft.EntityFrameworkCore.Scaffolding;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;

    public class SqlServerSchemaService : BaseSchemaService
    {
        public SqlServerSchemaService()
        {
            var serviceProvider = new SqlServerDesignTimeServices()
                .ConfigureDesignTimeServices(
                    new ServiceCollection()
                        .AddLogging()
                        .AddScaffolding()
                        .AddSingleton(typeof(IFileService), sp => {
                            return InMemoryFiles = new InMemoryFileService();
                        }))
                .BuildServiceProvider();
            serviceProvider.GetService<ILoggerFactory>().AddNLog();
            Generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            ScaffoldingModelFactory = serviceProvider.GetRequiredService<IScaffoldingModelFactory>();
        }
    }
}
