using System;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.FieldStorage.InfosetStorage;
using Orchard.ContentManagement.Handlers;
using Orchard.Indexing.Settings;
using System.Collections.Generic;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.FieldStorage;

namespace Orchard.Indexing.Handlers {
    public class InfosetFieldIndexingHandler : ContentHandler {

        private readonly IEnumerable<IContentFieldDriver> _contentFieldDrivers;
        private readonly IFieldStorageProvider _fieldStorageProvider;

        public InfosetFieldIndexingHandler(
            IEnumerable<IContentFieldDriver> contentFieldDrivers,
            IFieldStorageProvider fieldStorageProvider) {
            
            _contentFieldDrivers = contentFieldDrivers;
            _fieldStorageProvider = fieldStorageProvider;

            OnIndexing<InfosetPart>(
                (context, cp) => {
                    var infosetPart = context.ContentItem.As<InfosetPart>();
                    if (infosetPart == null) {
                        return;
                    }

                    // part fields
                    foreach ( var part in infosetPart.ContentItem.Parts ) {
                        foreach ( var field in part.PartDefinition.Fields ) {
                            if (!field.Settings.GetModel<FieldIndexing>().Included) {
                                continue;
                            }

                            // get all drivers for the current field type
                            // the driver will describe what values of the field should be indexed
                            var drivers = _contentFieldDrivers.Where(x => x.GetFieldInfo().Any(fi => fi.FieldTypeName == field.FieldDefinition.Name)).ToList();

                            ContentPart localPart = part;
                            ContentPartFieldDefinition localField = field;
                            var fieldStorage = _fieldStorageProvider.BindStorage(localPart, localField);
                            var indexName = infosetPart.TypeDefinition.Name.ToLower() + "-" + field.Name.ToLower();

                            var membersContext = new DescribeMembersContext(fieldStorage, values => {

                                foreach (var value in values) {

                                    if (value == null) {
                                        continue;
                                    }

                                    var t = value.GetType();

                                    // the T is nullable, convert using underlying type
                                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                                        t = Nullable.GetUnderlyingType(t);
                                    }
                                    
                                    var typeCode = Type.GetTypeCode(t);

                                    switch (typeCode) {
                                        case TypeCode.Empty:
                                        case TypeCode.Object:
                                        case TypeCode.DBNull:
                                        case TypeCode.String:
                                        case TypeCode.Char:
                                            context.DocumentIndex.Add(indexName, Convert.ToString(value)).RemoveTags().Analyze();
                                            break;
                                        case TypeCode.Boolean:
                                            context.DocumentIndex.Add(indexName, Convert.ToBoolean(value));
                                            break;
                                        case TypeCode.SByte:
                                        case TypeCode.Int16:
                                        case TypeCode.UInt16:
                                        case TypeCode.Int32:
                                        case TypeCode.UInt32:
                                        case TypeCode.Int64:
                                        case TypeCode.UInt64:
                                            context.DocumentIndex.Add(indexName, Convert.ToInt32(value));
                                            break;
                                        case TypeCode.Single:
                                        case TypeCode.Double:
                                        case TypeCode.Decimal:
                                            context.DocumentIndex.Add(indexName, Convert.ToDouble(value));
                                            break;
                                        case TypeCode.DateTime:
                                            context.DocumentIndex.Add(indexName, Convert.ToDateTime(value));
                                            break;
                                    }
                                }
                            });

                            foreach (var driver in drivers) {
                                driver.Describe(membersContext);
                            }
                        }
                    }
                });
        }
    }
}