namespace ReamQuery
{
    using System;
    using System.Globalization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
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
            if (string.IsNullOrWhiteSpace(baseDir))
            {
                Startup.Configuration["REAMQUERY_BASEDIR"] = System.AppContext.BaseDirectory;
            }

            var host = new WebHostBuilder()
                .UseConfiguration(Startup.Configuration)
                .UseKestrel()
                .UseStartup(typeof(Startup))
                .Build();

            host.Run();
        }
    }
}
