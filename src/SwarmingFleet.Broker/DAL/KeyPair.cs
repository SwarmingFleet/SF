
namespace SwarmingFleet.Broker.DAL
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

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
         
        /// <summary>
        /// 註冊狀態
        /// </summary>
        public bool Registered { get; set; }

        public DateTime? LastOnlineTime { get; set; } 

        public DateTime CreatedTime { get; private set; } = DateTime.UtcNow;
    }
}