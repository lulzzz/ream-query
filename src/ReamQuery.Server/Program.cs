namespace ReamQuery
{
    using System;
    using System.Globalization;
    using System.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.AspNetCore.Hosting;
    using NLog;
    using System.Text;
    using System.IO;

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
                .AddCommandLine(args)
                .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(Startup.Configuration)
                .UseKestrel()
                .UseStartup(typeof(Startup))
                .Build();

            Logger.Info("Starting in {0}", BaseDirectory);
            host.Run();
            Logger.Info("Exiting after {0} seconds", sw.Elapsed.TotalSeconds);
        }

        public static string BaseDirectory
        {
            get
            {
                var dllPath = new Uri(typeof(Program).Assembly.CodeBase).LocalPath;
                return Path.GetDirectoryName(dllPath);
            }
        }
    }
}
