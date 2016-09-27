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
            // append guid to create unique output location
            var outputPath = System.IO.Path.Combine(_tempFolder, Guid.NewGuid().ToIdentifierWithPrefix("folder"));
            var conf = new ReverseEngineeringConfiguration 
            {
                ConnectionString = connectionString,
                ContextClassName = programName,
                ProjectPath = "na",
                ProjectRootNamespace = assemblyNamespace,
                OutputPath = outputPath
            };
            ReverseEngineerFiles resFiles = null; 
            try 
            {
                resFiles = await Generator.GenerateAsync(conf);
            }
            catch (System.Exception exn) when (exn.ExpectedError())
            {
                Logger.Debug("Known error {0}", exn.Message);
                return new SchemaResult { Code = exn.StatusCode(), Message = exn.Message };
            }
            var output = new StringBuilder();
            var dbCtx = CreateContext(InMemoryFiles.RetrieveFileContents(outputPath, programName + ".cs"), isLibrary: withUsings);
            var ctx = dbCtx.Item1;
            if (!withUsings)
            {
                ctx = ctx.SkipLines(3);
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
                output.Append(InMemoryFiles.RetrieveFileContents(outputPath, System.IO.Path.GetFileName(fpath)).SkipLines(4));
            }
            
            return new SchemaResult 
            {
                Schema = output.ToString(),
                DefaultTable = dbCtx.Item2
            };
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

        string _refs =  Environment.NewLine +
"using System;" + Environment.NewLine + 
"using System.Collections.Generic;" + Environment.NewLine + 
"using System.ComponentModel.DataAnnotations;" + Environment.NewLine + 
"using System.ComponentModel.DataAnnotations.Schema;" + Environment.NewLine;

        string _instance =  Environment.NewLine + Environment.NewLine +
"        public static Lazy<Ctx> Instance = new Lazy<Ctx>(() => new Ctx());" +  Environment.NewLine;

        string _proxyPre = Environment.NewLine +
"    public class ##PROXY##" + Environment.NewLine +
"    {" + Environment.NewLine;

        string _proxyPost = Environment.NewLine +
"    }" + Environment.NewLine +
"}" + Environment.NewLine;

    }
}