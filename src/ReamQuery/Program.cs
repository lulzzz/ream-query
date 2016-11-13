namespace ReamQuery
{
    using System;
    using System.Globalization;
    using System.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using NLog;

    public class Program
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            // todo
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Startup.Configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            // ProjectJsonWorkspace used by CompilerService has issues with project refs,
            // this allows the test project to inject the correct base path when testing.
            var baseDir = Startup.Configuration["REAMQUERY_BASEDIR"];
            var distDir = Startup.Configuration["REAMQUERY_DISTDIR"];
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                Startup.Configuration["REAMQUERY_BASEDIR"] = System.AppContext.BaseDirectory;
            }
            if (string.IsNullOrWhiteSpace(distDir))
            {
                Startup.Configuration["REAMQUERY_DISTDIR"] = System.AppContext.BaseDirectory;
            }

            var host = new WebHostBuilder()
                .UseConfiguration(Startup.Configuration)
                .UseKestrel()
                .UseStartup(typeof(Startup))
                .Build();

            Logger.Info("Starting in {0}", Startup.Configuration["REAMQUERY_BASEDIR"]);
            host.Run();
            Logger.Info("Exiting after {0} seconds", sw.Elapsed.TotalSeconds);
        }
    }
}
