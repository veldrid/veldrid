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
        private static readonly List<(string lib, string function)> s_functionsToLoad = new List<(string lib, string function)>();

        private static TypeDefinition s_calliTargetRef;
        private static TypeReference s_intPtrRef;
        private static TypeDefinition s_calliHelperRef;
        private static MethodDefinition s_loadLibraryRef;
        private static TypeReference s_nativeLibTypeRef;
        private static MethodDefinition s_loadFunctionRef;

        public static int Main(string[] args)
        {
            string inputpath = null;
            string outputPath = null;
            bool copiedToTemp = false;
            ArgumentSyntax s = ArgumentSyntax.Parse(args, syntax =>
            {
                syntax.DefineOption("in", ref inputpath, "The location of the assembly to rewrite.");
                syntax.DefineOption("out", ref outputPath, "The output location of the rewritten DLL. If not specified, the DLL is rewritten in-place.");
            });

            if (inputpath == null)
            {
                Console.WriteLine("Error: --in is required.");
                Console.WriteLine(s.GetHelpText());
                return -1;
            }
            if (outputPath == null)
            {
                outputPath = inputpath;
                string copyPath = Path.GetTempFileName();
                File.Copy(inputpath, copyPath, overwrite: true);
                inputpath = copyPath;
                copiedToTemp = true;
            }
            try
            {
                Rewrite(inputpath, outputPath);
            }
            finally
            {
                if (copiedToTemp)
                {
                    File.Delete(inputpath);
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
                s_calliHelperRef = mainModule.GetType("Veldrid.MetalBindings.CalliRewriteHelper");
                s_loadLibraryRef = s_calliHelperRef.Methods.Single(md => md.Name == "LoadLibrary");
                s_nativeLibTypeRef = s_loadLibraryRef.ReturnType;
                s_loadFunctionRef = s_calliHelperRef.Methods.Single(md => md.Name == "LoadFunction");

                foreach (TypeDefinition type in mainModule.Types)
                {
                    DiscoverNativeLibs(type);
                }

                CreateLibraryLoaders(mainModule);

                foreach (TypeDefinition type in mainModule.Types)
                {
                    ProcessType(type);
                }

                dll.Write(outputPath);
            }
        }

        private static void CreateLibraryLoaders(ModuleDefinition module)
        {
            IEnumerable<IGrouping<string, (string lib, string function)>> groups = s_functionsToLoad.GroupBy(pair => pair.lib);

            foreach (IGrouping<string, (string lib, string function)> group in groups)
            {
                string libName = group.Key;
                string libClassName = GetClassName(libName);
                TypeDefinition libHolder = new TypeDefinition("_Internal", libClassName, TypeAttributes.Sealed | TypeAttributes.Abstract);
                libHolder.IsPublic = true;
                libHolder.BaseType = module.TypeSystem.Object;
                module.Types.Add(libHolder);

                MethodAttributes staticConstructorAttributes =
                    MethodAttributes.Private |
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName |
                    MethodAttributes.RTSpecialName |
                    MethodAttributes.Static;

                MethodDefinition libHolderCctor = new MethodDefinition(".cctor", staticConstructorAttributes, module.TypeSystem.Void);
                libHolder.Methods.Add(libHolderCctor);
                libHolder.IsBeforeFieldInit = false;
                ILProcessor libHolderIL = libHolderCctor.Body.GetILProcessor();

                FieldDefinition nativeLibField = new FieldDefinition("s_nativeLibrary", FieldAttributes.Static | FieldAttributes.Private, s_nativeLibTypeRef);
                libHolder.Fields.Add(nativeLibField);
                libHolderIL.Emit(OpCodes.Ldstr, libName);
                libHolderIL.Emit(OpCodes.Call, s_loadLibraryRef);
                libHolderIL.Emit(OpCodes.Stsfld, nativeLibField);

                foreach ((string lib, string function) in group)
                {
                    string functionPtrName = GetFunctionPointerFieldName(function);
                    FieldDefinition field = libHolder.Fields.SingleOrDefault(fd => fd.Name == functionPtrName);
                    if (field == null)
                    {
                        field = new FieldDefinition(functionPtrName, FieldAttributes.Static | FieldAttributes.Public, s_intPtrRef);
                        libHolder.Fields.Add(field);

                        libHolderIL.Emit(OpCodes.Ldsfld, nativeLibField);
                        libHolderIL.Emit(OpCodes.Ldstr, function);
                        libHolderIL.Emit(OpCodes.Call, s_loadFunctionRef);
                        libHolderIL.Emit(OpCodes.Stsfld, field);
                    }
                }

                libHolderIL.Emit(OpCodes.Ret);
            }
        }

        private static string GetClassName(string libName)
        {
            return "LibHolder_" + libName.Replace('\\', '_').Replace('/', '_').Replace('.', '_');
        }

        private static string GetFunctionPointerFieldName(string function)
        {
            return $"{function}_ptr";
        }

        private static void DiscoverNativeLibs(TypeDefinition type)
        {
            foreach (MethodDefinition method in type.Methods)
            {
                DiscoverNativeLibs(method);
            }
        }

        private static void DiscoverNativeLibs(MethodDefinition method)
        {
            if (method.CustomAttributes.Any(ca => ca.AttributeType == s_calliTargetRef))
            {
                s_functionsToLoad.Add((method.PInvokeInfo.Module.Name, method.PInvokeInfo.EntryPoint));
            }
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
                method.IsPInvokeImpl = false;
                method.Body = new MethodBody(method);
                method.ImplAttributes |= MethodImplAttributes.AggressiveInlining;
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

            TypeDefinition libHolderType = method.Module.GetType("_Internal", GetClassName(nativeLib));
            FieldDefinition libField = libHolderType.Fields.Single(fd => fd.Name == GetFunctionPointerFieldName(entryPoint));

            il.Emit(OpCodes.Ldsfld, libField);

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
