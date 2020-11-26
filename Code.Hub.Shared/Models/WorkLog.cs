using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Code.Hub.Shared.Models
{
    public class WorkLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public string Description { get; set; }

        [Required]
        public double Hours { get; set; }

        [Required]
        public WorkLogStatus Status { get; set; }

        [Required]
        public DateTime DateStarted { get; set; }
        [Required]
        public DateTime DateFinished { get; set; }

        public DateTime SubmittedTime { get; set; }
        public DateTime LastModifiedTime { get; set; }

        public int? EpicId { get; set; }
        [ForeignKey("EpicId")]
        public virtual Epic Epic { get; set; }

        public long? TaskId { get; set; }
        public string ProviderType { get; set; }
        public int? OrganizationId { get; set; }
    }
}
