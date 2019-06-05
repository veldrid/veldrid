using System;
using System.IO;
using System.Text;
using Veldrid.SPIRV;
using Veldrid.StbImage;

namespace Veldrid.SampleGallery
{
    public class SkyboxRenderer : DisposeCollectorBase
    {
        private Pipeline _pipeline;
        private readonly ResourceSet _set;

        public SkyboxRenderer(GraphicsDevice gd, Stream[] imageStreams)
            : base(gd)
        {
            ShaderSetDescription shaderSet = new ShaderSetDescription(
                Array.Empty<VertexLayoutDescription>(),
                Factory.CreateFromSpirv(
                    new ShaderDescription(ShaderStages.Vertex, Encoding.UTF8.GetBytes(VertexCode), "main"),
                    new ShaderDescription(ShaderStages.Fragment, Encoding.UTF8.GetBytes(FragmentCode), "main")));

            ResourceLayout layout = Factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("SkyTex", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("SkySamp", ResourceKind.Sampler, ShaderStages.Fragment)));

            GraphicsPipelineDescription gpd = new GraphicsPipelineDescription(
                BlendStateDescription.SingleOverrideBlend,
                gd.IsDepthRangeZeroToOne
                    ? DepthStencilStateDescription.DepthOnlyGreaterEqual
                    : DepthStencilStateDescription.DepthOnlyLessEqual,
                RasterizerStateDescription.Default,
                PrimitiveTopology.TriangleStrip,
                shaderSet,
                new[] { GalleryConfig.Global.CameraInfoLayout, GalleryConfig.Global.MainFBInfoLayout, layout },
                GalleryConfig.Global.MainFB.OutputDescription);

            _pipeline = Factory.CreateGraphicsPipeline(gpd);

            Texture cubeTex = StbTextureLoader.LoadCube(gd, Factory, imageStreams, false, true);
            _set = Factory.CreateResourceSet(new ResourceSetDescription(layout, cubeTex, Device.Aniso4xSampler));
        }

        public void Render(CommandList cl)
        {
            float depth = Device.IsDepthRangeZeroToOne ? 0 : 1;
            cl.SetViewport(0, new Viewport(0, 0, GalleryConfig.Global.MainFB.Width, GalleryConfig.Global.MainFB.Height, depth, depth));

            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, GalleryConfig.Global.CameraInfoSet);
            cl.SetGraphicsResourceSet(1, GalleryConfig.Global.MainFBInfoSet);
            cl.SetGraphicsResourceSet(2, _set);
            cl.Draw(4);
        }

        private static string VertexCode =
$@"
#version 450

layout (location = 0) out vec2 fsin_UV;

const vec4 QuadInfos[4] = 
{{
    vec4(-1, 1, 0, 0),
    vec4(1, 1, 1, 0),
    vec4(-1, -1, 0, 1),
    vec4(1, -1, 1, 1),
}};

void main()
{{
    gl_Position = vec4(QuadInfos[gl_VertexIndex].xy, 0, 1);
    fsin_UV = QuadInfos[gl_VertexIndex].zw;
}}
";
        private static string FragmentCode =
$@"
#version 450

{SharedShaders.CameraInfoSet(0)}
{SharedShaders.FBInfoSet(1)}

layout (set = 2, binding = 0) uniform textureCube SkyTex;
layout (set = 2, binding = 1) uniform sampler SkySamp;

layout (location = 0) in vec2 fsin_UV;
layout (location = 0) out vec4 fsout_color;

void main()
{{
    float x = fsin_UV.x * 2.0f - 1.0f;
    float y = 1.0f - fsin_UV.y * 2.0f;
    float z = 1.0f;
    vec3 ray_nds = vec3(x, y, z);
    vec4 ray_clip = vec4(ray_nds.xy, -1.0, 1.0);
    vec4 ray_eye = InvProjection * ray_clip;
    ray_eye = vec4(ray_eye.xy, -1.0, 0.0);
    vec3 ray_wor = (InvView * ray_eye).xyz;
    // don't forget to normalise the vector at some point
    ray_wor = normalize(ray_wor);

    fsout_color = texture(samplerCube(SkyTex, SkySamp), ray_wor);
}}
";
    }
}
