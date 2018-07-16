using System;
namespace Smartsheet.Net.Standard.Entities
{
	public class WebhookVerification
	{
		public WebhookVerification()
		{
		}

		public string WebhookId { get; set; }
		public string ChangeAgent { get; set; }
	}
}
