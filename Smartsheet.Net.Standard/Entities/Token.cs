using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
	public class Token : ISmartsheetResult
    {
        public Token()
        {
        }

		public string access_token { get; set; }
		public string token_type { get; set; }
		public string refresh_token { get; set; }
		public string expires_in { get; set; }
    }
}
