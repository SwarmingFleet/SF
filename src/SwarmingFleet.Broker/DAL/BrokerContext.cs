
namespace SwarmingFleet.Broker.DAL
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders; 

    public class BrokerContext : DbContext
    { 
        public BrokerContext(DbContextOptions<BrokerContext> options) : base(options)
        {
            this.Database.EnsureCreated();
        }

        public DbSet<KeyPair> KeyPairs { get; private set; }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{ 
        //    modelBuilder.Entity<KeyPair>()
        //        .HasKey(c => new { c.Dhk, c.Spk });
        //    //base.OnModelCreating(modelBuilder);
        //}
    }


    public class KeyPair
    {
        /// <summary>
        /// 伺服器預產金鑰(Server Pregenerated Key)
        /// </summary>
        [Key]
        public string Spk { get; set; }
        /// <summary>
        /// 裝置硬體金鑰(Device Hardware Key)
        /// </summary>
        public string Dhk { get; set; }
         
        public bool Registered { get; set; }

        public DateTime? LastOnlineTime { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
}