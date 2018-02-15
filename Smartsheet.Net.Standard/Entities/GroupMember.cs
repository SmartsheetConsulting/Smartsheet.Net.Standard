using System;
using System.Collections.Generic;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Entities
{
    public class GroupMember : SmartsheetObject
    {
        public GroupMember()
        {

        }

        public long? Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Name { get; set; }
    }
}
