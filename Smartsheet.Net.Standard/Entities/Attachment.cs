﻿using System;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class Attachment : ISmartsheetObject
    {
        public long? Id { get; set; }
        public long? ParentId { get; set; }
        public string AttachmentType { get; set; }
        public string AttachmentSubType { get; set; }
        public string MimeType { get; set; }
        public string ParentType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public User CreatedBy { get; set; }
        public string Name { get; set; }
        public long? SizeInKb { get; set; }
        public string Url { get; set; }
        public long? UrlExpiresInMillis { get; set; }
        public string Description { get; set; }

        public void Build()
        {
            Id = null;
            ParentId = null;
            MimeType = null;
            ParentType = null;
            CreatedAt = null;
            CreatedBy = null;
            SizeInKb = null;
            UrlExpiresInMillis = null;
        }
    }
}
