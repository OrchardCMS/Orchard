using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;
using PI.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PI.Authentication.Handlers
{
    public class PIAuthenticationSettingsHandler : ContentHandler
    {
        public PIAuthenticationSettingsHandler()
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<PIAuthenticationSettingsPart>("Site"));
            Filters.Add(new TemplateFilterForPart<PIAuthenticationSettingsPart>("PI", "PIAuthenticationSettings", "PI"));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);

            GroupInfo gi = new GroupInfo(T("PI"));
            
            var t = context.Metadata.EditorGroupInfo.Where(p => p.Name.Equals(T("PI"))).SingleOrDefault();

            if (t == null)
                context.Metadata.EditorGroupInfo.Add(gi);

        }
    }
}