namespace ReamQuery 
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using ReamQuery.Server.Services;
    using ReamQuery.Server.Handlers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using System.IO;

    public class Startup
    {
        public static IConfigurationRoot Configuration { get; set; }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IHostingEnvironment env)
        {
            loggerFactory.AddNLog().ConfigureNLog("nlog.config");
            app.UseWebSockets();
            app.UseMiddleware<CheckReadyStatusHandler>();
            app.UseMiddleware<ExecuteQueryHandler>();
            app.UseMiddleware<ExecuteCodeHandler>();
            app.UseMiddleware<QueryTemplateHandler>();
            app.UseMiddleware<CodeTemplateHandler>();
            app.UseMiddleware<StopServerHandler>();
            app.UseMiddleware<WebSocketHandler>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ClientService>();
            services.AddSingleton<SqlServerSchemaService>();
            services.AddSingleton<SqliteSchemaService>();
            services.AddSingleton<NpgsqlSchemaService>();
            services.AddSingleton<QueryService>();
            services.AddSingleton<SchemaService>();
            services.AddSingleton<CompileService>();
            services.AddSingleton<DatabaseContextService>();
            services.AddSingleton<FragmentService>();
            services.AddSingleton<CSharpCodeService>();
            services.AddSingleton<HostService>();
            services.AddSingleton<ReferenceProvider>();
        }
    }
}
