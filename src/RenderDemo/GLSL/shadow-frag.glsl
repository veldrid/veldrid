#version 140

uniform LightInfoBuffer
{
    vec3 lightDir;
    float __padding;
};

in vec3 out_position_worldSpace;
in vec4 out_lightPosition; //vertex with regard to light view
in vec3 out_normal;
in vec2 out_texCoord;

uniform sampler2D SurfaceTexture;
uniform sampler2D ShadowMap;

out vec4 outputColor;

void main()
{
    vec4 surfaceColor = texture2D(SurfaceTexture, out_texCoord);
    vec4 ambient = vec4(.4, .4, .4, 1);

    vec4 lightPosition_mod = out_lightPosition;

    //re-homogenize out_position after interpolation
    lightPosition_mod.xyz /= lightPosition_mod.w;

    // if out_position is not visible to the light - dont illuminate it
    // results in hard light frustum
    if (lightPosition_mod.x < -1.0f || lightPosition_mod.x > 1.0f ||
        lightPosition_mod.y < -1.0f || lightPosition_mod.y > 1.0f ||
        lightPosition_mod.z < 0.0f || lightPosition_mod.z > 1.0f)
    {
        outputColor = ambient * surfaceColor;
    }

    //transform clip space coords to texture space coords (-1:1 to 0:1)
    lightPosition_mod.x = lightPosition_mod.x / 2 + 0.5;
    lightPosition_mod.y = lightPosition_mod.y / -2 + 0.5;

    float ShadowMapBias = 0.005f;
    lightPosition_mod.z -= ShadowMapBias;

    //sample shadow map - point sampler
    float ShadowMapDepth = texture2D(ShadowMap, lightPosition_mod.xy).r;

    //if clip space z value greater than shadow map value then pixel is in shadow
    if (ShadowMapDepth < lightPosition_mod.z)
    {
        outputColor = ambient * surfaceColor;
    }

    //otherwise calculate ilumination at fragment
    vec3 L = -1 * lightDir;
    float ndotl = dot(normalize(out_normal), L);
    outputColor = ambient * surfaceColor + surfaceColor * ndotl;
}