using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AssetPrimitives;
using AssetProcessor;
using Veldrid.Utilities;

namespace Veldrid.SampleGallery
{
    public class SimpleMeshRender : Example
    {
        public DeviceBuffer VertexBuffer { get; private set; }
        public DeviceBuffer IndexBuffer { get; private set; }
        public uint IndexCount { get; private set; }
        public Texture CatTexture { get; private set; }

        public CommandList CL { get; private set; }

        public override async Task LoadResourcesAsync()
        {
            //List<Task> tasks = new List<Task>();

            //tasks.Add(Task.Run(() =>
            //{
            //    using (FileStream catFS = File.OpenRead(@"E:\Assets\cat\cat\cat.obj"))
            //    {
            //        ObjParser objParser = new ObjParser();
            //        ObjFile model = objParser.Parse(catFS);
            //        ConstructedMeshInfo firstMesh = model.GetFirstMesh();
            //        VertexBuffer = firstMesh.CreateVertexBuffer(Factory, Device);
            //        int indexCount;
            //        IndexBuffer = firstMesh.CreateIndexBuffer(Factory, Device, out indexCount);
            //        IndexCount = (uint)indexCount;
            //    }
            //}));

            //tasks.Add(Task.Run(async () =>
            //{
            //    ProcessedTexture tex;
            //    using (FileStream catDiffFS = File.OpenRead(@"E:\projects\veldrid\src\Veldrid.VirtualReality.Sample\cat\cat_diff.png"))
            //    {
            //        ImageSharpProcessor imageProcessor = new ImageSharpProcessor();
            //        tex = await imageProcessor.ProcessT(catDiffFS, "png");
            //    }

            //    CatTexture = tex.CreateDeviceTexture(Device, Factory, TextureUsage.Sampled);
            //}));

            //await Task.WhenAll(tasks);

            CL = Factory.CreateCommandList();
        }

        public override void Render(double deltaSeconds)
        {
            CL.Begin();
            CL.SetFramebuffer(Framebuffer);
            CL.ClearColorTarget(0, RgbaFloat.DarkRed);
            CL.End();
            Device.SubmitCommands(CL);
        }
    }
}
