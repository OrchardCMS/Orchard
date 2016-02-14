using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.ContentManagement.MetaData.Builders;

namespace Orchard.ContentManagement.MetaData {
    public static class CommonMetaDataExtensions {
        /// <summary>
        /// Adds IdentityPart to the content type.
        /// </summary>
        /// <returns>The ContentTypeDefinitionBuilder object on which this method is called.</returns>
        public static ContentTypeDefinitionBuilder WithIdentity(this ContentTypeDefinitionBuilder builder) {
            return builder.WithPart("IdentityPart");
        }
    }
}