namespace ReamQuery.RefDumper
{
    using System;
    using System.IO;
    using System.Reflection;

    public static class Helper
    {

        public static string ApplicationPath
        {
            get { return new Uri(Assembly.GetEntryAssembly().CodeBase).LocalPath; }
        }

        public static string ProjectFolder
        {
            get
            {
                var path = Path.GetDirectoryName(ApplicationPath);
                if (!path.EndsWith(@"tools\ReamQuery.RefDumper\bin\Debug\netcoreapp2.0"))
                {
                    throw new InvalidOperationException("Unexpected path");
                }
                return Path.GetFullPath(Path.Combine(path, "..", "..", "..", "..", ".."));
            }
        }
    }
}