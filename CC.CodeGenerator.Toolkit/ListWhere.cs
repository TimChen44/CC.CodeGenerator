using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Toolkit
{
    public static class ListWhere
    {
        public static IEnumerable<TSource> DtoWhere<TSource, TDto>(this IEnumerable<TSource> source, TDto dto)
        {

            var sProps = typeof(TSource).GetProperties();
            var dProps = typeof(TDto).GetProperties();

            var dtoProps = dProps.Where(x => sProps.Any(y => y.Name == x.Name));

            foreach (var dtoProp in dtoProps)
            {
                var func = WhereFunc<TSource>(dtoProp.Name);
                var value = dtoProp.GetValue(dto);
                source = source.Where(x => func(x, value?.ToString() ?? ""));
            }
            return source;
        }

        public static Func<TSource, string, bool> WhereFunc<TSource>(string propName)
        {
            ParameterExpression entityPar = Expression.Parameter(typeof(TSource), "x");
            var entityParS1 = Expression.Property(entityPar, propName);
            Console.WriteLine(entityParS1);//x.S1

            //ParameterExpression dtoPar = Expression.Parameter(typeof(Dto), "dto");
            //var dtoParS1 = Expression.Property(dtoPar, "S1");
            //Console.WriteLine(dtoParS1);//dto.S1
            ParameterExpression dtoPar = Expression.Parameter(typeof(string), "value");

            MethodCallExpression containsExp = Expression.Call(entityParS1, typeof(string).GetMethod("Contains", new Type[] { typeof(string) }), dtoPar);
            Console.WriteLine(containsExp);//x.S1.Contains(dto.S1)

            var whereLambda = Expression.Lambda<Func<TSource, string, bool>>(containsExp, entityPar, dtoPar);
            Console.WriteLine(whereLambda);//x => x.S1.Contains(dto.S1)

            return whereLambda.Compile();
        }
    }
}