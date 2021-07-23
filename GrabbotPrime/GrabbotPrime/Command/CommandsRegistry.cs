using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GrabbotPrime.Command
{
    public static class CommandsRegistry
    {
        public static IEnumerable<ICommand> GetCommands()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(ActiveCommand)))
                .OrderByDescending(x => (int)x.GetCustomAttributesData().First(x => x.AttributeType == typeof(ActiveCommand)).ConstructorArguments.First().Value)
                .Select(x => (ICommand)Activator.CreateInstance(x));
        }
    }
}
