using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using JetBrains.Annotations;
using Orchard.Environment.ShellBuilders.Models;

namespace Orchard.ContentManagement.Records {
    class ContentItemAlteration : IAutoMappingAlteration {
        private readonly IEnumerable<RecordBlueprint> _recordDescriptors;

        [UsedImplicitly]
        public ContentItemAlteration() {
            _recordDescriptors = Enumerable.Empty<RecordBlueprint>();
        }

        public ContentItemAlteration(IEnumerable<RecordBlueprint> recordDescriptors) {
            _recordDescriptors = recordDescriptors;
        }

        public void Alter(AutoPersistenceModel model) {

            model.Override<ContentItemRecord>(mapping => {
                foreach (var descriptor in _recordDescriptors.Where(d => Utility.IsPartRecord(d.Type))) {
                    var type = typeof(Alteration<,>).MakeGenericType(typeof(ContentItemRecord), descriptor.Type);
                    var alteration = (IAlteration<ContentItemRecord>)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
                mapping.IgnoreProperty(x => x.Infoset);
            });

            model.Override<ContentItemVersionRecord>(mapping => {
                foreach (var descriptor in _recordDescriptors.Where(d => Utility.IsPartVersionRecord(d.Type))) {
                    var type = typeof(Alteration<,>).MakeGenericType(typeof(ContentItemVersionRecord), descriptor.Type);
                    var alteration = (IAlteration<ContentItemVersionRecord>)Activator.CreateInstance(type);
                    alteration.Override(mapping);
                }
                mapping.IgnoreProperty(x => x.Infoset);
            });
        }

        interface IAlteration<TItemRecord> {
            void Override(AutoMapping<TItemRecord> mapping);
        }

        /// <summary>
        /// Add a "fake" column to the automapping record so that the column can be
        /// referenced when building joins accross content item record tables.
        /// <typeparam name="TItemRecord">Either ContentItemRecord or ContentItemVersionRecord</typeparam>
        /// <typeparam name="TPartRecord">A part record (deriving from TItemRecord)</typeparam>
        /// </summary>
        class Alteration<TItemRecord, TPartRecord> : IAlteration<TItemRecord> {
            public void Override(AutoMapping<TItemRecord> mapping) {

                mapping.References<TPartRecord>(typeof(TItemRecord), typeof(TPartRecord).Name, typeof(TPartRecord).Name)
                    .Access.NoOp()
                    .Column("Id")
                    .ForeignKey("none") // prevent foreign key constraint from ContentItem(Version)Record to TPartRecord
                    .Unique()
                    .Not.Insert()
                    .Not.Update()
                    .Cascade.All();
            }
        }
    }
}