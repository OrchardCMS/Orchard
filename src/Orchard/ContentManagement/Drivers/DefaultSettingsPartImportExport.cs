using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Orchard.ContentManagement.Handlers;
using Orchard.Recipes.Models;
using Orchard.Settings;

namespace Orchard.ContentManagement.Drivers
{
    public class DefaultSettingsPartImportExport
    {
        public static void ExportSettingsPart(ContentPart sitePart, ExportContentContext context) {
            var xAttributes = new List<object>();
            foreach (var property in sitePart.GetType().GetProperties()) {
                var propertyType = property.PropertyType;

                // Supported types (we also know they are not indexed properties).
                if (propertyType == typeof (string) || propertyType == typeof (bool) || propertyType == typeof (int)) {

                    // Exclude read-only properties.
                    if (property.GetSetMethod() != null) {
                        var value = property.GetValue(sitePart, null);
                        if (value == null)
                            continue;

                        xAttributes.Add(new XAttribute(property.Name, value));
                    }
                }
            }

            if(xAttributes.Any()) {
                context.Element(sitePart.PartDefinition.Name).Add(xAttributes.ToArray());
            }
        }

        public static void ImportSettingPart(ContentPart sitePart, XElement element) {
            if(element == null)
                return;
        
            foreach (var attribute in element.Attributes()) {
                var attributeName = attribute.Name.LocalName;
                var attributeValue = attribute.Value;
                
                var property = sitePart.GetType().GetProperty(attributeName);
                if (property == null) {
                    throw new InvalidOperationException(string.Format("Could set setting {0} for part {1} because it was not found.", attributeName, sitePart.PartDefinition.Name));
                }

                var propertyType = property.PropertyType;
                if (propertyType == typeof(string)) {
                    property.SetValue(sitePart, attributeValue, null);
                }
                else if (propertyType == typeof(bool)) {
                    property.SetValue(sitePart, Boolean.Parse(attributeValue), null);
                }
                else if (propertyType == typeof(int)) {
                    property.SetValue(sitePart, Int32.Parse(attributeValue), null);
                }
            }
        }
    }
}

