using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.Environment.Extensions;
using Orchard.ImportExport.Models;

namespace Orchard.ImportExport.Services {
    [OrchardFeature("Orchard.Deployment")]
    public class ItemDeploymentHistory : IItemDeploymentHistory {
        public IEnumerable<ItemDeploymentEntry> GetHistory(IContent contentItem) {
            return contentItem.GetDeploymentHistory();
        }

        public void AddEntry(IContent contentItem, ItemDeploymentEntry entry) {
            contentItem.AddDeploymentHistoryEntry(entry);
        }
    }

    public static class ItemDeploymentHistoryHelper {
        private const string PropertyName = "orchardItemDeploymentHistory";

        public static IEnumerable<ItemDeploymentEntry> GetDeploymentHistory(this IContent contentItem) {
            var infosetPart = contentItem.As<InfosetPart>();
            if (infosetPart == null || infosetPart.Infoset == null || infosetPart.Infoset.Element == null) {
                return new ItemDeploymentEntry[] { };
            }
            var el = infosetPart.Infoset.Element.Element(PropertyName);
            if (el == null) return new ItemDeploymentEntry[] { };
            var history = el.Elements("entry")
                .Select(entryElement => {
                    var completedUtcString = entryElement.Attr("completed");
                    DateTime? completedUtc = null;
                    DateTime completedUtcAttempt;
                    if (DateTime.TryParse(completedUtcString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out completedUtcAttempt)) {
                        completedUtc = completedUtcAttempt;
                    }
                    DeploymentStatus status;
                    Enum.TryParse(entryElement.Attr("status"), out status);
                    return new ItemDeploymentEntry {
                        DeploymentCompletedUtc = completedUtc,
                        Status = status,
                        TargetId = entryElement.Attr<int>("target"),
                        Description = entryElement.Value
                    };
                });
            return history;
        }

        public static void AddDeploymentHistoryEntry(this IContent contentItem, ItemDeploymentEntry entry) {
            var infosetPart = contentItem.As<InfosetPart>();
            if (infosetPart == null || infosetPart.Infoset == null || infosetPart.Infoset.Element == null) return;
            var el = infosetPart.Infoset.Element.Element(PropertyName);
            if (el == null) {
                el = new XElement(PropertyName);
                infosetPart.Infoset.Element.Add(el);
            }
            el.Add(new XElement("entry",
                new XAttribute("completed", entry.DeploymentCompletedUtc == null ? "null" :
                    entry.DeploymentCompletedUtc.Value.ToUniversalTime().ToString(CultureInfo.InvariantCulture)),
                new XAttribute("status", entry.Status),
                new XAttribute("target", entry.TargetId),
                new XText(entry.Description)
                ));
        }
    }
}