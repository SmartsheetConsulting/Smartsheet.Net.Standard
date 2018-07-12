using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class Attachment : ISmartsheetObject
    {
        public Attachment()
        {

        }

        public long Id { get; set; }
        public long UrlExpiresInMillis { get; set; }
        public long ParentId { get; set; }
        public long SizeInKb { get; set; }

        public string Name { get; set; }
        public string Url { get; set; }
        public string AttachmentType { get; set; }
        public string AttachmentSubType { get; set; }
        public string MimeType { get; set; }
        public string ParentType { get; set; }

        public DateTime CreatedAt { get; set; }

        public User CreatedBy { get; set; }
    }
}
