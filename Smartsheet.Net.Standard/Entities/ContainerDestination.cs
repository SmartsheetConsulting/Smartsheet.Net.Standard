using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Entities
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
