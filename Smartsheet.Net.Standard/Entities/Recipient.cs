using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class Recipient : ISmartsheetObject
    {
        public Recipient()
        {

        }

        public Recipient(string email)
        {
            this.Email = email;
        }

        public string Email { get; set; }
        public long? GroupId { get; set; }
    }
}
