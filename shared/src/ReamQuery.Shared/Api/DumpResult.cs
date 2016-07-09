namespace ReamQuery.Shared
{
    /// <summary>
    /// Base class for the result of a Dump invocation on an object reference.
    /// Can contain either tabular or singular values or empty
    /// </summary>
    public abstract class DumpResult
    {
        public string Title { get; set; }
    }
}
