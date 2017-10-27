using System.Numerics;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;
using Veldrid.NeoDemo;
using System;

[assembly: ShaderSet("ShadowMain", "Shaders.ShadowMain.VS", "Shaders.ShadowMain.FS")]

namespace Shaders
{
    public partial class ShadowMain
    {
        public Matrix4x4 Projection;
        public Matrix4x4 View;
        public Matrix4x4 World;
        public Matrix4x4 InverseTransposeWorld;
        public Matrix4x4 LightViewProjection1;
        public Matrix4x4 LightViewProjection2;
        public Matrix4x4 LightViewProjection3;
        public DepthCascadeLimits DepthLimits;
        public DirectionalLightInfo LightInfo;
        public CameraInfo CameraInfo;
        public PointLightsInfo PointLights;
        public MaterialProperties MaterialProperties;

        public Texture2DResource SurfaceTexture;
        public SamplerResource RegularSampler;
        public Texture2DResource AlphaMap;
        public SamplerResource AlphaMapSampler;
        public Texture2DResource ShadowMapNear;
        public Texture2DResource ShadowMapMid;
        public Texture2DResource ShadowMapFar;
        public SamplerResource ShadowMapSampler;

        public struct VertexInput
        {
            [PositionSemantic] public Vector3 Position;
            [NormalSemantic] public Vector3 Normal;
            [TextureCoordinateSemantic] public Vector2 TexCoord;
        }

        public struct PixelInput
        {
            [PositionSemantic] public Vector4 Position;
            [PositionSemantic] public Vector3 Position_WorldSpace;
            [TextureCoordinateSemantic] public Vector4 LightPosition1;
            [TextureCoordinateSemantic] public Vector4 LightPosition2;
            [TextureCoordinateSemantic] public Vector4 LightPosition3;
            [NormalSemantic] public Vector3 Normal;
            [TextureCoordinateSemantic] public Vector2 TexCoord;
            [PositionSemantic] public float FragDepth;
        }

        [VertexShader]
        public PixelInput VS(VertexInput input)
        {
            PixelInput output;
            Vector4 worldPosition = Mul(World, new Vector4(input.Position, 1));
            Vector4 viewPosition = Mul(View, worldPosition);
            output.Position = Mul(Projection, viewPosition);

            output.Position_WorldSpace = worldPosition.XYZ();

            Vector4 outNormal = Mul(InverseTransposeWorld, new Vector4(input.Normal, 1));
            output.Normal = Vector3.Normalize(outNormal.XYZ());

            output.TexCoord = input.TexCoord;

            output.LightPosition1 = Mul(World, new Vector4(input.Position, 1));
            output.LightPosition1 = Mul(LightViewProjection1, output.LightPosition1);

            output.LightPosition2 = Mul(World, new Vector4(input.Position, 1));
            output.LightPosition2 = Mul(LightViewProjection2, output.LightPosition2);

            output.LightPosition3 = Mul(World, new Vector4(input.Position, 1));
            output.LightPosition3 = Mul(LightViewProjection3, output.LightPosition3);

            output.FragDepth = output.Position.Z;
            return output;
        }

        [FragmentShader]
        public Vector4 FS(PixelInput input)
        {
            float alphaMapSample = Sample(AlphaMap, AlphaMapSampler, input.TexCoord).X;
            if (alphaMapSample == 0)
            {
                Discard();
            }

            Vector4 surfaceColor = Sample(SurfaceTexture, RegularSampler, input.TexCoord);
            Vector4 ambientLight = new Vector4(0.3f, 0.3f, 0.3f, 1f);
            Vector3 lightDir = -LightInfo.Direction;
            Vector4 directionalColor = ambientLight * surfaceColor;
            float shadowBias = 0.0005f;
            float lightIntensity = 0f;

            // Specular color
            Vector4 directionalSpecColor = new Vector4();

            Vector3 vertexToEye = Vector3.Normalize(input.Position_WorldSpace - CameraInfo.CameraPosition_WorldSpace);
            Vector3 lightReflect = Vector3.Normalize(Vector3.Reflect(LightInfo.Direction, input.Normal));

            float specularFactor = Vector3.Dot(vertexToEye, lightReflect);
            if (specularFactor > 0)
            {
                specularFactor = Pow(Abs(specularFactor), MaterialProperties.SpecularPower);
                directionalSpecColor = new Vector4(LightInfo.Color.XYZ() * MaterialProperties.SpecularIntensity * specularFactor, 1.0f);
            }

            // Directional light influence

            float depthTest = input.FragDepth;

            Vector2 shadowCoords_0 = ClipToTextureCoordinates(input.LightPosition1);
            Vector2 shadowCoords_1 = ClipToTextureCoordinates(input.LightPosition2);
            Vector2 shadowCoords_2 = ClipToTextureCoordinates(input.LightPosition3);

            float lightDepthValues_0 = input.LightPosition1.Z / input.LightPosition1.W;
            float lightDepthValues_1 = input.LightPosition2.Z / input.LightPosition2.W;
            float lightDepthValues_2 = input.LightPosition3.Z / input.LightPosition3.W;

            int shadowIndex = 3;

            Vector2 shadowCoords = Vector2.Zero;
            float lightDepthValue = 0;

            if ((depthTest < DepthLimits.NearLimit) && InRange(shadowCoords_0.X, 0, 1) && InRange(shadowCoords_0.Y, 0, 1))
            {
                shadowIndex = 0;
                shadowCoords = shadowCoords_0;
                lightDepthValue = lightDepthValues_0;
            }
            else if ((depthTest < DepthLimits.MidLimit) && InRange(shadowCoords_1.X, 0, 1) && InRange(shadowCoords_1.Y, 0, 1))
            {
                shadowIndex = 1;
                shadowCoords = shadowCoords_1;
                lightDepthValue = lightDepthValues_1;
            }
            else if (depthTest < DepthLimits.FarLimit && InRange(shadowCoords_2.X, 0, 1) && InRange(shadowCoords_2.Y, 0, 1))
            {
                shadowIndex = 2;
                shadowCoords = shadowCoords_2;
                lightDepthValue = lightDepthValues_2;
            }

            if (shadowIndex != 3)
            {
                // We are within one of the shadow maps.
                float shadowMapDepth = SampleDepthMap(shadowIndex, shadowCoords);

                float biasedDistToLight = (lightDepthValue - shadowBias);

                if (biasedDistToLight < shadowMapDepth)
                {
                    // In light (no occluders between light and fragment).
                    lightIntensity = Saturate(Vector3.Dot(input.Normal, lightDir));
                    if (lightIntensity > 0.0f)
                    {
                        directionalColor = surfaceColor * (lightIntensity * LightInfo.Color);
                    }
                }
                else
                {
                    // In shadow.
                    directionalColor = ambientLight * surfaceColor;
                    directionalSpecColor = Vector4.Zero;
                }
            }
            else
            {
                // We are outside of all shadow maps. Pretend like the object is not shadowed.
                lightIntensity = Saturate(Vector3.Dot(input.Normal, lightDir));
                if (lightIntensity > 0.0f)
                {
                    directionalColor = surfaceColor * lightIntensity * LightInfo.Color;
                }
            }

            // Point lights

            Vector4 pointDiffuse = new Vector4(0, 0, 0, 1);
            Vector4 pointSpec = new Vector4(0, 0, 0, 1);
            for (int i = 0; i < PointLights.NumActiveLights; i++)
            {
                PointLightInfo pli = PointLights.PointLights[i];
                Vector3 ptLightDir = Vector3.Normalize(pli.Position - input.Position_WorldSpace);
                float intensity = Saturate(Vector3.Dot(input.Normal, ptLightDir));
                float lightDistance = Vector3.Distance(pli.Position, input.Position_WorldSpace);
                intensity = Saturate(intensity * (1 - (lightDistance / pli.Range)));

                pointDiffuse += intensity * new Vector4(pli.Color, 1) * surfaceColor;

                // Specular
                Vector3 lightReflect0 = Vector3.Normalize(Vector3.Reflect(ptLightDir, input.Normal));

                float specularFactor0 = Vector3.Dot(vertexToEye, lightReflect0);
                if (specularFactor0 > 0 && pli.Range > lightDistance)
                {
                    specularFactor0 = Pow(Abs(specularFactor0), MaterialProperties.SpecularPower);
                    pointSpec += (1 - (lightDistance / pli.Range)) * (new Vector4(pli.Color * MaterialProperties.SpecularIntensity * specularFactor0, 1.0f));
                }
            }

            return WithAlpha(Saturate(directionalSpecColor + directionalColor + pointSpec + pointDiffuse), alphaMapSample);
        }

        private bool InRange(float val, float min, float max)
        {
            return val >= min && val <= max;
        }

        float SampleDepthMap(int index, Vector2 coord)
        {
            if (index == 0)
            {
                return Sample(ShadowMapNear, ShadowMapSampler, coord).X;
            }
            else if (index == 1)
            {
                return Sample(ShadowMapMid, ShadowMapSampler, coord).X;
            }
            else
            {
                return Sample(ShadowMapFar, ShadowMapSampler, coord).X;
            }
        }

        Vector4 WithAlpha(Vector4 baseColor, float alpha)
        {
            return new Vector4(baseColor.XYZ(), alpha);
        }
    }
}
