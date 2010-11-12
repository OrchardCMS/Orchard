using System;
using System.Collections.Generic;
using Orchard.Media.Models;

namespace Orchard.Media.Helpers {
    public static class MediaHelpers {
        public static IEnumerable<FolderNavigation> GetFolderNavigationHierarchy(string mediaPath) {
            List<FolderNavigation> navigations = new List<FolderNavigation>();
            if (String.IsNullOrEmpty(mediaPath)) {
                return navigations;
            }
            if (!mediaPath.Contains("\\")) {
                navigations.Add(new FolderNavigation { FolderName = mediaPath, FolderPath = mediaPath });
                return navigations;
            }

            string[] navigationParts = mediaPath.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            string currentPath = String.Empty;
            foreach (string navigationPart in navigationParts) {
                currentPath = (string.IsNullOrEmpty(currentPath) ? navigationPart : currentPath + "\\" + navigationPart);
                navigations.Add(new FolderNavigation { FolderName = navigationPart, FolderPath = currentPath });
            }

            return navigations;
        }
    }
}
