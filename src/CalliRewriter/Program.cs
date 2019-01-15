using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Linq;
using Mono.Collections.Generic;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.CommandLine;

namespace Veldrid.OpenGLBinding
{
    public class Program
    {
        private static readonly HashSet<string> s_nativeLibs = new HashSet<string>();
        private static readonly List<(string nativeLib, string function, FieldDefinition field)> s_initializedFields
            = new List<(string nativeLib, string function, FieldDefinition field)>();

        private static TypeDefinition s_calliTargetRef;
        private static TypeReference s_intPtrRef;

        public static int Main(string[] args)
        {
            string vkDllPath = null;
            string outputPath = null;
            bool copiedToTemp = false;
            ArgumentSyntax s = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("in", ref vkDllPath, "The location of the assembly to rewrite.");
                syntax.DefineOption("out", ref outputPath, "The output location of the rewritten DLL. If not specified, the DLL is rewritten in-place.");
            });

            if (vkDllPath == null)
            {
                Console.WriteLine("Error: --in is required.");
                Console.WriteLine(s.GetHelpText());
                return -1;
            }
            if (outputPath == null)
            {
                outputPath = vkDllPath + ".new";
                string copyPath = Path.GetTempFileName();
                File.Copy(vkDllPath, copyPath, overwrite: true);
                vkDllPath = copyPath;
                copiedToTemp = true;
            }
            try
            {
                Rewrite(vkDllPath, outputPath);
            }
            finally
            {
                if (copiedToTemp)
                {
                    File.Delete(vkDllPath);
                }
            }
            return 0;
        }

        private static void Rewrite(string inputPath, string outputPath)
        {
            using (AssemblyDefinition dll = AssemblyDefinition.ReadAssembly(inputPath))
            {
                ModuleDefinition mainModule = dll.Modules[0];
                s_calliTargetRef = mainModule.GetType("Veldrid.MetalBindings.CalliTargetAttribute");
                s_intPtrRef = mainModule.TypeSystem.IntPtr;

                foreach (TypeDefinition type in mainModule.Types)
                {
                    ProcessType(type);
                }

                CreateStaticInitializers(mainModule);

                dll.Write(outputPath);
            }
        }

        private static void CreateStaticInitializers(ModuleDefinition module)
        {
            TypeDefinition libHolder = new TypeDefinition("_Internal", "_NativeLibraries", TypeAttributes.Sealed | TypeAttributes.Abstract);
            module.Types.Add(libHolder);

            MethodAttributes staticConstructorAttributes =
                MethodAttributes.Private |
                MethodAttributes.HideBySig |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName |
                MethodAttributes.Static;

            MethodDefinition staticConstructor = new MethodDefinition(".cctor", staticConstructorAttributes, module.TypeSystem.Void);
            libHolder.Methods.Add(staticConstructor);
            libHolder.IsBeforeFieldInit = false;
            ILProcessor il = staticConstructor.Body.GetILProcessor();

            IEnumerable<IGrouping<string, (string nativeLib, string function, FieldDefinition field)>> groups
                = s_initializedFields.GroupBy(tup => tup.nativeLib);
            foreach (IGrouping<string, (string nativeLib, string function, FieldDefinition field)> group in groups)
            {
                string libName = group.Key;
                string libFieldName = GetFieldName(libName);
                FieldDefinition field = new FieldDefinition(libFieldName, FieldAttributes.Static | FieldAttributes.Assembly, module.TypeSystem.IntPtr);
                libHolder.Fields.Add(field);
                il.Emit(OpCodes.Ldc_I4_5);
                il.Emit(OpCodes.Conv_I);
                il.Emit(OpCodes.Stsfld, field);
                il.Emit(OpCodes.Ret);
                //il.Emit(OpCodes.Stsfld, libFieldName);
            }
        }

        private static string GetFieldName(string libName)
        {
            return "s_" + libName.Replace('\\', '_').Replace('/', '_').Replace('.', '_');
        }

        private static void ProcessType(TypeDefinition type)
        {
            foreach (MethodDefinition method in type.Methods)
            {
                ProcessMethod(method);
            }
        }

        private static void ProcessMethod(MethodDefinition method)
        {
            if (method.CustomAttributes.Any(ca => ca.AttributeType == s_calliTargetRef))
            {
                MethodCallingConvention callingConvention = GetCallConv(method.PInvokeInfo);
                string nativeLib = method.PInvokeInfo.Module.Name;
                string entryPoint = method.PInvokeInfo.EntryPoint;
                s_nativeLibs.Add(nativeLib);
                method.IsPInvokeImpl = false;
                method.Body = new MethodBody(method);
                ILProcessor processor = method.Body.GetILProcessor();
                RewriteMethod(method, nativeLib, entryPoint, callingConvention);
                method.CustomAttributes.Remove(method.CustomAttributes.Single(ca => ca.AttributeType == s_calliTargetRef));
            }
        }

        private static MethodCallingConvention GetCallConv(PInvokeInfo info)
        {
            if (info.IsCallConvCdecl) { return MethodCallingConvention.C; }
            else if (info.IsCallConvFastcall) { return MethodCallingConvention.FastCall; }
            else if (info.IsCallConvStdCall) { return MethodCallingConvention.StdCall; }
            else if (info.IsCallConvThiscall) { return MethodCallingConvention.ThisCall; }
            else if (info.IsCallConvWinapi) { return MethodCallingConvention.Default; }

            throw new InvalidOperationException();
        }

        private static void RewriteMethod(
            MethodDefinition method,
            string nativeLib,
            string entryPoint,
            MethodCallingConvention callingConvention)
        {
            ILProcessor il = method.Body.GetILProcessor();
            il.Body.Instructions.Clear();

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                EmitLoadArgument(il, i, method.Parameters);
                TypeReference parameterType = method.Parameters[i].ParameterType;
                if (parameterType.FullName == "System.String")
                {
                    throw new NotImplementedException();
                }
                else if (parameterType.IsByReference)
                {
                    VariableDefinition byRefVariable = new VariableDefinition(new PinnedType(parameterType));
                    method.Body.Variables.Add(byRefVariable);
                    il.Emit(OpCodes.Stloc, byRefVariable);
                    il.Emit(OpCodes.Ldloc, byRefVariable);
                    il.Emit(OpCodes.Conv_I);
                }
            }

            string functionPtrName = method.Name + "_ptr";
            FieldDefinition field = new FieldDefinition(functionPtrName, FieldAttributes.Static | FieldAttributes.Private, s_intPtrRef);
            method.DeclaringType.Fields.Add(field);

            s_initializedFields.Add((nativeLib, entryPoint, field));

            il.Emit(OpCodes.Ldsfld, field);

            CallSite callSite = new CallSite(method.ReturnType)
            {
                CallingConvention = MethodCallingConvention.StdCall
            };
            foreach (ParameterDefinition pd in method.Parameters)
            {
                TypeReference parameterType;
                if (pd.ParameterType.IsByReference)
                {
                    parameterType = new PointerType(pd.ParameterType.GetElementType());
                }
                else if (pd.ParameterType.FullName == "System.String")
                {
                    throw new NotImplementedException();
                }
                else
                {
                    parameterType = pd.ParameterType;
                }
                ParameterDefinition calliPD = new ParameterDefinition(pd.Name, pd.Attributes, parameterType);

                callSite.Parameters.Add(calliPD);
            }
            il.Emit(OpCodes.Calli, callSite);

            il.Emit(OpCodes.Ret);

            if (method.Body.Variables.Count > 0)
            {
                method.Body.InitLocals = true;
            }
        }

        private static void EmitLoadArgument(ILProcessor il, int i, Collection<ParameterDefinition> parameters)
        {
            if (i == 0)
            {
                il.Emit(OpCodes.Ldarg_0);
            }
            else if (i == 1)
            {
                il.Emit(OpCodes.Ldarg_1);
            }
            else if (i == 2)
            {
                il.Emit(OpCodes.Ldarg_2);
            }
            else if (i == 3)
            {
                il.Emit(OpCodes.Ldarg_3);
            }
            else
            {
                il.Emit(OpCodes.Ldarg, i);
            }
        }
    }
}
