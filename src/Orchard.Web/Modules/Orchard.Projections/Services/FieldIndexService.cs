using System;
using System.Linq;
using Orchard.Projections.Models;

namespace Orchard.Projections.Services {
    public class FieldIndexService : IFieldIndexService {

        public void Set(FieldIndexPart part, string partName, string fieldName, string valueName, object value, Type valueType) =>
            Set(part, partName, fieldName, valueName, value, valueType, FieldIndexRecordVersionOptions.Value);

        public void Set(FieldIndexPart part, string partName, string fieldName, string valueName, object value, Type valueType,
            FieldIndexRecordVersionOptions fieldIndexRecordVersionOption) {
            var propertyName = string.Join(".", partName, fieldName, valueName ?? "");

            var typeCode = Type.GetTypeCode(valueType);

            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                typeCode = Type.GetTypeCode(Nullable.GetUnderlyingType(valueType));
            }

            switch (typeCode) {
                case TypeCode.Char:
                case TypeCode.String:
                    var stringRecord = part.Record.StringFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (stringRecord == null) {
                        stringRecord = new StringFieldIndexRecord { PropertyName = propertyName };
                        part.Record.StringFieldIndexRecords.Add(stringRecord);
                    }

                    // Take the first 4000 chars as it is the limit for the field.
                    var stringRecordValue = value?.ToString().Substring(0, Math.Min(value.ToString().Length, 4000));
                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            stringRecord.Value = stringRecordValue;

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            stringRecord.LatestValue = stringRecordValue;

                            break;
                    }

                    break;
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    var integerRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (integerRecord == null) {
                        integerRecord = new IntegerFieldIndexRecord { PropertyName = propertyName };
                        part.Record.IntegerFieldIndexRecords.Add(integerRecord);
                    }

                    var integerRecordValue = value == null ? default(long?) : Convert.ToInt64(value);
                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            integerRecord.Value = integerRecordValue;

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            integerRecord.LatestValue = integerRecordValue;

                            break;
                    }

                    break;
                case TypeCode.DateTime:
                    var dateTimeRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (dateTimeRecord == null) {
                        dateTimeRecord = new IntegerFieldIndexRecord { PropertyName = propertyName };
                        part.Record.IntegerFieldIndexRecords.Add(dateTimeRecord);
                    }

                    var dateTimeRecordValue = value == null ? default(long?) : ((DateTime)value).Ticks;
                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            dateTimeRecord.Value = dateTimeRecordValue;

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            dateTimeRecord.LatestValue = dateTimeRecordValue;

                            break;
                    }

                    break;
                case TypeCode.Boolean:
                    var booleanRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (booleanRecord == null) {
                        booleanRecord = new IntegerFieldIndexRecord { PropertyName = propertyName };
                        part.Record.IntegerFieldIndexRecords.Add(booleanRecord);
                    }

                    var booleanRecordValue = value == null ? default(long?) : Convert.ToInt64((bool)value);
                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            booleanRecord.Value = booleanRecordValue;

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            booleanRecord.LatestValue = booleanRecordValue;

                            break;
                    }

                    break;
                case TypeCode.Decimal:
                    var decimalRecord = part.Record.DecimalFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (decimalRecord == null) {
                        decimalRecord = new DecimalFieldIndexRecord { PropertyName = propertyName };
                        part.Record.DecimalFieldIndexRecords.Add(decimalRecord);
                    }

                    var decimalRecordValue = value == null ? default(decimal?) : Convert.ToDecimal((decimal)value);
                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            decimalRecord.Value = decimalRecordValue;

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            decimalRecord.LatestValue = decimalRecordValue;

                            break;
                    }

                    break;
                case TypeCode.Single:
                case TypeCode.Double:
                    var doubleRecord = part.Record.DoubleFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (doubleRecord == null) {
                        doubleRecord = new DoubleFieldIndexRecord { PropertyName = propertyName };
                        part.Record.DoubleFieldIndexRecords.Add(doubleRecord);
                    }

                    var doubleRecordValue = value == null ? default(double?) : Convert.ToDouble(value);
                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            doubleRecord.Value = doubleRecordValue;

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            doubleRecord.LatestValue = doubleRecordValue;

                            break;
                    }

                    break;
            }
        }

        public T Get<T>(FieldIndexPart part, string partName, string fieldName, string valueName) =>
            Get<T>(part, partName, fieldName, valueName, FieldIndexRecordVersionOptions.Value);

        public T Get<T>(FieldIndexPart part, string partName, string fieldName, string valueName, FieldIndexRecordVersionOptions fieldIndexRecordVersionOption) {
            var propertyName = string.Join(".", partName, fieldName, valueName ?? "");

            var typeCode = Type.GetTypeCode(typeof(T));

            switch (typeCode) {
                case TypeCode.Char:
                case TypeCode.String:
                    var stringRecord = part.Record.StringFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    var stringRecordValue = default(T);

                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            stringRecordValue = (T)Convert.ChangeType(stringRecord.Value, typeof(T));

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            stringRecordValue = (T)Convert.ChangeType(stringRecord.LatestValue, typeof(T));

                            break;
                    }

                    return stringRecord != null ? stringRecordValue : default;
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    var integerRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    var integerRecordValue = default(T);

                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            integerRecordValue = (T)Convert.ChangeType(integerRecord.Value, typeof(T));

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            integerRecordValue = (T)Convert.ChangeType(integerRecord.LatestValue, typeof(T));

                            break;
                    }

                    return integerRecord != null ? integerRecordValue : default;
                case TypeCode.Decimal:
                    var decimalRecord = part.Record.DecimalFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    var decimalRecordValue = default(T);

                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            decimalRecordValue = (T)Convert.ChangeType(decimalRecord.Value, typeof(T));

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            decimalRecordValue = (T)Convert.ChangeType(decimalRecord.LatestValue, typeof(T));

                            break;
                    }

                    return decimalRecord != null ? decimalRecordValue : default;
                case TypeCode.Single:
                case TypeCode.Double:
                    var doubleRecord = part.Record.DoubleFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    var doubleRecordValue = default(T);

                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            doubleRecordValue = (T)Convert.ChangeType(doubleRecord.Value, typeof(T));

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            doubleRecordValue = (T)Convert.ChangeType(doubleRecord.LatestValue, typeof(T));

                            break;
                    }

                    return doubleRecord != null ? doubleRecordValue : default;
                case TypeCode.DateTime:
                    var dateTimeRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    var dateTimeRecordValue = default(T);

                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            dateTimeRecordValue = (T)Convert.ChangeType(new DateTime(Convert.ToInt64(dateTimeRecord.Value)), typeof(T));

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            dateTimeRecordValue = (T)Convert.ChangeType(new DateTime(Convert.ToInt64(dateTimeRecord.LatestValue)), typeof(T));

                            break;
                    }

                    return dateTimeRecord != null ? dateTimeRecordValue : default;
                case TypeCode.Boolean:
                    var booleanRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    var booleanRecordValue = default(T);

                    switch (fieldIndexRecordVersionOption) {
                        case FieldIndexRecordVersionOptions.Value:
                            booleanRecordValue = (T)Convert.ChangeType(booleanRecord.Value, typeof(T));

                            break;
                        case FieldIndexRecordVersionOptions.LatestValue:
                            booleanRecordValue = (T)Convert.ChangeType(booleanRecord.LatestValue, typeof(T));

                            break;
                    }

                    return booleanRecord != null ? booleanRecordValue : default;
                default:
                    return default;
            }
        }
    }
}
