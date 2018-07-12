using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class AlternateEmail : ISmartsheetObject
    {
        public AlternateEmail()
        {

        }

        public long Id { get; set; }

        public string Email { get; set; }

        public bool Confirmed { get; set; }
    }
}
