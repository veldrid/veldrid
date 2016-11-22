using ImGuiNET;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Veldrid.Assets;
using Veldrid.Graphics;
using Veldrid.RenderDemo.Drawers;

namespace Veldrid.RenderDemo
{
    public class AssetEditorWindow
    {
        private object _selectedAsset = null;

        private bool _windowOpened = false;
        private readonly LooseFileDatabase _assetDb;

        private TextInputBuffer _filenameBuffer = new TextInputBuffer(100);
        private string _loadedAssetPath;
        private bool _columnWidthSet;

        public AssetEditorWindow(LooseFileDatabase lfdb)
        {
            _assetDb = lfdb;
        }

        public void Open() => _windowOpened = true;

        public void Render(RenderContext rc)
        {
            if (_windowOpened)
            {
                Vector2 size = ImGui.GetIO().DisplaySize / ImGui.GetIO().DisplayFramebufferScale;
                ImGui.SetNextWindowSize(size - new Vector2(20, 35), SetCondition.Always);
                ImGui.SetNextWindowPos(new Vector2(10, 25), SetCondition.Always);
            
                ImGui.PushStyleVar(StyleVar.WindowRounding, 0f);
                if (ImGui.BeginWindow("Editor Window", ref _windowOpened, WindowFlags.ShowBorders | WindowFlags.NoCollapse | WindowFlags.NoMove | WindowFlags.NoResize))
                {
                    ImGui.Columns(2, "EditorColumns", true);
                    if (!_columnWidthSet)
                    {
                        _columnWidthSet = true;
                        float initialOffset = Math.Min(200, ImGui.GetWindowWidth() * 0.3f);
                        ImGui.SetColumnOffset(1, initialOffset);
                    }

                    DirectoryNode node = _assetDb.GetRootDirectoryGraph();
                    DrawRecursiveNode(node, false);

                    ImGui.NextColumn();
                    if (_selectedAsset != null)
                    {
                        Drawer d = DrawerCache.GetDrawer(_selectedAsset.GetType());
                        d.Draw(_selectedAsset.GetType().Name, ref _selectedAsset, rc);

                        ImGui.Text("Asset Name:");
                        ImGui.PushItemWidth(220);
                        ImGui.SameLine();
                        if (ImGui.InputText(" ", _filenameBuffer.Data, _filenameBuffer.CapacityInBytes, InputTextFlags.Default, null))
                        {
                            _loadedAssetPath = _filenameBuffer.StringValue;
                        }
                        ImGui.PopItemWidth();
                        ImGui.SameLine();
                        if (ImGui.Button("Save"))
                        {
                            string path = _assetDb.GetAssetPath(_loadedAssetPath);
                            using (var fs = File.CreateText(path))
                            {
                                var serializer = JsonSerializer.CreateDefault();
                                serializer.TypeNameHandling = TypeNameHandling.All;
                                serializer.Serialize(fs, _selectedAsset);
                            }
                        }
                    }
                }
                ImGui.EndWindow();
                ImGui.PopStyleVar();
            }
        }

        private void DrawRecursiveNode(DirectoryNode node, bool pushTreeNode)
        {
            if (!pushTreeNode || ImGui.TreeNode(node.FolderName))
            {
                foreach (DirectoryNode child in node.Children)
                {
                    DrawRecursiveNode(child, pushTreeNode: true);
                }

                foreach (AssetInfo asset in node.AssetInfos)
                {
                    if (ImGui.Selectable(asset.Name) && _loadedAssetPath != asset.Path)
                    {
                        _selectedAsset = _assetDb.LoadAsset(asset.Path);
                        _loadedAssetPath = asset.Path;
                        _filenameBuffer.StringValue = asset.Name;
                    }
                    if (ImGui.IsLastItemHovered())
                    {
                        ImGui.SetTooltip(asset.Path);
                    }
                    if (ImGui.BeginPopupContextItem(asset.Name + "_context"))
                    {
                        if (ImGui.Button("Clone"))
                        {
                            _assetDb.CloneAsset(asset.Path);
                        }
                        if (ImGui.Button("Delete"))
                        {
                            _assetDb.DeleteAsset(asset.Path);
                        }
                        ImGui.EndPopup();
                    }
                }

                if (pushTreeNode)
                {
                    ImGui.TreePop();
                }
            }
        }
    }

    public abstract class Drawer
    {
        public Type TypeDrawn { get; }

        public bool Draw(string label, ref object obj, RenderContext rc)
        {
            ImGui.PushID(label);

            bool result;
            if (obj == null)
            {
                result = DrawNewItemSelector(label, ref obj, rc);
            }
            else
            {
                result = DrawNonNull(label, ref obj, rc);
            }

            ImGui.PopID();

            return result;
        }

        protected abstract bool DrawNonNull(string label, ref object obj, RenderContext rc);
        protected virtual bool DrawNewItemSelector(string label, ref object obj, RenderContext rc)
        {
            ImGui.Text(label + ": NULL ");
            ImGui.SameLine();
            if (ImGui.Button($"Create New"))
            {
                obj = CreateNewObject();
                return true;
            }
            if (ImGui.IsLastItemHovered())
            {
                ImGui.SetTooltip($"Create a new {TypeDrawn.Name}.");
            }
            return false;
        }

        public virtual object CreateNewObject()
        {
            try
            {
                return Activator.CreateInstance(TypeDrawn);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error creating instance of " + TypeDrawn, e);
            }
        }

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
            { typeof(byte), new FuncDrawer<byte>(GenericDrawFuncs.DrawByte) },
            { typeof(string), new FuncDrawer<string>(GenericDrawFuncs.DrawString, GenericDrawFuncs.NewString) },
            { typeof(bool), new FuncDrawer<bool>(GenericDrawFuncs.DrawBool) },
            { typeof(ImageSharpTexture), new TextureDrawer() },
            { typeof(ConstructedMeshInfo), new ModelDrawer() }
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
            else if (ti.IsArray)
            {
                return (Drawer)Activator.CreateInstance(typeof(ArrayDrawer<>).MakeGenericType(type.GetElementType()));
            }
            else if (ti.IsAbstract)
            {
                return new AbstractItemDrawer(type);
            }
            else if (type.GetTypeInfo().IsGenericType)
            {
                if (typeof(AssetRef<>).GetTypeInfo().IsAssignableFrom(type.GetGenericTypeDefinition()))
                {
                    return (Drawer)Activator.CreateInstance(typeof(AssetRefDrawer<>).MakeGenericType(type.GenericTypeArguments[0]));
                }
            }

            return new ComplexItemDrawer(type);
        }
    }

    public abstract class Drawer<T> : Drawer
    {
        public Drawer() : base(typeof(T)) { }

        protected sealed override bool DrawNonNull(string label, ref object obj, RenderContext rc)
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

            bool result = Draw(label, ref tObj, rc);
            obj = tObj;
            return result;
        }

        public abstract bool Draw(string label, ref T obj, RenderContext rc);
    }

    public delegate bool DrawFunc<T>(string label, ref T obj, RenderContext rc);
    public class FuncDrawer<T> : Drawer<T>
    {
        private readonly DrawFunc<T> _drawFunc;
        private readonly Func<object> _newFunc;

        public FuncDrawer(DrawFunc<T> drawFunc, Func<object> newFunc = null)
        {
            _drawFunc = drawFunc;
            _newFunc = newFunc;
        }

        public override bool Draw(string label, ref T obj, RenderContext rc)
        {
            return _drawFunc(label, ref obj, rc);
        }

        public override object CreateNewObject()
        {
            if (_newFunc != null)
            {
                return _newFunc();
            }
            else
            {
                return base.CreateNewObject();
            }
        }
    }

    public static class GenericDrawFuncs
    {
        public static unsafe bool DrawString(string label, ref string s, RenderContext rc)
        {
            bool result = false;

            if (s == null)
            {
                s = "";
                result = true;
            }

            byte* stackBytes = stackalloc byte[200];
            IntPtr stringStorage = new IntPtr(stackBytes);
            for (int i = 0; i < 200; i++) { stackBytes[i] = 0; }
            IntPtr ansiStringPtr = Marshal.StringToHGlobalAnsi(s);
            SharpDX.Utilities.CopyMemory(stringStorage, ansiStringPtr, s.Length);
            float stringWidth = ImGui.GetTextSize(label).X;
            ImGui.PushItemWidth(ImGui.GetContentRegionAvailableWidth() - stringWidth - 10);
            result |= ImGui.InputText(label, stringStorage, 200, InputTextFlags.Default, null);
            ImGui.PopItemWidth();
            if (result)
            {
                string newString = Marshal.PtrToStringAnsi(stringStorage);
                s = newString;
            }
            Marshal.FreeHGlobal(ansiStringPtr);

            return result;
        }

        public static object NewString()
        {
            return string.Empty;
        }

        public static bool DrawInt(string label, ref int i, RenderContext rc)
        {
            ImGui.PushItemWidth(50f);
            bool result = ImGui.DragInt(label, ref i, 1f, int.MinValue, int.MaxValue, i.ToString());
            ImGui.PopItemWidth();
            return result;
        }

        public static bool DrawSingle(string label, ref float f, RenderContext rc)
        {
            ImGui.PushItemWidth(50f);
            bool result = ImGui.DragFloat(label, ref f, -1000f, 1000f, 1f, f.ToString(), 1f);
            ImGui.PopItemWidth();
            return result;
        }

        public static bool DrawByte(string label, ref byte b, RenderContext rc)
        {
            ImGui.PushItemWidth(50f);
            int val = b;
            if (ImGui.DragInt(label, ref val, 1f, byte.MinValue, byte.MaxValue, b.ToString()))
            {
                b = (byte)val;
                ImGui.PopItemWidth();
                return true;
            }
            ImGui.PopItemWidth();
            return false;
        }

        public static bool DrawBool(string label, ref bool b, RenderContext rc)
        {
            return ImGui.Checkbox(label, ref b);
        }
    }

    public class EnumDrawer : Drawer
    {
        private readonly string[] _enumOptions;

        public EnumDrawer(Type enumType) : base(enumType)
        {
            _enumOptions = Enum.GetNames(enumType);
        }

        protected override bool DrawNonNull(string label, ref object obj, RenderContext rc)
        {
            bool result = false;
            string menuLabel = $"{label}: {obj.ToString()}";
            if (ImGui.BeginMenu(menuLabel))
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

    public class ArrayDrawer<T> : Drawer
    {
        private readonly bool _isValueType;

        public ArrayDrawer() : base(typeof(T[]))
        {
            _isValueType = typeof(T).GetTypeInfo().IsValueType;
        }

        protected override bool DrawNonNull(string label, ref object obj, RenderContext rc)
        {
            T[] arr = (T[])obj;
            int length = arr.Length;
            bool newArray = false;

            if (ImGui.TreeNode($"{label}[{length}]###{label}"))
            {
                if (ImGui.IsLastItemHovered())
                {
                    ImGui.SetTooltip($"{TypeDrawn.GetElementType()}[{arr.Length}]");
                }

                if (!newArray)
                {
                    if (ImGui.SmallButton("-"))
                    {
                        int newLength = Math.Max(length - 1, 0);
                        Array.Resize(ref arr, newLength);
                        newArray = true;
                    }
                    ImGui.SameLine();
                    ImGui.Spacing();
                    ImGui.SameLine();
                    if (ImGui.SmallButton("+"))
                    {
                        Array.Resize(ref arr, length + 1);
                        newArray = true;
                    }
                }

                length = arr.Length;

                for (int i = 0; i < length; i++)
                {
                    ImGui.PushStyleColor(ColorTarget.Button, RgbaFloat.Red.ToVector4());
                    if (ImGui.Button($"X##{i}", new System.Numerics.Vector2(15, 15)))
                    {
                        ShiftArrayDown(arr, i);
                        Array.Resize(ref arr, length - 1);
                        newArray = true;
                        length -= 1;
                        ImGui.PopStyleColor();
                        i--;
                        continue;
                    }
                    ImGui.PopStyleColor();
                    ImGui.SameLine();
                    object element = arr[i];
                    Drawer drawer;
                    if (element == null)
                    {
                        drawer = DrawerCache.GetDrawer(typeof(T));
                    }
                    else
                    {
                        Type realType = element.GetType();
                        drawer = DrawerCache.GetDrawer(realType);
                    }

                    bool changed = drawer.Draw($"{TypeDrawn.GetElementType().Name}[{i}]", ref element, rc);
                    if (changed || drawer.TypeDrawn.GetTypeInfo().IsValueType)
                    {
                        arr[i] = (T)element;
                    }
                }

                ImGui.TreePop();
            }
            else if (ImGui.IsLastItemHovered())
            {
                ImGui.SetTooltip($"{TypeDrawn.GetElementType()}[{arr.Length}]");
            }

            if (newArray)
            {
                obj = arr;
                return true;
            }

            return false;
        }

        private void ShiftArrayDown(T[] arr, int start)
        {
            for (int i = start; i < arr.Length - 1; i++)
            {
                arr[i] = arr[i + 1];
            }
        }

        public override object CreateNewObject()
        {
            return new T[0];
        }
    }

    public class ComplexItemDrawer : Drawer
    {
        private readonly PropertyInfo[] _properties;
        private readonly bool _drawRootNode;

        public ComplexItemDrawer(Type type) : this(type, true) { }
        public ComplexItemDrawer(Type type, bool drawRootNode)
            : base(type)
        {
            _drawRootNode = drawRootNode;
            _properties = type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        }

        protected override bool DrawNonNull(string label, ref object obj, RenderContext rc)
        {
            ImGui.PushID(label);

            if (!_drawRootNode || ImGui.CollapsingHeader(label, label, true, true))
            {
                foreach (PropertyInfo pi in _properties)
                {
                    if (_drawRootNode)
                    {
                        const int levelMargin = 5;
                        ImGui.PushItemWidth(levelMargin);
                        ImGui.LabelText("", "");
                        ImGui.PopItemWidth();
                        ImGui.SameLine();
                    }

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
                    bool changed = drawer.Draw(pi.Name, ref value, rc);
                    if (changed && originalValue != value)
                    {
                        pi.SetValue(obj, value);
                    }
                }
            }

            ImGui.PopID();

            return false;
        }
    }

    public class AbstractItemDrawer : Drawer
    {
        private readonly Type[] _subTypes;

        public AbstractItemDrawer(Type type) : base(type)
        {
            _subTypes = type.GetTypeInfo().Assembly.GetTypes().Where(t => type.IsAssignableFrom(t) && !t.GetTypeInfo().IsAbstract).ToArray();
        }

        protected override bool DrawNonNull(string label, ref object obj, RenderContext rc)
        {
            throw new InvalidOperationException("AbstractItemDrawer shouldn't be used for non-null items.");
        }

        protected override bool DrawNewItemSelector(string label, ref object obj, RenderContext rc)
        {
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
