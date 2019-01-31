using System;
using System.Collections;
using System.Collections.Generic;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class Comment : ISmartsheetObject
    {
        public long? Id { get; set; }
        public long? DiscussionId { get; set; }
        public IList<Attachment> Attachments { get; set; }
        public DateTime? CreatedAt { get; set; }
        public User CreatedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string Text { get; set; }
    }
}
