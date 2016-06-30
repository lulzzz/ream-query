namespace QueryEngine 
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using QueryEngine.Services;
    using QueryEngine.Handlers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

    public class Startup
    {
        public static IConfigurationRoot Configuration { get; set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            var ll = LogLevel.Debug;
            Enum.TryParse(Configuration.GetSection("Logging:LogLevel:Default").Value, out ll);
            loggerFactory.AddConsole(ll);
            app.UseMiddleware<CheckReadyStatusHandler>();
            app.UseMiddleware<StopServerHandler>();
            app.UseMiddleware<ExecuteQueryHandler>();
            app.UseMiddleware<QueryTemplateHandler>();
            app.UseMiddleware<DebugHandler>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var sqlServerSchemaSvc = new SqlServerSchemaService();
            var npgsqlSchemaSvc = new NpgsqlSchemaService();
            var schemaService = new SchemaService(sqlServerSchemaSvc, npgsqlSchemaSvc);
            var fragmentService = new FragmentService();
            var compiler = new CompileService(schemaService);
            var dbContextService = new DatabaseContextService(schemaService, compiler);
            var queryService = new QueryService(compiler, dbContextService, schemaService, fragmentService);
            services.AddSingleton<QueryService>(queryService);
            services.AddSingleton<SchemaService>(schemaService);
            services.AddSingleton<CompileService>(compiler);
            services.AddSingleton<DatabaseContextService>(dbContextService);
            services.AddSingleton<FragmentService>(fragmentService);
        }
    }
}
