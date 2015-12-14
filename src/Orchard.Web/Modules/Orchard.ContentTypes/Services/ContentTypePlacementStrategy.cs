using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentTypes.Settings;
using Orchard.DisplayManagement.Descriptors;
using Orchard.ContentTypes.Extensions;
using Orchard.Environment;
using Orchard.Environment.Extensions.Models;

namespace Orchard.ContentTypes.Services {
    public class TypePlacement {
        public PlacementSettings Placement { get; set; }
        public string ContentType { get; set; }
    }

    public class ContentTypePlacementStrategy : IShapeTableEventHandler {
        private readonly Work<IContentDefinitionManager> _contentDefinitionManager;

        public ContentTypePlacementStrategy(Work<IContentDefinitionManager> contentDefinitionManager) {
            _contentDefinitionManager = contentDefinitionManager;
        }
        
        public virtual Feature Feature { get; set; }

        public void ShapeTableCreated(ShapeTable shapeTable) {

            var typeDefinitions = _contentDefinitionManager.Value.ListTypeDefinitions();
            var allPlacements = typeDefinitions.SelectMany(td => td.GetPlacement(PlacementType.Editor).Select(p => new TypePlacement { Placement = p, ContentType = td.Name }) );
            
            // group all placement settings by shape type
            var shapePlacements = allPlacements.GroupBy(x => x.Placement.ShapeType).ToDictionary(x => x.Key, y=> y.ToList());

            // create a new predicate in a ShapeTableDescriptor has a custom placement
            foreach(var shapeType in shapeTable.Descriptors.Keys) {
                List<TypePlacement> customPlacements;
                if(shapePlacements.TryGetValue(shapeType, out customPlacements)) {
                    var descriptor = shapeTable.Descriptors[shapeType];
                    // there are some custom placements, build a predicate
                    var placement = descriptor.Placement;
                        
                    if(!customPlacements.Any()) {
                        continue;
                    }

                    descriptor.Placement = ctx => {
                        if(ctx.DisplayType == null) {
                            foreach(var customPlacement in customPlacements) {
                                
                                var type = customPlacement.ContentType;
                                var differentiator = customPlacement.Placement.Differentiator;

                                if (((ctx.Differentiator ?? String.Empty) == (differentiator ?? String.Empty)) && ctx.ContentType == type) {
                                    
                                    var location = customPlacement.Placement.Zone;
                                    if (!String.IsNullOrEmpty(customPlacement.Placement.Position)) {
                                        location = String.Concat(location, ":", customPlacement.Placement.Position);
                                    }

                                    return new PlacementInfo { Location = location };
                                }
                            }
                        }

                        return placement(ctx);
                    };
                }
            }
        }
    }
}