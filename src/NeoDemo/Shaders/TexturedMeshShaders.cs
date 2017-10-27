using ShaderGen;
using System.Numerics;
using Veldrid;
using Veldrid.NeoDemo;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet(
    "TexturedMesh",
    "Shaders.TexturedMesh.VS",
    "Shaders.TexturedMesh.FS")]

namespace Shaders
{
    public class TexturedMesh
    {
        public Matrix4x4 Projection;
        public Matrix4x4 View;
        public Matrix4x4 World;
        public Matrix4x4 InverseTransposeWorld;
        public DirectionalLightInfo LightInfo;
        public Texture2DResource SurfaceTexture;
        public SamplerResource SurfaceSampler;

        public struct PosNormTex
        {
            [PositionSemantic] public Vector3 Position;
            [NormalSemantic] public Vector3 Normal;
            [TextureCoordinateSemantic] public Vector2 TexCoord;
        }

        public struct FSInput
        {
            [PositionSemantic] public Vector4 Position;
            [NormalSemantic] public Vector3 Normal;
            [TextureCoordinateSemantic] public Vector2 TexCoords;
        }

        [VertexShader]
        public FSInput VS(PosNormTex input)
        {
            FSInput output;
            output.Position = Mul(Projection, Mul(View, Mul(World, new Vector4(input.Position, 1))));
            Vector4 normal = Mul(InverseTransposeWorld, new Vector4(input.Normal, 1));
            output.Normal = new Vector3(normal.X, normal.Y, normal.Z);
            output.TexCoords = input.TexCoord;
            return output;
        }

        [FragmentShader]
        public Vector4 FS(FSInput input)
        {
            float nDotL = Vector3.Dot(input.Normal, -LightInfo.Direction);
            nDotL = Clamp(nDotL, 0, 1);
            Vector4 lightContribution = nDotL * LightInfo.Color;
            Vector4 textureColor = Sample(SurfaceTexture, SurfaceSampler, input.TexCoords);
            Vector4 ambientLight = new Vector4(0.2f, 0.2f, 0.2f, 1f);
            Vector4 total = (lightContribution + ambientLight) * textureColor;
            return Saturate(total);
        }
    }
}
