﻿using System;
using System.Collections.Generic;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.Net.Standard.Entities
{
    public class Discussion : ISmartsheetObject
    {
        public long? Id { get; set; }
        public long? ParentId { get; set; }
        public string ParentType { get; set; }
        public string AccessLevel { get; set; }
        public ICollection<Attachment> CommentAttachments { get; set; }
        public int? CommentCount { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public Comment Comment { get; set; }
        public User CreatedBy { get; set; }
        public DateTime? LastCommentedAt { get; set; }
        public User LastCommentedUser { get; set; }
        public bool? ReadOnly { get; set; }
        public string Title { get; set; }
    }
}
