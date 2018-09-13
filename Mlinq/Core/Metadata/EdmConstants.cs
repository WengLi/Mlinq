using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlinq.Common;

namespace Mlinq.Core.Metadata
{
    internal static class EdmConstants
    {
        internal const string EdmNamespace = "Edm";
        internal const string TransientNamespace = "Transient";
        internal const int NumPrimitiveTypes = (int)Metadata.PrimitiveTypeKind.DateTimeOffset + 1;

        internal const int NumBuiltInTypes = (int)BuiltInTypeKind.TypeUsage + 1;

        internal const string Binary = "Binary";
        internal const string Boolean = "Boolean";
        internal const string Byte = "Byte";
        internal const string DateTime = "DateTime";
        internal const string Decimal = "Decimal";
        internal const string Double = "Double";
        internal const string Guid = "Guid";
        internal const string Single = "Single";
        internal const string SByte = "SByte";
        internal const string Int16 = "Int16";
        internal const string Int32 = "Int32";
        internal const string Int64 = "Int64";
        internal const string Money = "Money";
        internal const string Null = "Null";
        internal const string String = "String";
        internal const string DateTimeOffset = "DateTimeOffset";
        internal const string Time = "Time";
        internal const string UInt16 = "UInt16";
        internal const string UInt32 = "UInt32";
        internal const string UInt64 = "UInt64";

        private static ReadOnlyCollection<PrimitiveType> _primitiveTypes;

        private static void InitializePrimitiveTypes()
        {
            if (_primitiveTypes != null)
            {
                return;
            }

            var primitiveTypes = new PrimitiveType[EdmConstants.NumPrimitiveTypes];
            primitiveTypes[(int)PrimitiveTypeKind.Binary] = GetPrimitiveType(PrimitiveTypeKind.Binary, EdmConstants.Binary, typeof(Byte[]));
            primitiveTypes[(int)PrimitiveTypeKind.Boolean] = GetPrimitiveType(PrimitiveTypeKind.Boolean, EdmConstants.Boolean, typeof(Boolean));
            primitiveTypes[(int)PrimitiveTypeKind.Byte] = GetPrimitiveType(PrimitiveTypeKind.Byte, EdmConstants.Byte, typeof(Byte));
            primitiveTypes[(int)PrimitiveTypeKind.DateTime] = GetPrimitiveType(PrimitiveTypeKind.DateTime, EdmConstants.DateTime, typeof(DateTime));
            primitiveTypes[(int)PrimitiveTypeKind.Decimal] = GetPrimitiveType(PrimitiveTypeKind.Decimal, EdmConstants.Decimal, typeof(Decimal));
            primitiveTypes[(int)PrimitiveTypeKind.Double] = GetPrimitiveType(PrimitiveTypeKind.Double, EdmConstants.Double, typeof(Double));
            primitiveTypes[(int)PrimitiveTypeKind.Single] = GetPrimitiveType(PrimitiveTypeKind.Single, EdmConstants.Single, typeof(Single));
            primitiveTypes[(int)PrimitiveTypeKind.Guid] = GetPrimitiveType(PrimitiveTypeKind.Guid, EdmConstants.Guid, typeof(Guid));
            primitiveTypes[(int)PrimitiveTypeKind.Int16] = GetPrimitiveType(PrimitiveTypeKind.Int16, EdmConstants.Int16, typeof(Int16));
            primitiveTypes[(int)PrimitiveTypeKind.Int32] = GetPrimitiveType(PrimitiveTypeKind.Int32, EdmConstants.Int32, typeof(Int32));
            primitiveTypes[(int)PrimitiveTypeKind.Int64] = GetPrimitiveType(PrimitiveTypeKind.Int64, EdmConstants.Int64, typeof(Int64));
            primitiveTypes[(int)PrimitiveTypeKind.SByte] = GetPrimitiveType(PrimitiveTypeKind.SByte, EdmConstants.SByte, typeof(SByte));
            primitiveTypes[(int)PrimitiveTypeKind.String] = GetPrimitiveType(PrimitiveTypeKind.String, EdmConstants.String, typeof(String));
            primitiveTypes[(int)PrimitiveTypeKind.Time] = GetPrimitiveType(PrimitiveTypeKind.Time, EdmConstants.Time, typeof(TimeSpan));
            primitiveTypes[(int)PrimitiveTypeKind.DateTimeOffset] = GetPrimitiveType(PrimitiveTypeKind.DateTimeOffset, EdmConstants.DateTimeOffset, typeof(DateTimeOffset));

            _primitiveTypes = new ReadOnlyCollection<PrimitiveType>(primitiveTypes);
        }

        private static PrimitiveType GetPrimitiveType(PrimitiveTypeKind primitiveTypeKind, string name, Type clrType)
        {
            PrimitiveType primitiveType = new PrimitiveType();
            EdmType.Initialize(primitiveType, name, EdmConstants.EdmNamespace, true, null);
            PrimitiveType.Initialize(primitiveType, primitiveTypeKind);
            return primitiveType;
        }

        internal static PrimitiveType GetPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        {
            InitializePrimitiveTypes();
            return _primitiveTypes[(int)primitiveTypeKind];
        }

        internal static bool TryGetPrimitiveTypeKind(Type clrType, out PrimitiveTypeKind resolvedPrimitiveTypeKind)
        {
            PrimitiveTypeKind? primitiveTypeKind = null;
            if (!clrType.IsEnum())
            {
                switch (Type.GetTypeCode(clrType))
                {
                    case TypeCode.Boolean:
                        primitiveTypeKind = PrimitiveTypeKind.Boolean;
                        break;
                    case TypeCode.Byte:
                        primitiveTypeKind = PrimitiveTypeKind.Byte;
                        break;
                    case TypeCode.DateTime:
                        primitiveTypeKind = PrimitiveTypeKind.DateTime;
                        break;
                    case TypeCode.Decimal:
                        primitiveTypeKind = PrimitiveTypeKind.Decimal;
                        break;
                    case TypeCode.Double:
                        primitiveTypeKind = PrimitiveTypeKind.Double;
                        break;
                    case TypeCode.Int16:
                        primitiveTypeKind = PrimitiveTypeKind.Int16;
                        break;
                    case TypeCode.Int32:
                        primitiveTypeKind = PrimitiveTypeKind.Int32;
                        break;
                    case TypeCode.Int64:
                        primitiveTypeKind = PrimitiveTypeKind.Int64;
                        break;
                    case TypeCode.SByte:
                        primitiveTypeKind = PrimitiveTypeKind.SByte;
                        break;
                    case TypeCode.Single:
                        primitiveTypeKind = PrimitiveTypeKind.Single;
                        break;
                    case TypeCode.String:
                        primitiveTypeKind = PrimitiveTypeKind.String;
                        break;
                    case TypeCode.Object:
                        {
                            if (typeof(byte[]) == clrType)
                            {
                                primitiveTypeKind = PrimitiveTypeKind.Binary;
                            }
                            else if (typeof(DateTimeOffset) == clrType)
                            {
                                primitiveTypeKind = PrimitiveTypeKind.DateTimeOffset;
                            }
                            else if (typeof(Guid) == clrType)
                            {
                                primitiveTypeKind = PrimitiveTypeKind.Guid;
                            }
                            else if (typeof(TimeSpan) == clrType)
                            {
                                primitiveTypeKind = PrimitiveTypeKind.Time;
                            }
                            break;
                        }
                }
            }

            if (primitiveTypeKind.HasValue)
            {
                resolvedPrimitiveTypeKind = primitiveTypeKind.Value;
                return true;
            }
            else
            {
                resolvedPrimitiveTypeKind = default(PrimitiveTypeKind);
                return false;
            }
        }
    }
}
