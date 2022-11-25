using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Components.GameEvents
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ScriptUnloadedEventAttribute : Attribute
    {
    }
}
