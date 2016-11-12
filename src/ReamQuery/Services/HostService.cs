namespace ReamQuery.Services
{
    using System;
    using Microsoft.CodeAnalysis;
    using ReamQuery.Models;
    using NLog;
    using ReamQuery.Core;
    using System.Diagnostics;
    using System.Threading.Tasks;

    public class HostService 
    {
        CompileService _compiler;
        ClientService _clientService;

        public HostService(ClientService clientService, CompileService compiler)
        {
            _clientService = clientService;
            _compiler = compiler;
        }

        private static Logger Logger = LogManager.GetCurrentClassLogger();

        public CompileResult StartGenerated(Guid id, string source, string assmName, MetadataReference context = null) 
        {
            var sw = new Stopwatch();
            sw.Start();
            var compileResult = _compiler.LoadType(source, assmName, context);
            if (compileResult.Code == Api.StatusCode.Ok)
            {
                var t = new Task(async () =>
                {
                    var programInstance = (IGenerated) Activator.CreateInstance(compileResult.Type);
                    var e1 = sw.Elapsed.TotalMilliseconds;
                    sw.Reset();
                    sw.Start();
                    await StartInternal(id, programInstance);
                    var e2 = sw.Elapsed.TotalMilliseconds;
                    Logger.Debug("{2}: IGenerated.Run TotalMilliseconds {0} (startup: {1} ms)", e2, e1, id);
                });
                t.Start();
            }
            return compileResult;
        }

        async Task StartInternal(Guid id, IGenerated instance)
        {
            // todo make buffer window configurable
            using (var emitter = new Emitter(id, 100))
            {
                _clientService.AddEmitter(emitter); // should unsub when disposing
                await instance.Run(emitter);
                emitter.Complete();
            }
        }
    }
}
