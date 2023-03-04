using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CC.CodeGenerator.Toolkit.Query
{
    public static class WhereExtensions
    {
        public static IEnumerable<TSource> Where<TSource, TDto>(this IEnumerable<TSource> source, TDto dto)
        {
            var sProps = typeof(TSource).GetProperties();
            var dProps = typeof(TDto).GetProperties();

            var dtoProps = dProps.Where(x => sProps.Any(y => y.Name == x.Name));

            foreach (var dtoProp in dtoProps)
            {
                //var func = WhereFunc<TSource>(dtoProp.Name);
                //var value = dtoProp.GetValue(dto);
                //source = source.Where(x => func(x, value?.ToString() ?? ""));
            }
            return source;
        }




    }
    public interface IOperation
    {

    }
    public abstract class Operation
    {

    }

    public class StringOper : Operation, IOperation
    {

        public void Like()
        {

        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class StringQuery : Attribute, IOperationQuery
    {
    }

    public interface IOperationQuery
    {
    }
}
