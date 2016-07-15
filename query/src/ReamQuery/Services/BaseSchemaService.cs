namespace ReamQuery
{
    using System;
    using System.Linq;
    using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
    using ReamQuery.Services;
    using Microsoft.EntityFrameworkCore.Scaffolding;
    using System.Threading.Tasks;
    using ReamQuery.Models;
    using System.Text;
    using System.Text.RegularExpressions;
    using ReamQuery.Helpers;
    using NLog;

    public abstract class BaseSchemaService
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        
        private string _tempFolder;

        protected InMemoryFileService InMemoryFiles;
        protected ReverseEngineeringGenerator Generator;
        protected IScaffoldingModelFactory ScaffoldingModelFactory;
        public BaseSchemaService() 
        {
            // todo figure out something better
            _tempFolder = Environment.GetEnvironmentVariable("TEMP");
            if (string.IsNullOrEmpty(_tempFolder)) 
            {
                _tempFolder = "/tmp";
            }
        }
        public async Task<SchemaResult> GetSchemaSource(string connectionString, string assemblyNamespace, bool withUsings = true) 
        {
            var programName = "Ctx";
            var conf = new ReverseEngineeringConfiguration 
            {
                ConnectionString = connectionString,
                ContextClassName = programName,
                ProjectPath = "na",
                ProjectRootNamespace = assemblyNamespace,
                OutputPath = _tempFolder
            };
            ReverseEngineerFiles resFiles = null; 
            try 
            {
                resFiles = await Generator.GenerateAsync(conf);
            }
            catch (System.Exception exn) when (exn.ExpectedError())
            {
                return new SchemaResult { Code = exn.StatusCode(), Message = exn.Message };
            }
            var output = new StringBuilder();
            var dbCtx = CreateContext(InMemoryFiles.RetrieveFileContents(_tempFolder, programName + ".cs"), isLibrary: withUsings);
            var ctx = dbCtx.Item1;
            if (!withUsings)
            {
                var x = 
                ctx = StripHeaderLines(3, ctx);
            }
            else
            {
                output.Append(_refs);
            }
            // remove the entity generated warning about injected connection strings
            ctx = Regex.Replace(ctx, @"#warning.*", "");
            output.Append(ctx);
            Logger.Info("ContextFile.Count {0}", resFiles.ContextFile.Count());
            foreach(var fpath in resFiles.EntityTypeFiles)
            {
                output.Append(StripHeaderLines(4, InMemoryFiles.RetrieveFileContents(_tempFolder, System.IO.Path.GetFileName(fpath))));
            }
            
            return new SchemaResult 
            {
                Schema = output.ToString(),
                DefaultTable = dbCtx.Item2
            };
        }

        string StripHeaderLines(int lines, string contents) 
        {
            return string.Join("\n", contents.Split('\n').Skip(lines));
        }

        Tuple<string, string> CreateContext(string ctx, bool isLibrary = true) 
        {
            var idx1 = ctx.IndexOf("{");
            var idx = ctx.IndexOf("{", idx1 + 1) + 1;
            var pre = ctx.Substring(0, idx);
            var post = ctx.Substring(idx);
            var newCtx = pre + _instance + post.Substring(0, post.Length - 2);
            var regex = new Regex(@"^\s*public virtual DbSet\<([^>]*).*$", RegexOptions.Multiline);
            var tables = regex.Matches(ctx);
            var proxyCtx = _proxyPre.Replace("##PROXY##", isLibrary ? "Main" : "Proxy");
            var firstTable = string.Empty;
            foreach(Match t in tables)
            {
                if (string.IsNullOrEmpty(firstTable)) 
                {
                    firstTable = t.Groups[1].Value.ToString();
                }
                var proxy = "get { return Ctx.Instance.Value." + t.Groups[1].Value + "; }";
                proxyCtx += t.Value.Replace("get; set;", proxy);
            }
            return Tuple.Create(newCtx + proxyCtx + _proxyPost, firstTable);
        }

        string _refs = @"
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
";

        string _instance = @"
        public static Lazy<Ctx> Instance = new Lazy<Ctx>(() => new Ctx());
";

        string _proxyPre = @"
    public class ##PROXY##
    {
";

        string _proxyPost = @"
    }
}
";
    }
}