using System.Collections.Generic;

namespace Smartsheet.Net.Standard.Entities
{
    public class MultiRowEmail : RowEmail
    {
        public MultiRowEmail()
        {

        }

        public ICollection<long> RowIds { get; set; }
    }
}
