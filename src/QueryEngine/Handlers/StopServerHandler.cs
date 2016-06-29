namespace QueryEngine.Handlers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class StopServerHandler : BaseHandler<bool, string>
    {
        public StopServerHandler(RequestDelegate next) : base(next) { }

        protected override bool Handle(string path)
        {
            return path.Contains("/stopserver");
        }

        protected override async Task<bool> Execute(string input)
        {
            QueryEngine.Program.AppLifeTime.StopApplication();
            return await Task.FromResult(true);
        }
    }
}