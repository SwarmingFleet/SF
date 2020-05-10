
namespace SwarmingFleet.Broker.DataAccessLayers
{
    public interface IKeyed<TKey> where TKey : struct
    {
        TKey Id { get; }
    }
}