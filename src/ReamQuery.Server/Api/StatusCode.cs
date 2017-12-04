namespace ReamQuery.Server.Api
{
    public enum StatusCode
    {
        Ok,
        CompilationError,
        ServerUnreachable,
        ConnectionStringSyntax,
        NamespaceIdentifier,
        UnknownError,
    }
}
