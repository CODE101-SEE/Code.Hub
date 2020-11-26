using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Code.Hub.Shared.Models
{
    public class Epic
    {
        public Epic()
        {
            WorkLogs = new List<WorkLog>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(64, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDisabled { get; set; }

        public IEnumerable<WorkLog> WorkLogs { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}
