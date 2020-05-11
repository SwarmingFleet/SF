
namespace SwarmingFleet.Broker.DAL
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using SwarmingFleet.Contracts;
    using SwarmingFleet.DAL;

    public class WorkerInfo : IKeyed<Guid>
    {
        public WorkerInfo()
        {
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; private set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string MemorySize { get; set; }
        [Required]
        public string StorageSize { get; set; }
        [Required]
        public string OperationSystem { get; set; }


        [Required]
        public string CPU { get; set; }
        [Required]
        public string GPU { get; set; }
        [Required]
        public string MAC { get; set; }

        [Required]
        public DateTime LastLoginTimes { get; set; }

        private const char DELIMITER = ';';

        public WorkerInfo(Worker worker, string address, DateTime lastLoginTimes)
        {
            this.Name = worker.Name;
            this.MemorySize = string.Join(DELIMITER, worker.MemorySizes);
            this.StorageSize = string.Join(DELIMITER, worker.StorageSizes);
            this.OperationSystem = worker.OperationSystem;
            this.CPU = string.Join(DELIMITER, worker.CPUs);
            this.GPU = string.Join(DELIMITER, worker.GPUs);
            this.MAC = string.Join(DELIMITER, worker.MACs);

            this.Address = address;
            this.LastLoginTimes = lastLoginTimes;
        }
    }
}