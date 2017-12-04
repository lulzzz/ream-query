namespace ReamQuery.Server.Handlers
{
    using System;
    using System.Threading.Tasks;
    using ReamQuery.Server.Api;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Hosting;

    public class StopServerHandler : BaseHandler<StatusResponse, string>
    {
        IApplicationLifetime _lifetime;

        public StopServerHandler(RequestDelegate next, IApplicationLifetime lifetime) : base(next)
        {
            _lifetime = lifetime;
        }

        protected override bool Handle(string path)
        {
            return path.Contains("/stopserver");
        }

        protected override async Task<StatusResponse> Execute(string input)
        {
            _lifetime.StopApplication();
            return await Task.FromResult(new StatusResponse { Code = StatusCode.Ok });
        }
    }
}