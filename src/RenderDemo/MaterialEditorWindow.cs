using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid.Assets;
using Veldrid.Graphics;

namespace Veldrid.RenderDemo
{
    class MaterialEditorWindow : SwappableRenderItem
    {
        private static readonly string[] s_stages = new[] { "Overlay" };

        private readonly ObjectEditor _editor = new ObjectEditor();

        private TestItem _testItem = new TestItem();
        private MaterialAsset _materialAsset = new MaterialAsset();
        public MaterialAsset MaterialAsset => _materialAsset;

        public void ChangeRenderContext(RenderContext rc)
        {
        }

        public RenderOrderKey GetRenderOrderKey()
        {
            return new RenderOrderKey();
        }

        public IEnumerable<string> GetStagesParticipated()
        {
            return s_stages;
        }

        public void Render(RenderContext context, string pipelineStage)
        {
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(260, 600), SetCondition.Always);
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(context.Window.Width - 268, 8), SetCondition.Always);
            if (ImGui.BeginWindow("Editor Window", WindowFlags.NoMove | WindowFlags.NoResize))
            {
                _editor.DrawObject("Material", ref _materialAsset);
            }
            ImGui.EndWindow();
        }
    }

    public class ObjectEditor
    {
        public void DrawObject<T>(string label, ref T obj)
        {
            object orig = obj;
            object o = obj;
            DrawObject(label, ref o);
            obj = (T)o;
        }

        private void DrawObject(string label, ref object obj)
        {
            Drawer drawer = DrawerCache.GetDrawer(obj.GetType());
            drawer.Draw(label, ref obj);
        }
    }

    public abstract class Drawer
    {
        public Type TypeDrawn { get; }

        public abstract bool Draw(string label, ref object obj);
        public Drawer(Type type)
        {
            TypeDrawn = type;
        }
    }

    public static class DrawerCache
    {
        private static Dictionary<Type, Drawer> s_drawers = new Dictionary<Type, Drawer>()
        {
            { typeof(int), new FuncDrawer<int>(GenericDrawFuncs.DrawInt) },
            { typeof(float), new FuncDrawer<float>(GenericDrawFuncs.DrawSingle) },
            { typeof(string), new FuncDrawer<string>(GenericDrawFuncs.DrawString) },
        };

        public static Drawer GetDrawer(Type type)
        {
            Drawer d;
            if (!s_drawers.TryGetValue(type, out d))
            {
                d = CreateDrawer(type);
                s_drawers.Add(type, d);
            }

            return d;
        }

        private static Drawer CreateDrawer(Type type)
        {
            TypeInfo ti = type.GetTypeInfo();
            if (ti.IsEnum)
            {
                return new EnumDrawer(type);
            }
            else if (ti.IsAbstract)
            {
                return new NullItemDrawer(type);
            }
            else
            {
                return new ComplexItemDrawer(type);
            }
        }
    }

    public abstract class Drawer<T> : Drawer
    {
        public Drawer() : base(typeof(T)) { }

        public sealed override bool Draw(string label, ref object obj)
        {
            T tObj;
            try
            {
                tObj = (T)obj;
            }
            catch (InvalidCastException)
            {
                throw new InvalidOperationException($"Invalid type given to Drawer<{typeof(T).Name}>. {obj.GetType().Name} is not a compatible type.");
            }

            bool result = Draw(label, ref tObj);
            obj = tObj;
            return result;
        }

        public abstract bool Draw(string label, ref T obj);
    }

    public delegate bool DrawFunc<T>(string label, ref T obj);
    public class FuncDrawer<T> : Drawer<T>
    {
        private readonly DrawFunc<T> _drawFunc;

        public FuncDrawer(DrawFunc<T> drawFunc)
        {
            _drawFunc = drawFunc;
        }

        public override bool Draw(string label, ref T obj)
        {
            return _drawFunc(label, ref obj);
        }
    }

    public static class GenericDrawFuncs
    {
        public static unsafe bool DrawString(string label, ref string s)
        {
            bool result = false;

            if (s == null)
            {
                s = "";
                result = true;
            }

            IntPtr stringStorage = Marshal.AllocHGlobal(200);
            for (int i = 0; i < 200; i++) { ((byte*)stringStorage.ToPointer())[i] = 0; }
            IntPtr ansiStringPtr = Marshal.StringToHGlobalAnsi(s);
            SharpDX.Utilities.CopyMemory(stringStorage, ansiStringPtr, s.Length);
            result |= ImGui.InputText(label, stringStorage, 200, InputTextFlags.Default, null);
            if (result)
            {
                string newString = Marshal.PtrToStringAnsi(stringStorage);
                s = newString;
            }
            Marshal.FreeHGlobal(stringStorage);
            Marshal.FreeHGlobal(ansiStringPtr);

            return result;
        }

        public static bool DrawInt(string label, ref int i)
        {
            return ImGui.DragInt(label, ref i, 1f, int.MinValue, int.MaxValue, i.ToString());
        }

        public static bool DrawSingle(string label, ref float f)
        {
            return ImGui.DragFloat(label, ref f, -1000f, 1000f, 1f, f.ToString(), 1f);
        }
    }

    public class EnumDrawer : Drawer
    {
        private readonly string[] _enumOptions;

        public EnumDrawer(Type enumType) : base(enumType)
        {
            _enumOptions = Enum.GetNames(enumType);
        }

        public override bool Draw(string label, ref object obj)
        {
            bool result = false;
            if (ImGui.BeginMenu(label))
            {
                foreach (string item in _enumOptions)
                {
                    if (ImGui.MenuItem(item, ""))
                    {
                        result = true;
                        obj = Enum.Parse(TypeDrawn, item);
                    }
                }
                ImGui.EndMenu();
            }

            return result;
        }
    }

    public class ComplexItemDrawer : Drawer
    {
        private readonly PropertyInfo[] _properties;

        public ComplexItemDrawer(Type type) : base(type)
        {
            _properties = type.GetTypeInfo().GetProperties();
        }

        public override bool Draw(string label, ref object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            bool result = false;

            ImGui.PushID(label);
            if (ImGui.TreeNode(label))
            {
                foreach (PropertyInfo pi in _properties)
                {
                    object originalValue = pi.GetValue(obj);
                    object value = originalValue;
                    Drawer drawer;
                    if (value != null)
                    {
                        drawer = DrawerCache.GetDrawer(value.GetType());
                    }
                    else
                    {
                        drawer = DrawerCache.GetDrawer(pi.PropertyType);
                    }
                    bool changed = drawer.Draw(pi.Name, ref value);
                    if (changed && originalValue != value)
                    {
                        pi.SetValue(obj, value);
                    }
                }

                ImGui.TreePop();
            }
            ImGui.PopID();

            return result;
        }
    }

    public class NullItemDrawer : Drawer
    {
        private readonly Type[] _subTypes;

        public NullItemDrawer(Type type) : base(type)
        {
            _subTypes = type.GetTypeInfo().Assembly.GetTypes().Where(t => type.IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract).ToArray();
        }

        public override bool Draw(string label, ref object obj)
        {
            if (obj != null)
            {
                throw new InvalidOperationException($"{nameof(NullItemDrawer)} should not be used for non-null items.");
            }

            bool result = false;

            ImGui.PushID(label);

            if (ImGui.BeginMenu(label))
            {
                foreach (Type t in _subTypes)
                {
                    if (ImGui.MenuItem(t.Name, ""))
                    {
                        obj = Activator.CreateInstance(t);
                        result = true;
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.PopID();

            return result;
        }
    }
}
