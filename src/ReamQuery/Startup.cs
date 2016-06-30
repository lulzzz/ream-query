namespace ReamQuery 
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using ReamQuery.Services;
    using ReamQuery.Handlers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using Microsoft.AspNetCore.Hosting;

    public class Startup
    {
        public static IConfigurationRoot Configuration { get; set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IHostingEnvironment env)
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
            // ProjectJsonWorkspace used by CompilerService has issues with project refs,
            // this allows the test project to inject the correct base path when testing.
            var baseDir = Configuration["REAMQUERY_BASEDIR"];
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                baseDir = System.AppContext.BaseDirectory;
            }
            var sqlServerSchemaSvc = new SqlServerSchemaService();
            var npgsqlSchemaSvc = new NpgsqlSchemaService();
            var schemaService = new SchemaService(sqlServerSchemaSvc, npgsqlSchemaSvc);
            var fragmentService = new FragmentService();
            var compiler = new CompileService(schemaService, baseDir);
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
