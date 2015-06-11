namespace Orchard.Comments.Services {
    public class CreateCommentContext {
        public virtual string Author { get; set; }
        public virtual string SiteName { get; set; }
        public virtual string Email { get; set; }
        public virtual string CommentText { get; set; }
        public virtual int CommentedOn { get; set; }
    }
}