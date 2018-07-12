using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
	public class ContainerDestination : ISmartsheetObject
	{
		public ContainerDestination()
		{

		}

		public long DestinationId { get; set; }

		public string DestinationType { get; set; }

		public string NewName { get; set; }
	}
}
