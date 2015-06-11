using System.Collections.Generic;
using System.Web.Mvc;

namespace IDeliverable.Slides.ViewModels
{
    public class ProjectionSlidesProviderViewModel
    {
        public IList<SelectListItem> QueryOptions { get; set; }
        public int? SelectedQueryId { get; set; }
        public string DisplayType { get; set; }
    }
}