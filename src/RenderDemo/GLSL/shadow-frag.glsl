#version 140

uniform LightInfoBuffer
{
    vec3 lightDir;
    float _padding;
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
    vec4 surfaceColor = texture(SurfaceTexture, out_texCoord);
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
		return;
    }

    //transform clip space coords to texture space coords (-1:1 to 0:1)
    lightPosition_mod.x = lightPosition_mod.x / 2 + 0.5;
    lightPosition_mod.y = lightPosition_mod.y / -2 + 0.5;

    vec3 L = -1 * normalize(lightDir);
    float ndotl = dot(normalize(out_normal), L);

    float cosTheta = clamp(ndotl, 0, 1);
    float bias = 0.0005 * tan(acos(cosTheta));
    bias = clamp(bias, 0, 0.01);

    lightPosition_mod.z -= bias;

    //sample shadow map - point sampler
    float shadowMapDepth = texture(ShadowMap, lightPosition_mod.xy).r;

    //if clip space z value greater than shadow map value then pixel is in shadow
    if (shadowMapDepth < lightPosition_mod.z)
    {
        outputColor = ambient * surfaceColor;
		return;
    }

    //otherwise calculate ilumination at fragment
    ndotl = clamp(ndotl, 0, 1);
    outputColor = ambient * surfaceColor + surfaceColor * ndotl;
	return;
}
