namespace ReamQuery.RefDumper
{
    using System;
    using System.Linq;
    using System.IO;
    using System.Runtime.InteropServices;
    using Buildalyzer;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Microsoft.Build.Framework;

    class Program
    {
        static void Main(string[] args)
        {
            var files = CreateJsonRefList();
            CopyReferenceFiles(files);
            Console.WriteLine("RefDumper success");
        }

        static IEnumerable<string> CreateJsonRefList()
        {
            var projPath = Path.Combine(Helper.ProjectFolder, "src", "ReamQuery.Server", "ReamQuery.Server.csproj");
            Console.WriteLine("Project file: {0} (exists: {1})", projPath, File.Exists(projPath));
            var lf = new LoggerFactory();
            lf.AddConsole();
            var manager = new AnalyzerManager(lf, LoggerVerbosity.Minimal);
            var analyzer = manager.GetProject(projPath);
            analyzer.Load();
            var refs = analyzer.GetReferences();
            if (refs.Count() != refs.Distinct().Count())
            {
                throw new InvalidOperationException("Unexpected duplicates");
            }
            var json = JsonConvert.SerializeObject(refs.Select(x => Path.GetFileName(x)), Formatting.Indented);
            var jsonPath = Path.Combine(Helper.ProjectFolder, "src", "ReamQuery.Resources", "ReferenceList.json");
            File.WriteAllText(jsonPath, json);
            return refs;
        }

        static void CopyReferenceFiles(IEnumerable<string> files)
        {
            var folder = Path.Combine(Helper.ProjectFolder, "src", "ReamQuery.Resources", "MetadataFiles");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            else
            {
                var oldFiles = Directory.GetFiles(folder);
                foreach(var file in oldFiles)
                {
                    File.Delete(file);
                }
            }
            int copied = 0;
            foreach(var file in files)
            {
                var filename = Path.GetFileName(file);
                File.Copy(file, Path.Combine(folder, filename), true);
                copied++;
            }
            Console.WriteLine("{0} reference files", copied);
        }
    }
}
