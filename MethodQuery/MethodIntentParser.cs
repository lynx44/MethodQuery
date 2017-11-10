using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MethodQuery
{
    /// <summary>
    /// Parses the intent of a method signature
    /// </summary>
    public class MethodIntentParser
    {
        public MethodIntent GetIntent(MethodInfo method)
        {
            return new MethodIntent()
            {
                ReturnType = method.ReturnType.IsGenericType ? method.ReturnType.GetGenericArguments().First() : method.ReturnType,
                Parameters = method.GetParameters()
            };
        }
    }

    public class MethodIntent
    {
        public Type ReturnType { get; set; }
        public ParameterInfo[] Parameters { get; set; }
    }
}
