﻿using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Comments.Models;
using Orchard.ContentManagement;

namespace Orchard.Comments.ViewModels {
    public class CommentsIndexViewModel {
        public IList<CommentEntry> Comments { get; set; }
        public CommentIndexOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class CommentEntry {
        public CommentPartRecord Comment { get; set; }
        public IContent CommentedOn { get; set; }
        public bool IsChecked { get; set; }
    }

    public class CommentIndexOptions {
        public CommentIndexFilter Filter { get; set; }
        public CommentIndexBulkAction BulkAction { get; set; }
    }

    public enum CommentIndexBulkAction {
        None,
        Unapprove,
        Approve,
        Delete
    }

    public enum CommentIndexFilter {
        All,
        Pending,
        Approved,
    }
}
