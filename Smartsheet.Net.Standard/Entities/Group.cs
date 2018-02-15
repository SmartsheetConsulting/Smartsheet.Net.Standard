using System;
using System.Collections.Generic;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Entities
{
    public class Group : SmartsheetObject
    {
       
        public Group()
        {

        }

        public Group(string groupName, string description, List<GroupMember> members)
        {
            this.Name = groupName;
            this.Description = description;
            this.Members = members;
        }

        public Group(string groupName, string description, long ownerId)
        {
            this.Name = groupName;
            this.Description = description;
            this.OwnerId = ownerId;
        }

        public long? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public long? OwnerId { get; set; }
        public IList<GroupMember> Members { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
