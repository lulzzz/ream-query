namespace ReamQuery
{
    using System;
    using System.Globalization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        public static IApplicationLifetime AppLifeTime;

        public static void Main()
        {
            // todo
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");

            // ProjectJsonWorkspace used by CompilerService has issues with project refs,
            // this allows the test project to inject the correct base path when testing.
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REAMQUERY_BASEDIR")))
            {
                Environment.SetEnvironmentVariable("REAMQUERY_BASEDIR", System.AppContext.BaseDirectory);
            }
            
            var port = 8111;
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("REAMQUERY_PORT")))
            {
                Int32.TryParse(Environment.GetEnvironmentVariable("REAMQUERY_PORT"), out port);
            }

            Startup.Configuration = new ConfigurationBuilder()
                .SetBasePath(System.AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables("REAMQUERY_")
                .AddCommandLine(new[] { "--server.urls", "http://localhost:" + port })
                .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(Startup.Configuration)
                .UseKestrel()
                .UseStartup(typeof(Startup))
                .Build();

            host.Run();
        }
    }
}
