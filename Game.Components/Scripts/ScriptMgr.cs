using log4net;
using Microsoft.CSharp;
using Microsoft.VisualBasic;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Game.Components.Scripts
{
    public class ScriptMgr
    {
        private static ILog log = LogManager.GetLogger(typeof(ScriptMgr));

        private static readonly Dictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();

        public static Assembly[] Scripts
        {
            get
            {
                lock (_assemblies)
                {
                    return _assemblies.Values.ToArray();
                }
            }
        }

        public static bool InsertAssembly(Assembly assembly)
        {
            lock (_assemblies)
            {
                if (!_assemblies.ContainsKey(assembly.FullName))
                {
                    _assemblies.Add(assembly.FullName, assembly);
                    return true;
                }
                return false;
            }
        }

        public static bool RemoveAssembly(Assembly assembly)
        {
            lock (_assemblies)
            {
                if (!_assemblies.ContainsKey(assembly.FullName))
                    return false;

                return _assemblies.Remove(assembly.FullName);
            }
        }

        public static bool CompileScripts(bool compileVB, string path, string dllName, string[] asm_names)
        {
            if (!path.EndsWith("\\") && !path.EndsWith("/"))
            {
                path += "/";
            }
            ArrayList arrayList = GetScripts(new DirectoryInfo(path), compileVB ? "*.vb" : "*.cs", canLoad: true);
            if (arrayList.Count == 0)
            {
                return true;
            }
            if (File.Exists(dllName))
            {
                File.Delete(dllName);
            }
            CompilerResults compilerResults = null;
            try
            {
                CodeDomProvider codeDomProvider = null;
                codeDomProvider = ((!compileVB) ? ((CodeDomProvider)new CSharpCodeProvider()) : ((CodeDomProvider)new VBCodeProvider()));
                CompilerParameters compilerParameters = new CompilerParameters(asm_names, dllName, includeDebugInformation: true);
                compilerParameters.GenerateExecutable = false;
                compilerParameters.GenerateInMemory = false;
                compilerParameters.WarningLevel = 2;
                compilerParameters.CompilerOptions = "/lib:.";
                string[] array = new string[arrayList.Count];
                for (int i = 0; i < arrayList.Count; i++)
                {
                    array[i] = ((FileInfo)arrayList[i]).FullName;
                }
                compilerResults = codeDomProvider.CompileAssemblyFromFile(compilerParameters, array);
                GC.Collect();
                if (compilerResults.Errors.HasErrors)
                {
                    foreach (CompilerError error in compilerResults.Errors)
                    {
                        if (!error.IsWarning)
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("   ");
                            stringBuilder.Append(error.FileName);
                            stringBuilder.Append(" Line:");
                            stringBuilder.Append(error.Line);
                            stringBuilder.Append(" Col:");
                            stringBuilder.Append(error.Column);
                            if (log.IsWarnEnabled)
                                log.Warn($"Script compilation failed because: : {stringBuilder}");
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                if(log.IsErrorEnabled)
                    log.Error($"CompileScripts: {ex}");
            }
            if (compilerResults != null && !compilerResults.Errors.HasErrors)
            {
                InsertAssembly(compilerResults.CompiledAssembly);
            }
            return true;
        }

        private static ArrayList GetScripts(object files, string name, bool canLoad)
        {
            ArrayList arrayList = new ArrayList();
            if (!((FileSystemInfo)files).Exists)
                return arrayList;

            arrayList.AddRange(((DirectoryInfo)files).GetFiles(name));
            if (canLoad)
            {
                DirectoryInfo[] directories = ((DirectoryInfo)files).GetDirectories();
                foreach (DirectoryInfo directoryInfo in directories)
                    arrayList.AddRange(GetScripts(directoryInfo, name, canLoad));
            }
            return arrayList;
        }

        public static object CreateInstance(string name)
        {
            Assembly[] scripts = Scripts;
            int count = 0;
            Type type;

            while (true)
            {
                if (count < scripts.Length)
                {
                    type = scripts[count].GetType(name);
                    if (type != null && type.IsClass)
                        break;
                    count++;
                    continue;
                }
                return null;
            }
            Console.WriteLine($"{name} = {type.Name}");
            return Activator.CreateInstance(type);
        }

        public static Type[] GetDerivedClass(Type baseType)
        {
            if (baseType == null)
                return new Type[0];

            ArrayList arrayList = new ArrayList();
            foreach (Assembly item in new ArrayList(Scripts))
            {
                Type[] types = item.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsClass && baseType.IsAssignableFrom(type))
                        arrayList.Add(type);
                }
            }
            return (Type[])arrayList.ToArray(typeof(Type));
        }
    }
}
