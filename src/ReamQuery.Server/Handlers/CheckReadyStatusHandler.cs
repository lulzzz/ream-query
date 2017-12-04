namespace ReamQuery.Server.Handlers
{
    using System.Threading.Tasks;
    using ReamQuery.Server.Api;
    using Microsoft.AspNetCore.Http;

    public class CheckReadyStatusHandler : BaseHandler<StatusResponse, string>
    {
        public CheckReadyStatusHandler(RequestDelegate next) : base(next) { }

        protected override bool Handle(string path)
        {
            return path.Contains("/checkreadystatus");
        }

        protected override async Task<StatusResponse> Execute(string input)
        {
            return await Task.FromResult(new StatusResponse());
        }
    }
}