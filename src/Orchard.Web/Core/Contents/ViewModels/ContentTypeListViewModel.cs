using System;
using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Mvc.ViewModels;

namespace Orchard.Core.Contents.ViewModels {
    public class ContentTypeListViewModel : BaseViewModel {
        public IEnumerable<ContentTypeDefinition> Types { get; set; }
    }
}