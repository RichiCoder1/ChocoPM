using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ChocoPM.Extensions
{
    public static class LinqExtensions
    {
        private static readonly ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>> _cachedTypes =
            new ConcurrentDictionary<Type, ConcurrentDictionary<string, PropertyInfo>>();

        private static PropertyInfo GetProperty<T>(string propertyName)
        {
            var type = typeof(T);
            ConcurrentDictionary<string, PropertyInfo> propDic;
            if (!_cachedTypes.TryGetValue(type, out propDic))
            {
                propDic = new ConcurrentDictionary<string, PropertyInfo>();
                _cachedTypes.TryAdd(type, propDic);
            }

            PropertyInfo targetProp;
            if (!propDic.TryGetValue(propertyName, out targetProp))
            {
                targetProp = type.GetProperty(propertyName);
                if (targetProp == null)
                    return null;
                propDic.TryAdd(propertyName, targetProp);
            }
            return targetProp;
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> query, string propertyName)
        {
            if ((new OrderByVistor(query.Expression)).HasOrderBy)
                throw new InvalidOperationException("You can't call OrderBy on a query that is already ordered. Try using ThenBy.");

            var targetProp = GetProperty<T>(propertyName);
            if (targetProp == null)
                throw new ArgumentException("There is no property with that name");

            ParameterExpression[] parameters = new ParameterExpression[] {
                Expression.Parameter(query.ElementType, "query") };

            var queryExpr = query.Expression;
            queryExpr = Expression.Call(
                typeof(Queryable),
                "OrderBy", new Type[] { query.ElementType, targetProp.PropertyType },
                queryExpr,
                Expression.Lambda(Expression.Property(parameters[0], targetProp), parameters[0]));

            return query.Provider.CreateQuery<T>(queryExpr);
        }

        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> query, string propertyName)
        {
            if ((new OrderByVistor(query.Expression)).HasOrderBy)
                throw new InvalidOperationException("You can't call OrderByDescending on a query that is already ordered. Try using ThenByDescending.");

            var targetProp = GetProperty<T>(propertyName);
            if (targetProp == null)
                throw new ArgumentException("There is no property with that name");

            ParameterExpression[] parameters = new ParameterExpression[] {
                Expression.Parameter(query.ElementType, "query") };

            var queryExpr = query.Expression;
            queryExpr = Expression.Call(typeof(Queryable), "OrderByDescending", new Type[] { query.ElementType, targetProp.PropertyType }, query.Expression, Expression.Lambda(Expression.Property(parameters[0], targetProp), parameters[0]));
            return query.Provider.CreateQuery<T>(queryExpr);
        }

        public static IQueryable<T> ThenBy<T>(this IQueryable<T> query, string propertyName)
        {
            if (!(new OrderByVistor(query.Expression)).HasOrderBy)
                throw new InvalidOperationException("You can't call ThenBy on a query that isnt already ordered. First call OrderBy.");

            var targetProp = GetProperty<T>(propertyName);
            if (targetProp == null)
                throw new ArgumentException("There is no property with that name");

            ParameterExpression[] parameters = new ParameterExpression[] {
                Expression.Parameter(query.ElementType, "query") };

            var queryExpr = query.Expression;
            queryExpr = Expression.Call(typeof(Queryable), "ThenBy", new Type[] { query.ElementType, targetProp.PropertyType }, queryExpr, Expression.Lambda(Expression.Property(parameters[0], targetProp), parameters[0]));
            return query.Provider.CreateQuery<T>(queryExpr);
        }

        public static IQueryable<T> ThenByDescending<T>(this IQueryable<T> query, string propertyName)
        {
            if (!(new OrderByVistor(query.Expression)).HasOrderBy)
                throw new InvalidOperationException("You can't call ThenByDescending on a query that isnt already ordered. First call OrderByDescending.");

            var targetProp = GetProperty<T>(propertyName);
            if (targetProp == null)
                throw new ArgumentException("There is no property with that name");

            ParameterExpression[] parameters = new ParameterExpression[] {
                Expression.Parameter(query.ElementType, "query") };

            var queryExpr = query.Expression;
            queryExpr = Expression.Call(typeof(Queryable), "ThenByDescending", new Type[] { query.ElementType, targetProp.PropertyType }, queryExpr, Expression.Lambda(Expression.Property(parameters[0], targetProp), parameters[0]));
            return query.Provider.CreateQuery<T>(queryExpr);
        }

        internal class OrderByVistor : ExpressionVisitor
        {
            private Expression expr;
            public bool HasOrderBy { get; set; }

            public OrderByVistor(Expression queryExpr)
            {
                this.expr = queryExpr;
                this.Visit(queryExpr);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "OrderByDescending" || node.Method.Name == "OrderBy")
                    HasOrderBy = true;

                if (node.CanReduce)
                    return base.VisitMethodCall(node);
                else 
                    return node;
            }

        }

    }
}
