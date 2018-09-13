using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.Core.Metadata
{
    public enum PrimitiveTypeKind
    {
        /// <summary>
        /// Binary Type Kind
        /// </summary>
        Binary = 0,

        /// <summary>
        /// Boolean Type Kind
        /// </summary>
        Boolean = 1,

        /// <summary>
        /// Byte Type Kind
        /// </summary>
        Byte = 2,

        /// <summary>
        /// DateTime Type Kind
        /// </summary>
        DateTime = 3,

        /// <summary>
        /// Decimal Type Kind
        /// </summary>
        Decimal = 4,

        /// <summary>
        /// Double Type Kind
        /// </summary>
        Double = 5,

        /// <summary>
        /// Guid Type Kind
        /// </summary>
        Guid = 6,

        /// <summary>
        /// Single Type Kind
        /// </summary>
        Single = 7,

        /// <summary>
        /// SByte Type Kind
        /// </summary>
        SByte = 8,

        /// <summary>
        /// Int16 Type Kind
        /// </summary>
        Int16 = 9,

        /// <summary>
        /// Int32 Type Kind
        /// </summary>
        Int32 = 10,

        /// <summary>
        /// Int64 Type Kind
        /// </summary>
        Int64 = 11,

        /// <summary>
        /// String Type Kind
        /// </summary>
        String = 12,

        /// <summary>
        /// Time Type Kind
        /// </summary>
        Time = 13,

        /// <summary>
        /// DateTimeOffset Type Kind
        /// </summary>
        DateTimeOffset = 14
    }
}
