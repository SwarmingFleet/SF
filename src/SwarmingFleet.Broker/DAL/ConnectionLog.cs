
namespace SwarmingFleet.Broker.DAL
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    /// <summary>
    /// 連線紀錄
    /// </summary>
    public class ConnectionLog
    {
        /// <summary>
        /// 識別碼
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        /// <summary>
        /// 時間戳
        /// </summary>
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// 端點
        /// </summary>
        [Required]
        public string Endpoint { get; set; }

        /// <summary>
        /// 存取動作
        /// </summary>
        [Required]
        public string Action { get; set; }

        /// <summary>
        /// 備註
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 危害等級
        /// </summary>
        public HazardLevels? HazardLevel { get; set; } 
         

        /// <summary>
        /// 伺服器預產金鑰(如果有)
        /// </summary> 
        public string Spk { get; set; }

        /// <summary>
        /// 裝置硬體金鑰(如果有)
        /// </summary>
        public string Dhk { get; set; }
    }
}