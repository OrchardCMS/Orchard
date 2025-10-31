using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Orchard.Comments.Models
{
    public class CommentPart : ContentPart<CommentPartRecord>
    {
        public LazyField<ContentItem> CommentedOnContentItemField { get; } = new LazyField<ContentItem>();
        public LazyField<ContentItemMetadata> CommentedOnContentItemMetadataField { get; } = new LazyField<ContentItemMetadata>();

        [StringLength(255)]
        public string Author
        {
            get { return Record.Author; }
            set { Record.Author = value; }
        }

        [StringLength(245)]
        [DisplayName("Site")]
        [RegularExpression(@"^(http(s)?://)?([a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}[\S]+$")]
        public string SiteName
        {
            get { return Record.SiteName; }
            set { Record.SiteName = value; }
        }

        public string UserName
        {
            get { return Record.UserName; }
            set { Record.UserName = value; }
        }

        [RegularExpression(@"^(?![\.@])(""([^""\r\\]|\\[""\r\\])*""|([-\w!#$%&'*+/=?^`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-zA-Z0-9][\w\.-]*[a-zA-Z0-9]\.[a-zA-Z][a-zA-Z\.]*[a-zA-Z]$")]
        public string Email
        {
            get { return Record.Email; }
            set { Record.Email = value; }
        }

        public CommentStatus Status
        {
            get { return Record.Status; }
            set { Record.Status = value; }
        }

        public DateTime? CommentDateUtc
        {
            get { return Record.CommentDateUtc; }
            set { Record.CommentDateUtc = value; }
        }

        [Required, DisplayName("Comment")]
        public string CommentText
        {
            get { return Record.CommentText; }
            set { Record.CommentText = value; }
        }

        public int CommentedOn
        {
            get { return Record.CommentedOn; }
            set { Record.CommentedOn = value; }
        }

        public int? RepliedOn
        {
            get { return Record.RepliedOn; }
            set { Record.RepliedOn = value; }
        }

        public decimal Position
        {
            get { return Record.Position; }
            set { Record.Position = value; }
        }

        public ContentItem CommentedOnContentItem
        {
            get { return CommentedOnContentItemField.Value; }
            set { CommentedOnContentItemField.Value = value; }
        }

        public ContentItemMetadata CommentedOnContentItemMetadata
        {
            get { return CommentedOnContentItemMetadataField.Value; }
            set { CommentedOnContentItemMetadataField.Value = value; }
        }

        public int CommentedOnContainer
        {
            get { return Record.CommentedOnContainer; }
            set { Record.CommentedOnContainer = value; }
        }
    }
}