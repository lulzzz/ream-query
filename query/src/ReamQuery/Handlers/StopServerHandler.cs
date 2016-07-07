namespace ReamQuery.Handlers
{
    using System.Threading.Tasks;
    using ReamQuery.Api;
    using Microsoft.AspNetCore.Http;

    public class StopServerHandler : BaseHandler<StatusResponse, string>
    {
        public StopServerHandler(RequestDelegate next) : base(next) { }

        protected override bool Handle(string path)
        {
            return path.Contains("/stopserver");
        }

        protected override async Task<StatusResponse> Execute(string input)
        {
            ReamQuery.Program.AppLifeTime.StopApplication();
            return await Task.FromResult(new StatusResponse());
        }
    }
}