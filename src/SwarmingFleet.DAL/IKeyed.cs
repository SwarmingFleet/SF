
namespace SwarmingFleet.DAL
{
    public interface IKeyed<TKey> where TKey : struct
    {
        TKey Id { get; }
    }
}