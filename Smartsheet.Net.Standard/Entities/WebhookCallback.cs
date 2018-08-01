using System;
using System.Collections.Generic;

namespace Smartsheet.Net.Standard.Entities
{
	public class WebhookCallback
	{
		public WebhookCallback()
		{
		}

		//	Sent on initial verification (and every 100 requests)
		public long? WebhookId { get; set; }
		public string ChangeAgent { get; set; }
		public string Challenge { get; set; }

		//	Standard data included in callback
		public string Nonce { get; set; }
		public DateTime Timestamp { get; set; }
		public string Scope { get; set; }
		public long? ScopeObjectId { get; set; }
		public List<WebhookCallbackEvent> Events { get; set; }
	}
}
