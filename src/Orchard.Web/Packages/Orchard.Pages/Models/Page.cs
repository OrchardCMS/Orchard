using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Security;

namespace Orchard.Pages.Models {
    public class Page : ContentPart<PageRecord> {
        [HiddenInput(DisplayValue = false)]
        public int Id { get { return ContentItem.Id; } }

        [Required]
        public string Title {
            get { return this.As<RoutableAspect>().Title; }
            set { this.As<RoutableAspect>().Title = value; }
        }

        [Required]
        public string Slug {
            get { return this.As<RoutableAspect>().Slug; }
            set { this.As<RoutableAspect>().Slug = value; }
        }

        public IUser Creator {
            get { return this.As<CommonAspect>().Owner; }
            set { this.As<CommonAspect>().Owner = value; }
        }

        public bool IsPublished {
            get { return ContentItem.VersionRecord.Published; }
        }

        public bool HasDraft {
            get {
                return (
                    (ContentItem.VersionRecord.Published == false) ||
                    (ContentItem.VersionRecord.Published && ContentItem.VersionRecord.Latest == false));
            }
        }

        public bool HasPublished {
            get { 
                if (IsPublished) 
                    return true;
                if (ContentItem.ContentManager.Get(Id, VersionOptions.Published) != null) 
                    return true;
                return false;
            }
        }

        public string PublishedSlug {
            get {
                if (IsPublished)
                    return Slug;
                Page publishedPage = ContentItem.ContentManager.Get<Page>(Id, VersionOptions.Published);
                if (publishedPage == null) 
                    return String.Empty;
                return publishedPage.Slug;
            }
        }

        public DateTime? Published {
            get { return Record.Published; }
            set { Record.Published = value; }
        }

        //[CascadeAllDeleteOrphan]
        //public virtual IList<Scheduled> Scheduled { get; protected set; }
    }
}
