using System.Collections.Generic;

namespace Smartsheet.NET.Standard.Entities
{
    public class Report : Sheet
    {
        public Report()
        {
              
        }

        public IEnumerable<Sheet> SourceSheets { get; set; }
    }
}
