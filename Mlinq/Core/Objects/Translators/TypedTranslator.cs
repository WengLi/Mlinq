using Mlinq.Core.LinqPredicate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mlinq.Core.Objects.Translators
{
    internal abstract class TypedTranslator<T_Linq> : Translator
           where T_Linq : Expression
    {
        protected TypedTranslator(params ExpressionType[] nodeTypes)
            : base(nodeTypes)
        {
        }

        internal override Predicate Translate(PredicateConverter parent, Expression linq)
        {
            return TypedTranslate(parent, (T_Linq)linq);
        }

        protected abstract Predicate TypedTranslate(PredicateConverter parent, T_Linq linq);
    }
}
