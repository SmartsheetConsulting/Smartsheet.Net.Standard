using System.Collections.Generic;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Entities
{
    public class Filter : ISmartsheetObject
    {
        public Filter()
        {
            this.Criteria = new List<Criteria>();
            this.Values = new List<dynamic>();
        }

        public string Type { get; set; }
        public bool ExcludedSection { get; set; }

        public ICollection<dynamic> Values { get; set; }
        public ICollection<Criteria> Criteria { get; set; }
    }
}
