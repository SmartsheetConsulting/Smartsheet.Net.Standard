using System.Collections.Generic;

namespace Smartsheet.Net.Standard.Entities
{
    public class Report : Sheet
    {
        public Report()
        {
              
        }

        public IEnumerable<Sheet> SourceSheets { get; set; }
    }
}
