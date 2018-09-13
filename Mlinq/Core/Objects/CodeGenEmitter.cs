using Mlinq.Common;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects
{
    internal static class CodeGenEmitter
    {
        internal static readonly MethodInfo CodeGenEmitter_BinaryEquals = typeof(CodeGenEmitter).GetOnlyDeclaredMethod("BinaryEquals");
        internal static readonly MethodInfo DbDataReader_GetValue = typeof(DbDataReader).GetOnlyDeclaredMethod("GetValue");
        internal static readonly MethodInfo DbDataReader_GetString = typeof(DbDataReader).GetOnlyDeclaredMethod("GetString");
        internal static readonly MethodInfo DbDataReader_GetInt16 = typeof(DbDataReader).GetOnlyDeclaredMethod("GetInt16");
        internal static readonly MethodInfo DbDataReader_GetInt32 = typeof(DbDataReader).GetOnlyDeclaredMethod("GetInt32");
        internal static readonly MethodInfo DbDataReader_GetInt64 = typeof(DbDataReader).GetOnlyDeclaredMethod("GetInt64");
        internal static readonly MethodInfo DbDataReader_GetBoolean = typeof(DbDataReader).GetOnlyDeclaredMethod("GetBoolean");
        internal static readonly MethodInfo DbDataReader_GetDecimal = typeof(DbDataReader).GetOnlyDeclaredMethod("GetDecimal");
        internal static readonly MethodInfo DbDataReader_GetFloat = typeof(DbDataReader).GetOnlyDeclaredMethod("GetFloat");
        internal static readonly MethodInfo DbDataReader_GetDouble = typeof(DbDataReader).GetOnlyDeclaredMethod("GetDouble");
        internal static readonly MethodInfo DbDataReader_GetDateTime = typeof(DbDataReader).GetOnlyDeclaredMethod("GetDateTime");
        internal static readonly MethodInfo DbDataReader_GetGuid = typeof(DbDataReader).GetOnlyDeclaredMethod("GetGuid");
        internal static readonly MethodInfo DbDataReader_GetByte = typeof(DbDataReader).GetOnlyDeclaredMethod("GetByte");
        internal static readonly MethodInfo DbDataReader_IsDBNull = typeof(DbDataReader).GetOnlyDeclaredMethod("IsDBNull");
        internal static readonly Expression DBNull_Value = Expression.Constant(DBNull.Value, typeof(object));

        internal static readonly ParameterExpression DbDataReader_Parameter = Expression.Parameter(typeof(DbDataReader), "shaper");
        internal static readonly ParameterExpression Shaper_Parameter = Expression.Parameter(typeof(Shaper), "shaper");
        internal static readonly Expression Shaper_Reader = Expression.Field(Shaper_Parameter, typeof(Shaper).GetField("Reader"));

        internal static bool BinaryEquals(byte[] left, byte[] right)
        {
            if (null == left)
            {
                return null == right;
            }
            else if (null == right)
            {
                return false;
            }
            if (left.Length != right.Length)
            {
                return false;
            }
            for (var i = 0; i < left.Length; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal static Expression<Func<Shaper, TResult>> BuildShaperLambda<TResult>(Expression body)
        {
            return body == null ? null : Expression.Lambda<Func<Shaper, TResult>>(body, Shaper_Parameter);
        }

        internal static Expression Emit_Reader_GetValue(int ordinal, Type type)
        {
            var result = Expression.Call(Shaper_Reader, DbDataReader_GetValue, Expression.Constant(ordinal));
            return result;
        }

        internal static Expression Emit_Reader_IsDBNull(int ordinal)
        {
            Expression result = Expression.Call(Shaper_Reader, DbDataReader_IsDBNull, Expression.Constant(ordinal));
            return result;
        }

        internal static Expression Emit_Conditional_NotDBNull(Expression result, int ordinal, Type columnType)
        {
            result = Expression.Condition(Emit_Reader_IsDBNull(ordinal), Expression.Constant(columnType.GetDefaultValue(), columnType), result);
            return result;
        }

        internal static MethodInfo GetReaderMethod(Type type, out bool isNullable)
        {
            MethodInfo result;
            isNullable = false;

            var underlyingType = Nullable.GetUnderlyingType(type);
            if (null != underlyingType)
            {
                isNullable = true;
                type = underlyingType;
            }

            var typeCode = Type.GetTypeCode(type);
            switch (typeCode)
            {
                case TypeCode.String:
                    result = DbDataReader_GetString;
                    isNullable = true;
                    break;
                case TypeCode.Int16:
                    result = DbDataReader_GetInt16;
                    break;
                case TypeCode.Int32:
                    result = DbDataReader_GetInt32;
                    break;
                case TypeCode.Int64:
                    result = DbDataReader_GetInt64;
                    break;
                case TypeCode.Boolean:
                    result = DbDataReader_GetBoolean;
                    break;
                case TypeCode.Decimal:
                    result = DbDataReader_GetDecimal;
                    break;
                case TypeCode.Double:
                    result = DbDataReader_GetDouble;
                    break;
                case TypeCode.Single:
                    result = DbDataReader_GetFloat;
                    break;
                case TypeCode.DateTime:
                    result = DbDataReader_GetDateTime;
                    break;
                case TypeCode.Byte:
                    result = DbDataReader_GetByte;
                    break;
                default:
                    if (typeof(Guid) == type)
                    {
                        result = DbDataReader_GetGuid;
                    }
                    else if (typeof(TimeSpan) == type || typeof(DateTimeOffset) == type)
                    {
                        result = DbDataReader_GetValue;
                    }
                    else if (typeof(Object) == type)
                    {
                        result = DbDataReader_GetValue;
                    }
                    else
                    {
                        result = DbDataReader_GetValue;
                        isNullable = true;
                    }
                    break;
            }
            return result;
        }

        internal static Expression Emit_NullConstant(Type type)
        {
            Expression nullConstant;
            if (type.IsNullable())
            {
                nullConstant = Expression.Constant(null, type);
            }
            else
            {
                nullConstant = Expression.Constant(null, typeof(object));
            }
            return nullConstant;
        }
    }
}
