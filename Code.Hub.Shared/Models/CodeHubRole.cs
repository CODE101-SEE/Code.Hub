using Code.Hub.Shared.Auditing;
using Microsoft.AspNetCore.Identity;
using System;

namespace Code.Hub.Shared.Models
{
    public class CodeHubRole : IdentityRole<string>, IHasSoftDelete
    {
        public bool IsDeleted { get; set; }
        public Guid DeleteUserId { get; set; }
        public DateTime DeletedDateTime { get; set; }
    }
}
