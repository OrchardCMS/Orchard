using System;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Title.Models;

namespace Contrib.Taxonomies.Models {
    public class TermPart : ContentPart<TermPartRecord>, IComparable<TermPart> {
        public string Name {
            get { return this.As<TitlePart>().Title; }
            set { this.As<TitlePart>().Title = value; }
        }

        public string Slug {
            get { return this.As<AutoroutePart>().DisplayAlias; }
            set { this.As<AutoroutePart>().DisplayAlias = value; }
        }

        public IContent Container {
            get { return this.As<ICommonPart>().Container; }
            set { this.As<ICommonPart>().Container = value; }
        }

        public int TaxonomyId {
            get { return Record.TaxonomyId; }
            set { Record.TaxonomyId = value; }
        }

        /// <summary>
        /// e.g., /; /1/; /1/2/
        /// </summary>
        public string Path {
            get { return Record.Path; }
            set { Record.Path = value; }
        }

        public int Count {
            get { return Record.Count; }
            set { Record.Count = value; }
        }

        public bool Selectable {
            get { return Record.Selectable; }
            set { Record.Selectable = value; }
        }

        public int Weight {
            get { return Record.Weight; }
            set { Record.Weight = value; }
        }

        public string FullPath { get { return String.Concat(Path, Id); } }

        #region IComparable<TermPart> Members

        public int CompareTo(TermPart other) {
            if(other.Path == this.Path) {
                var weight = Weight.CompareTo(other.Weight);
                
                if(weight != 0) {
                    return weight; 
                }
                // if same weight, compare by name
                return this.Name.CompareTo(other.Name);
            } 
            
            return FullPath.CompareTo(other.FullPath);
        }

        #endregion
    }
}