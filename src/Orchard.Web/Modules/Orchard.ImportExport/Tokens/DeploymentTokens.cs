using System;
using Orchard.Core.Settings.Tokens;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;
using Orchard.Localization;

namespace Orchard.ImportExport.Tokens {
    [OrchardFeature("Orchard.Deployment")]
    public class DeploymentTokens : ITokenProvider {
        private readonly IWorkContextAccessor _workContextAccessor;

        public DeploymentTokens(
            IWorkContextAccessor workContextAccessor) {
            _workContextAccessor = workContextAccessor;


            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(dynamic context) {
            context.For("Deployment", T("Deployment"), T("Content deployment tokens"))
                .Token("DeployChangesAfter", T("Deploy changes after"), T("Request for content changed after given date."))
                .Token("ContentTypes", T("Content types"), T("Requested content types"));
        }

        public void Evaluate(dynamic context) {
            if (_workContextAccessor.GetContext().HttpContext == null) {
                return;
            }
            context.For<RecipeRequest>("Deployment", _workContextAccessor.GetContext().GetState<RecipeRequest>("Deployment.RecipeRequest"))
                .Token("DeployChangesAfter", (Func<RecipeRequest, object>)(FormatUtcDateAsLocal))
                .Token("ContentTypes", (Func<RecipeRequest, object>)(FormatContentTypesString));
        }

        private object FormatUtcDateAsLocal(RecipeRequest recipeRequest) {
            if (recipeRequest == null || !recipeRequest.DeployChangesAfterUtc.HasValue)
                return null;

            //DateTimeFilterForm.GetFilterPredicate (used for date field filters) assumes value saved in local time
            //ensure date is Utc
            var date = new DateTime(recipeRequest.DeployChangesAfterUtc.Value.Ticks, DateTimeKind.Utc);

            //DateTimeFilterForm.GetFilterPredicate using simple DateTime.ToUniversalTime() / LocalTime instead of configured timezones 
            //so below line does not work
            //return TimeZoneInfo.ConvertTimeFromUtc(date, _timeZone.Value).ToString("yyyy-MM-dd HH:mm:ss", _cultureInfo.Value);

            //Simple conversion to default local time to match with reverse operation in DateTimeFilterForm.GetFilterPredicate
            return date.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }

        private object FormatContentTypesString(RecipeRequest recipeRequest) {
            if (recipeRequest == null || recipeRequest.ContentTypes == null)
                return null;

            return string.Join(",", recipeRequest.ContentTypes);
        }
    }
}