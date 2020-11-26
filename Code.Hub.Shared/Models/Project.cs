using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Code.Hub.Shared.Models
{
    public class Project
    {
        public Project()
        {
            Epics = new List<Epic>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(64, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }

        public string Description { get; set; }
        public bool IsDisabled { get; set; }

        public IEnumerable<Epic> Epics { get; set; }

        public int OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }
}
