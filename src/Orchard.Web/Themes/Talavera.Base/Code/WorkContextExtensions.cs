using Orchard;
using Orchard.Widgets.Services;
using Orchard.Conditions.Services;
using System.Collections.Generic;
using System.Text;

namespace Talavera.Base
{
    static public class WorkContextExtensions
    {
        static public IList<string> GetLayerCssClassesAsList(this WorkContext workContext)
        {
            var widgetsService = workContext.Resolve<IWidgetsService>();
            var conditionManager = workContext.Resolve<IConditionManager>();

            var classNames = new List<string>();
            foreach (var layer in widgetsService.GetLayers())
            {
                try
                {
                    if (conditionManager.Matches(layer.LayerRule))
                    {                        
                        classNames.Add(string.Format("layer-{0}", layer.Name.ToLower()));
                    }
                }
                catch
                {
                }
            }
            return classNames;
        }

        static public string GetLayerCssClassesAsString(this WorkContext workContext)
        {
            var widgetsService = workContext.Resolve<IWidgetsService>();
            var conditionManager = workContext.Resolve<IConditionManager>();
            var sb = new StringBuilder();
                        
            foreach (var layer in widgetsService.GetLayers())
            {
                try
                {
                    if (conditionManager.Matches(layer.LayerRule))
                    {                        
                        sb.AppendFormat("layer-{0} ", layer.Name.ToLower());
                    }
                }
                catch
                {
                }
            }
            
            return sb.ToString();
        }
    }
}