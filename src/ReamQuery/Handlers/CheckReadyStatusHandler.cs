namespace ReamQuery.Handlers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class CheckReadyStatusHandler : BaseHandler<bool, string>
    {
        public CheckReadyStatusHandler(RequestDelegate next) : base(next) { }

        protected override bool Handle(string path)
        {
            return path.Contains("/checkreadystatus");
        }

        protected override async Task<bool> Execute(string input)
        {
            return await Task.FromResult(true);
        }
    }
}