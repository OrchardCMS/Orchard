﻿using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Orchard.UI.Resources {
    public class MetaEntry {
        private readonly TagBuilder _builder = new TagBuilder("meta");

        public static MetaEntry Combine(MetaEntry meta1, MetaEntry meta2, string contentSeparator) {
            var newMeta = new MetaEntry();
            Merge(newMeta._builder.Attributes, meta1._builder.Attributes, meta2._builder.Attributes);
            if (!String.IsNullOrEmpty(meta1.Content) && !String.IsNullOrEmpty(meta2.Content)) {
                newMeta.Content = meta1.Content + contentSeparator + meta2.Content;
            }
            return newMeta;
        }

        private static void Merge(IDictionary<string, string> d1, params IDictionary<string, string>[] sources) {
            foreach(var d in sources) {
                foreach (var pair in d) {
                    d1[pair.Key] = pair.Value;
                }
            }
        }

        public MetaEntry AddAttribute(string name, string value) {
            _builder.MergeAttribute(name, value);
            return this;
        }
        public MetaEntry SetAttribute(string name, string value) {
            _builder.MergeAttribute(name, value, true);
            return this;
        }

        public string Name {
            get {
                string value;
                _builder.Attributes.TryGetValue("name", out value);
                return value;
            }
            set { SetAttribute("name", value); }
        }

        public string Content {
            get {
                string value;
                _builder.Attributes.TryGetValue("content", out value);
                return value;
            }
            set { SetAttribute("content", value); }
        }

        public string HttpEquiv {
            get {
                string value;
                _builder.Attributes.TryGetValue("http-equiv", out value);
                return value;
            }
            set { SetAttribute("http-equiv", value); }
        }

        public string Charset {
            get {
                string value;
                _builder.Attributes.TryGetValue("charset", out value);
                return value;
            }
            set { SetAttribute("charset", value); }
        }

        public string GetTag() {
            return _builder.ToString(TagRenderMode.SelfClosing);
        }
    }
}
