using Code.Hub.Shared.Auditing;
using Microsoft.AspNetCore.Identity;
using System;

namespace Code.Hub.Shared.Models
{
    public class CodeHubUser : IdentityUser<string>, IHasSoftDelete
    {
        public bool LoginEnabled { get; set; }
        public DateTime LastLoggedInTime { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }

        public bool IsDeleted { get; set; }
        public Guid DeleteUserId { get; set; }
        public DateTime DeletedDateTime { get; set; }
    }
}
