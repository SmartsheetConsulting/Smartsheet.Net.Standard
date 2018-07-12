using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class Sight : ISmartsheetObject
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
