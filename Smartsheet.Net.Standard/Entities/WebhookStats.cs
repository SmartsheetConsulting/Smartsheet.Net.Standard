using System;

namespace Smartsheet.NET.Standard.Entities
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
