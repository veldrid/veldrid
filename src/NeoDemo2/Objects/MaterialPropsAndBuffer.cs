using System.Runtime.CompilerServices;

namespace Veldrid.NeoDemo.Objects
{
    public class MaterialPropsAndBuffer
    {
        private MaterialProperties _properties;
        private bool _newProperties;

        public string Name { get; set; }
        public DeviceBuffer UniformBuffer { get; private set; }

        public MaterialProperties Properties
        {
            get => _properties;
            set { _properties = value; _newProperties = true; }
        }

        public MaterialPropsAndBuffer(MaterialProperties mp)
        {
            _properties = mp;
        }

        public void CreateDeviceObjects(GraphicsDevice gd)
        {
            UniformBuffer = gd.ResourceFactory.CreateBuffer(
                new BufferDescription((uint)Unsafe.SizeOf<MaterialProperties>(), BufferUsage.UniformBuffer));
            UniformBuffer.Update(0, ref _properties);
        }

        public void DestroyDeviceObjects()
        {
            UniformBuffer.Dispose();
        }

        public void FlushChanges(CommandBuffer cb)
        {
            if (_newProperties)
            {
                _newProperties = false;
                cb.UpdateBuffer(UniformBuffer, 0, ref _properties);
            }
        }
    }
}
