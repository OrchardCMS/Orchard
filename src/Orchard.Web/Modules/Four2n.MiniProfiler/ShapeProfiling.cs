// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ShapeProfiling.cs" company="Daniel Dabrowski - rod.42n.pl">
//   Copyright (c) 2008 Daniel Dabrowski - 42n. All rights reserved.
// </copyright>
// <summary>
//   Defines the ShapeProfiling type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Four2n.Orchard.MiniProfiler
{
    using Four2n.Orchard.MiniProfiler.Services;

    using global::Orchard.ContentManagement;

    using global::Orchard.DisplayManagement.Implementation;

    using global::Orchard.DisplayManagement.Shapes;

    public class ShapeProfiling : IShapeFactoryEvents
    {
        private readonly IProfilerService profiler;

        public ShapeProfiling(IProfilerService profiler)
        {
            this.profiler = profiler;
        }

        public void Creating(ShapeCreatingContext context)
        {
        }

        public void Created(ShapeCreatedContext context)
        {
            var shapeMetadata = (ShapeMetadata)context.Shape.Metadata;
/*
            if (shapeMetadata.Type.Equals("Zone") || context.Shape.ContentItem == null)
            {
                return;
            }
*/

            shapeMetadata.OnDisplaying(this.OnDisplaying);
            shapeMetadata.OnDisplayed(this.OnDisplayed);
        }

        public void Displaying(ShapeDisplayingContext context)
        {
            if (context.ShapeMetadata.Type.Equals("Zone"))
            {
                return;
            }

            this.profiler.StepStart(StepKeys.ShapeProfiling, context.ShapeMetadata.Type + " - Display");
        }

        public void Displayed(ShapeDisplayedContext context)
        {
            if (context.ShapeMetadata.Type.Equals("Zone"))
            {
                return;
            }

            this.profiler.StepStop(StepKeys.ShapeProfiling);
        }

        public void OnDisplaying(ShapeDisplayingContext context)
        {
            IContent content = null;
            if (context.Shape.ContentItem != null)
            {
                content = context.Shape.ContentItem;
            }
            else if (context.Shape.ContentPart != null)
            {
                content = context.Shape.ContentPart;
            }

            var message = string.Format(
                "Shape Display: {0} ({1}) ({2})",
                context.ShapeMetadata.Type,
                context.ShapeMetadata.DisplayType,
                (string)(content != null ? content.ContentItem.ContentType : "non-content"));

            this.profiler.StepStart(StepKeys.ShapeProfiling, message, true);
        }

        public void OnDisplayed(ShapeDisplayedContext context)
        {
            this.profiler.StepStop(StepKeys.ShapeProfiling);
        }
    }
}