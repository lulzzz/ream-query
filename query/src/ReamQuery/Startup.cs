namespace ReamQuery 
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using ReamQuery.Services;
    using ReamQuery.Handlers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using System.IO;

    public class Startup
    {
        public static IConfigurationRoot Configuration { get; set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IHostingEnvironment env)
        {
            loggerFactory.AddNLog();
            env.ConfigureNLog(Path.Combine(Configuration["REAMQUERY_BASEDIR"], "nlog.config"));
            app.UseMiddleware<CheckReadyStatusHandler>();
            app.UseMiddleware<ExecuteQueryHandler>();
            app.UseMiddleware<QueryTemplateHandler>();
            app.UseMiddleware<StopServerHandler>();
            app.UseMiddleware<WebSocketHandler>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ClientService, ClientService>();
            services.AddSingleton<SqlServerSchemaService, SqlServerSchemaService>();
            services.AddSingleton<NpgsqlSchemaService, NpgsqlSchemaService>();
            services.AddSingleton<QueryService, QueryService>();
            services.AddSingleton<SchemaService, SchemaService>();
            services.AddSingleton<CompileService, CompileService>();
            services.AddSingleton<DatabaseContextService, DatabaseContextService>();
            services.AddSingleton<FragmentService, FragmentService>();
        }
    }
}
