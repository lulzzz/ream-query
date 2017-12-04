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
                // assume this is run in the source folder layout
                return Path.GetFullPath(Path.Combine(path, "..", "..", "..", "..", ".."));
            }
        }
    }
}