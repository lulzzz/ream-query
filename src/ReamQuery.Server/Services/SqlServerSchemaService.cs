namespace ReamQuery.Server.Services
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using ReamQuery.Server.Services;
    using Microsoft.EntityFrameworkCore.Scaffolding;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;

    public class SqlServerSchemaService : BaseSchemaService
    {
        public SqlServerSchemaService()
        {
            var services = new ServiceCollection()
                .AddLogging()
                .AddSingleton<ReverseEngineeringGenerator>()
                .AddSingleton<ScaffoldingUtilities>()
                .AddSingleton<CSharpUtilities>()
                .AddSingleton<ConfigurationFactory>()
                .AddSingleton<DbContextWriter>()
                .AddSingleton<EntityTypeWriter>()
                .AddSingleton<CodeWriter, StringBuilderCodeWriter>()
                .AddSingleton<CandidateNamingService, EntityNamingService>()
                .AddSingleton(typeof(IFileService), sp => {
                    return InMemoryFiles = new InMemoryFileService();
                });
            new SqlServerDesignTimeServices().ConfigureDesignTimeServices(services);
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetService<ILoggerFactory>().AddNLog();
            Generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            ScaffoldingModelFactory = serviceProvider.GetRequiredService<IScaffoldingModelFactory>();
        }
    }
}
