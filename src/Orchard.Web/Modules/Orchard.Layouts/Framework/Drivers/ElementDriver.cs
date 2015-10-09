using System;
using System.Linq;
using Orchard.Layouts.Framework.Display;
using Orchard.Layouts.Framework.Elements;

namespace Orchard.Layouts.Framework.Drivers {
    public abstract class ElementDriver<TElement> : Component, IElementDriver where TElement: Element {
        public virtual int Priority {
            get { return 0; }
        }

        public EditorResult BuildEditor(ElementEditorContext context) {
            return OnBuildEditor((TElement) context.Element, context);
        }

        public EditorResult UpdateEditor(ElementEditorContext context) {
            return OnUpdateEditor((TElement)context.Element, context);
        }

        public void CreatingDisplay(ElementCreatingDisplayShapeContext context) {
            OnCreatingDisplay((TElement)context.Element, context);
        }

        public void Displaying(ElementDisplayingContext context) {
            OnDisplaying((TElement) context.Element, context);
        }

        public void Displayed(ElementDisplayedContext context) {
            OnDisplayed((TElement)context.Element, context);
        }

        public void LayoutSaving(ElementSavingContext context) {
            OnLayoutSaving((TElement) context.Element, context);
        }

        public void Removing(ElementRemovingContext context) {
            OnRemoving((TElement) context.Element, context);
        }

        public void Exporting(ExportElementContext context) {
            OnExporting((TElement)context.Element, context);
        }

        public void Exported(ExportElementContext context) {
            OnExported((TElement)context.Element, context);
        }

        public void Importing(ImportElementContext context) {
            OnImporting((TElement)context.Element, context);
        }

        public void Imported(ImportElementContext context) {
            OnImported((TElement)context.Element, context);
        }

        public void ImportCompleted(ImportElementContext context) {
            OnImportCompleted((TElement)context.Element, context);
        }

        protected virtual EditorResult OnBuildEditor(TElement element, ElementEditorContext context) {
            return null;
        }

        protected virtual EditorResult OnUpdateEditor(TElement element, ElementEditorContext context) {
            return OnBuildEditor(element, context);
        }

        protected virtual void OnCreatingDisplay(TElement element, ElementCreatingDisplayShapeContext context) {
        }

        protected virtual void OnDisplaying(TElement element, ElementDisplayingContext context) {
        }

        protected virtual void OnDisplayed(TElement element, ElementDisplayedContext context) {
        }

        protected virtual void OnLayoutSaving(TElement element, ElementSavingContext context) {
        }

        protected virtual void OnRemoving(TElement element, ElementRemovingContext context) {
        }

        protected virtual void OnExporting(TElement element, ExportElementContext context) {
        }

        protected virtual void OnExported(TElement element, ExportElementContext context) {
        }

        protected virtual void OnImporting(TElement element, ImportElementContext context) {
        }

        protected virtual void OnImported(TElement element, ImportElementContext context) {
        }

        protected virtual void OnImportCompleted(TElement element, ImportElementContext context) {
        }

        protected EditorResult Editor(ElementEditorContext context, params dynamic[] editorShapes) {
            foreach (var editorShape in editorShapes) {
                if (String.IsNullOrWhiteSpace(editorShape.Metadata.Position)) {
                    editorShape.Metadata.Position = "Properties:0";
                }
            }

            var result = new EditorResult {
                Editors = editorShapes.ToList()
            };

            return result;
        }
    }
}