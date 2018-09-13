using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Mlinq.Core.IServices
{
    public static class SqlQuery
    {
        private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1)
        {
            return f.Method;
        }

        private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2)
        {
            return f.Method;
        }

        public static ISqlQuery<TSource> Where<TSource>(this ISqlQuery<TSource> source, Expression<Func<TSource, bool>> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call(null, GetMethodInfo(Where, source, predicate), new Expression[2]
		    {
			    source.Expression,
			    Expression.Quote(predicate)
		    }));
        }

        public static ISqlQuery<TResult> Select<TSource, TResult>(this ISqlQuery<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (selector == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return source.Provider.CreateQuery<TResult>((Expression)Expression.Call(null, GetMethodInfo(Select, source, selector), new Expression[2]
		    {
			    source.Expression,
			    Expression.Quote(selector)
		    }));
        }

        public static TSource FirstOrDefault<TSource>(this ISqlQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentException("source");
            }
            return source.Provider.Execute<TSource>((Expression)Expression.Call(null, GetMethodInfo(FirstOrDefault, source), source.Expression));
        }

        public static ISqlQuery<TSource> Distinct<TSource>(this ISqlQuery<TSource> source)
        {
            if (source == null)
            {
                throw new ArgumentException("source");
            }
            return source.Provider.CreateQuery<TSource>((Expression)Expression.Call(null, GetMethodInfo(Distinct, source), source.Expression));
        }
    }
}
