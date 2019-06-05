namespace Veldrid.SampleGallery
{
    internal static class SharedShaders
    {
        public static string CameraInfoSet(uint set) =>
$@"
layout (set = {set}, binding = 0) uniform CameraInfo
{{
    mat4 View;
    mat4 InvView;
    mat4 Projection;
    mat4 InvProjection;
    vec3 CameraPosition_WorldSpace;
    float _padding1;
    vec3 CameraLookDirection;
    float _padding2;
}};
";

        public static string FBInfoSet(uint set) =>
$@"
layout (set = {set}, binding = 0) uniform FBInfo
{{
    uint FB_Width;
    uint FB_Height;
    uint _FBInfo_padding0;
    uint _FBInfo_padding1;
}};
";
    }
}
