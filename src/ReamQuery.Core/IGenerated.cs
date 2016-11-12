namespace ReamQuery.Core
{
    using System.Threading.Tasks;

    public interface IGenerated
    {
        Task Run(Emitter emitter);
    }
}
