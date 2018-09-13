using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mlinq.Core.LinqPredicate.DbCommandTrees
{
    public enum DbCommandTreeKind
    {
        /// <summary>
        /// A query to retrieve data
        /// </summary>
        Query,

        /// <summary>
        /// Update existing data
        /// </summary>
        Update,

        /// <summary>
        /// Insert new data
        /// </summary>
        Insert,

        /// <summary>
        /// Deleted existing data
        /// </summary>
        Delete,

        /// <summary>
        /// Call a function
        /// </summary>
        Function,
    }
}
