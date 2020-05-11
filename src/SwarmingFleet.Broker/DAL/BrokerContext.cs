
namespace SwarmingFleet.Broker.DAL
{
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders; 

    public class BrokerContext : DbContext
    {
        public BrokerContext(DbContextOptions<BrokerContext> options) : base(options)
        {
        }

        public DbSet<WorkerInfo> Workers { get; set; } 
    }
}