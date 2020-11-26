using System;

namespace Code.Hub.Shared.Auditing
{
    public interface IHasCreationInformation
    {
        public DateTime TimeCreated { get; set; }
    }
}
