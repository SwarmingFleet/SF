
namespace SwarmingFleet.Broker.DAL
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
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

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{ 
        //    modelBuilder.Entity<KeyPair>()
        //        .HasKey(c => new { c.Dhk, c.Spk });
        //    //base.OnModelCreating(modelBuilder);
        //}

        /// <summary>
        /// 金鑰對表
        /// </summary>
        public DbSet<KeyPair> KeyPairs { get; private set; }

        /// <summary>
        /// 連線紀錄表
        /// </summary>
        public DbSet<ConnectionLog> ConnectionLogs { get; private set; }
    }
}