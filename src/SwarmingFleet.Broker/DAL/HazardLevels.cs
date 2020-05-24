
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
    /// 危害等級
    /// </summary>
    public enum HazardLevels
    {
        /// <summary>
        /// 低等危害
        /// </summary>
        Low,
        /// <summary>
        /// 中等危害
        /// </summary>
        Middle,
        /// <summary>
        /// 高等危害
        /// </summary>
        High,
        /// <summary>
        /// 嚴重危害
        /// </summary>
        Critical,
        /// <summary>
        /// 致命危害
        /// </summary>
        Fatal
    }
}