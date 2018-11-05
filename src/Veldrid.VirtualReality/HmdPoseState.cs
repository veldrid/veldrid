using System.Numerics;

namespace Veldrid.VirtualReality
{
    public readonly struct HmdPoseState
    {
        public readonly Matrix4x4 LeftEyeView;
        public readonly Matrix4x4 RightEyeView;
        public readonly Matrix4x4 LeftEyeProjection;
        public readonly Matrix4x4 RightEyeProjection;

        public HmdPoseState(
            Matrix4x4 leftEyeView, 
            Matrix4x4 rightEyeView, 
            Matrix4x4 leftEyeProjection, 
            Matrix4x4 rightEyeProjection)
        {
            LeftEyeView = leftEyeView;
            RightEyeView = rightEyeView;
            LeftEyeProjection = leftEyeProjection;
            RightEyeProjection = rightEyeProjection;
        }
    }
}
