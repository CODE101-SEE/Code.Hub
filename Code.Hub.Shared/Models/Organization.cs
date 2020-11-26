using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Code.Hub.Shared.Models
{
    public class Organization
    {
        public Organization()
        {
            Projects = new List<Project>();
        }

        public int Id { get; set; }
        [Required]
        [StringLength(64, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }
        public string Description { get; set; }

        public string ProviderType { get; set; }
        public string Url { get; set; }
        public string AuthToken { get; set; }

        public string Color { get; set; }

        public bool IsDisabled { get; set; }

        public int CacheDurationSeconds { get; set; }

        public IEnumerable<Project> Projects { get; set; }

    }
}
