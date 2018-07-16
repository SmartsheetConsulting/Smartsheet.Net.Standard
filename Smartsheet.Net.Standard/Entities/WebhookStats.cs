using System;

namespace Smartsheet.Net.Standard.Entities
{
	public class WebhookStats
	{
		public WebhookStats()
		{
		}

		public long? LastCallbackAttemptRetryCount { get; }
		public DateTime LastCallbackAttempt { get; }
		public DateTime LastSuccessfulCallback { get; }
	}
}
