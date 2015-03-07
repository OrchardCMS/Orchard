using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Orchard.DynamicForms.Validators.Settings;
using Orchard.Layouts.Helpers;

namespace Orchard.DynamicForms.Elements {
    public class Enumeration : LabeledFormElement {
        private readonly Lazy<IEnumerable<SelectListItem>> _options;

        public Enumeration() {
            _options = new Lazy<IEnumerable<SelectListItem>>(GetOptions);
        }

        public IEnumerable<SelectListItem> Options {
            get { return _options.Value; }
        }

        public string InputType {
            get { return this.Retrieve(x => x.InputType, () => "SelectList"); }
            set { this.Store(x => x.InputType, value); }
        }

        public EnumerationValidationSettings ValidationSettings {
            get { return Data.GetModel<EnumerationValidationSettings>(""); }
        }

        private IEnumerable<SelectListItem> GetOptions() {
            return ParseOptionsText();
        }

        private IEnumerable<SelectListItem> ParseOptionsText() {
            var data = this.Retrieve("Options", () => "");
            var lines = Regex.Split(data, @"(?:\r\n|[\r\n])", RegexOptions.Multiline);
            return lines.Select(ParseLine).Where(x => x != null);
        }

        private static SelectListItem ParseLine(string line) {
            if (String.IsNullOrWhiteSpace(line))
                return null;

            var parts = line.Split(':');

            if (parts.Length == 1) {
                var value = parts[0].Trim();
                return new SelectListItem {
                    Text = value,
                    Value = value
                };
            }
            else {
                var text = parts[0].Trim();
                var value = String.Join(":", parts.Skip(1)).Trim();
                return new SelectListItem {
                    Text = text,
                    Value = value
                };
            }
        }
    }
}