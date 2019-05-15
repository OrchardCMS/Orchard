using System;
using System.Linq;
using Orchard.Projections.Models;

namespace Orchard.Projections.Services {
    public class FieldIndexService : IFieldIndexService {

        public void Set(FieldIndexPart part, string partName, string fieldName, string valueName, object value, Type valueType) {
            Set(part, partName, fieldName, valueName, value, valueType, FieldIndexRecordVersionOptions.Value);
        }

        public void Set(FieldIndexPart part, string partName, string fieldName, string valueName, object value, Type valueType,
            FieldIndexRecordVersionOptions fieldIndexRecordVersionOption) {
            var propertyName = String.Join(".", partName, fieldName, valueName ?? "");

            var typeCode = Type.GetTypeCode(valueType);

            if(valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>)) {
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

                    // take the first 4000 chars as it is the limit for the field
                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        stringRecord.Value = value == null ? null : value.ToString().Substring(0, Math.Min(value.ToString().Length, 4000));
                    }
                    else {
                        stringRecord.LatestValue = value == null ? null : value.ToString().Substring(0, Math.Min(value.ToString().Length, 4000));
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

                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        integerRecord.Value = value == null ? default(long?) : Convert.ToInt64(value);
                    }
                    else {
                        integerRecord.LatestValue = value == null ? default(long?) : Convert.ToInt64(value);
                    }
                    break;
                case TypeCode.DateTime:
                    var dateTimeRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (dateTimeRecord == null) {
                        dateTimeRecord = new IntegerFieldIndexRecord { PropertyName = propertyName };
                        part.Record.IntegerFieldIndexRecords.Add(dateTimeRecord);
                    }

                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        dateTimeRecord.Value = value == null ? default(long?) : ((DateTime)value).Ticks;
                    }
                    else {
                        dateTimeRecord.LatestValue = value == null ? default(long?) : ((DateTime)value).Ticks;
                    }
                    break;
                case TypeCode.Boolean:
                    var booleanRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (booleanRecord == null) {
                        booleanRecord = new IntegerFieldIndexRecord { PropertyName = propertyName };
                        part.Record.IntegerFieldIndexRecords.Add(booleanRecord);
                    }

                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        booleanRecord.Value = value == null ? default(long?) : Convert.ToInt64((bool)value);
                    }
                    else {
                        booleanRecord.LatestValue = value == null ? default(long?) : Convert.ToInt64((bool)value);
                    }
                    break;
                case TypeCode.Decimal:
                    var decimalRecord = part.Record.DecimalFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (decimalRecord == null) {
                        decimalRecord = new DecimalFieldIndexRecord { PropertyName = propertyName };
                        part.Record.DecimalFieldIndexRecords.Add(decimalRecord);
                    }

                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        decimalRecord.Value = value == null ? default(decimal?) : Convert.ToDecimal((decimal)value);
                    }
                    else {
                        decimalRecord.LatestValue = value == null ? default(decimal?) : Convert.ToDecimal((decimal)value);
                    }
                    break;
                case TypeCode.Single:
                case TypeCode.Double:
                    var doubleRecord = part.Record.DoubleFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (doubleRecord == null) {
                        doubleRecord = new DoubleFieldIndexRecord { PropertyName = propertyName };
                        part.Record.DoubleFieldIndexRecords.Add(doubleRecord);
                    }

                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        doubleRecord.Value = value == null ? default(double?) : Convert.ToDouble(value);
                    }
                    else {
                        doubleRecord.LatestValue = value == null ? default(double?) : Convert.ToDouble(value);
                    }
                    break;
            }
        }

        public T Get<T>(FieldIndexPart part, string partName, string fieldName, string valueName) {
            return Get<T>(part, partName, fieldName, valueName, FieldIndexRecordVersionOptions.Value);
        }

        public T Get<T>(FieldIndexPart part, string partName, string fieldName, string valueName, FieldIndexRecordVersionOptions fieldIndexRecordVersionOption) {
            var propertyName = String.Join(".", partName, fieldName, valueName ?? "");

            var typeCode = Type.GetTypeCode(typeof(T));

            switch (typeCode) {
                case TypeCode.Char:
                case TypeCode.String:
                    var stringRecord = part.Record.StringFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        return stringRecord != null ? (T)Convert.ChangeType(stringRecord.Value, typeof(T)) : default(T);
                    }
                    else {
                        return stringRecord != null ? (T)Convert.ChangeType(stringRecord.LatestValue, typeof(T)) : default(T);
                    }
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    var integerRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        return integerRecord != null ? (T)Convert.ChangeType(integerRecord.Value, typeof(T)) : default(T);
                    }
                    else {
                        return integerRecord != null ? (T)Convert.ChangeType(integerRecord.LatestValue, typeof(T)) : default(T);
                    }
                case TypeCode.Decimal:
                    var decimalRecord = part.Record.DecimalFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        return decimalRecord != null ? (T)Convert.ChangeType(decimalRecord.Value, typeof(T)) : default(T);
                    }
                    else {
                        return decimalRecord != null ? (T)Convert.ChangeType(decimalRecord.LatestValue, typeof(T)) : default(T);
                    }
                case TypeCode.Single:
                case TypeCode.Double:
                    var doubleRecord = part.Record.DoubleFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        return doubleRecord != null ? (T)Convert.ChangeType(doubleRecord.Value, typeof(T)) : default(T);
                    }
                    else {
                        return doubleRecord != null ? (T)Convert.ChangeType(doubleRecord.LatestValue, typeof(T)) : default(T);
                    }
                case TypeCode.DateTime:
                    var dateTimeRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        return dateTimeRecord != null ? (T)Convert.ChangeType(new DateTime(Convert.ToInt64(dateTimeRecord.Value)), typeof(T)) : default(T);
                    }
                    else {
                        return dateTimeRecord != null ? (T)Convert.ChangeType(new DateTime(Convert.ToInt64(dateTimeRecord.LatestValue)), typeof(T)) : default(T);
                    }
                case TypeCode.Boolean:
                    var booleanRecord = part.Record.IntegerFieldIndexRecords.FirstOrDefault(r => r.PropertyName == propertyName);
                    if (fieldIndexRecordVersionOption == FieldIndexRecordVersionOptions.Value) {
                        return booleanRecord != null ? (T)Convert.ChangeType(booleanRecord.Value, typeof(T)) : default(T);
                    }
                    else {
                        return booleanRecord != null ? (T)Convert.ChangeType(booleanRecord.LatestValue, typeof(T)) : default(T);
                    }
                default:
                    return default(T);
            }
        }
    }
}
