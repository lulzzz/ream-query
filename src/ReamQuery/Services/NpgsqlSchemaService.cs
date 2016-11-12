namespace ReamQuery
{
    using System;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using ReamQuery.Services;
    using Microsoft.EntityFrameworkCore.Scaffolding;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;

    public class NpgsqlSchemaService : BaseSchemaService
    {
        public NpgsqlSchemaService()
        {
            var serviceProvider = new NpgsqlDesignTimeServices()
                .ConfigureDesignTimeServices(
                    new ServiceCollection()
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
                        }))
                .BuildServiceProvider();
            serviceProvider.GetService<ILoggerFactory>().AddNLog();
            Generator = serviceProvider.GetRequiredService<ReverseEngineeringGenerator>();
            ScaffoldingModelFactory = serviceProvider.GetRequiredService<IScaffoldingModelFactory>();
        }
    }
}