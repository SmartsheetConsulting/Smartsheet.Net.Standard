using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class Share : ISmartsheetObject
    {
        public string Id { get; set; }
        public long? GroupId { get; set; }
        public long? UserId { get; set; }
        public string Type { get; set; }
        public string AccessLevel { get; set; }
        public bool? CcMe { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Scope {get; set;}
        public string Subject { get; set; }
        public string Message { get; set; }

        public Share Build()
        {
            if (this.Email != null && this.GroupId != null)
            {
                throw new Exception("Group ID or Email may be passed, but not both");
            }
            
            this.Id = null;
            this.Type = null;
            this.CreatedAt = null;
            this.ModifiedAt = null;
            this.UserId = null;
            this.Name = null;
            this.Scope = null;

            return this;
        }
    }
}