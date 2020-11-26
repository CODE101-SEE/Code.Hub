using System;

namespace Code.Hub.Shared.Auditing
{
    public interface IHasSoftDelete
    {
        public bool IsDeleted { get; set; }
        public Guid DeleteUserId { get; set; }
        public DateTime DeletedDateTime { get; set; }
    }
}
