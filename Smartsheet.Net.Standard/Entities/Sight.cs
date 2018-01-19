using System;
using Smartsheet.NET.Standard.Entities;

namespace Smartsheet.NET.Standard.Entities
{
    public class Sight : SmartsheetObject
    {
        public Sight()
        {

        }

        public long? Id { get; set; }
        public string Name { get; set; }
        public string AccessLevel { get; set; }
        public string Permalink { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

    }
}
