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

	// perform perspective divide
    vec3 projCoords = out_lightPosition.xyz / out_lightPosition.w;

    // if out_position is not visible to the light - dont illuminate it
    // results in hard light frustum
    if (projCoords.x < -1.0f || projCoords.x > 1.0f ||
        projCoords.y < -1.0f || projCoords.y > 1.0f ||
        projCoords.z < 0.0f || projCoords.z > 1.0f)
    {
        outputColor = ambient * surfaceColor;
		return;
    }

	// Transform to [0,1] range
    projCoords = projCoords * 0.5 + 0.5;

    vec3 L = -1 * normalize(lightDir);
    float ndotl = dot(normalize(out_normal), L);

    float cosTheta = clamp(ndotl, 0, 1);
    float bias = 0.0015 * tan(acos(cosTheta));
    bias = clamp(bias, 0, 0.01);

    projCoords.z -= bias;

    //sample shadow map - point sampler
    float shadowMapDepth = texture(ShadowMap, projCoords.xy).r;

    //if clip space z value greater than shadow map value then pixel is in shadow
    if (shadowMapDepth < projCoords.z)
    {
        outputColor = ambient * surfaceColor;
		return;
    }

    //otherwise calculate ilumination at fragment
    ndotl = clamp(ndotl, 0, 1);
    outputColor = ambient * surfaceColor + surfaceColor * ndotl;
	return;
}
