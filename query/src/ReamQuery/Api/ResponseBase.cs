namespace ReamQuery.Api
{
    public abstract class ResponseBase
    {
        public StatusCode Code { get; set; }

        public string Message { get; set; }
    }
}
