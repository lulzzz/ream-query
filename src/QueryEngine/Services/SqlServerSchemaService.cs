namespace QueryEngine
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using QueryEngine.Services;
    using Microsoft.EntityFrameworkCore.Scaffolding;
    using Microsoft.Extensions.Logging;

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
            serviceProvider.GetService<ILoggerFactory>().AddConsole();
            Generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            ScaffoldingModelFactory = serviceProvider.GetRequiredService<IScaffoldingModelFactory>();
        }
    }
}
