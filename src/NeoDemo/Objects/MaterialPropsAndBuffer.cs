using System.Runtime.CompilerServices;

namespace Veldrid.NeoDemo.Objects
{
    public class MaterialPropsAndBuffer
    {
        private MaterialProperties _properties;
        private bool _newProperties;

        public string Name { get; set; }
        public UniformBuffer UniformBuffer { get; private set; }

        public MaterialProperties Properties
        {
            get => _properties;
            set { _properties = value; _newProperties = true; }
        }

        public MaterialPropsAndBuffer(MaterialProperties mp)
        {
            _properties = mp;
        }

        public void CreateDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            UniformBuffer = gd.ResourceFactory.CreateUniformBuffer(new BufferDescription((ulong)Unsafe.SizeOf<MaterialProperties>()));
            cl.UpdateBuffer(UniformBuffer, 0, ref _properties);
        }

        public void DestroyDeviceObjects()
        {
            UniformBuffer.Dispose();
        }

        public void FlushChanges(CommandList cl)
        {
            if (_newProperties)
            {
                _newProperties = false;
                cl.UpdateBuffer(UniformBuffer, 0, ref _properties);
            }
        }
    }
}
