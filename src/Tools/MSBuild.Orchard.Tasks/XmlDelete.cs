using System;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.Orchard.Tasks {
    public class XmlDelete : Task {

        public string Namespace { get; set; }
        public string Prefix { get; set; }
        [Required]
        public string XmlFileName { get; set; }
        [Required]
        public string XPath { get; set; }

        public override bool Execute() {
            try {
                var document = new XmlDocument();
                document.Load(this.XmlFileName);

                var navigator = document.CreateNavigator();
                var nsResolver = new XmlNamespaceManager(navigator.NameTable);

                if (!string.IsNullOrEmpty(this.Prefix) && !string.IsNullOrEmpty(this.Namespace)) {
                    nsResolver.AddNamespace(this.Prefix, this.Namespace);
                }

                var expr = XPathExpression.Compile(this.XPath, nsResolver);

                var iterator = navigator.Select(expr);
                while (iterator.MoveNext()) {
                    iterator.Current.DeleteSelf();
                }

                using (var writer = new XmlTextWriter(this.XmlFileName, Encoding.UTF8)) {
                    writer.Formatting = Formatting.Indented;
                    document.Save(writer);
                    writer.Close();
                }
            }
            catch (Exception exception) {
                base.Log.LogErrorFromException(exception);
                return false;
            }
            base.Log.LogMessage("Updated file '{0}'", new object[] { this.XmlFileName });
            return true;
        }
    }
}

