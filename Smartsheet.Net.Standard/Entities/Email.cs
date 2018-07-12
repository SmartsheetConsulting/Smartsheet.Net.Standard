using System.Collections.Generic;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class Email : ISmartsheetObject
    {
        public Email()
        {

        }

        public string Subject { get; set; }
        public string Message { get; set; }
        public bool? CcMe { get; set; }

        public ICollection<Recipient> SendTo { get; set; }
    }
}
