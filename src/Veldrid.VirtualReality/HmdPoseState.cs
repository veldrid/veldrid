using System.Numerics;

namespace Veldrid.VirtualReality
{
    public readonly struct HmdPoseState
    {
        public readonly Matrix4x4 LeftEyeProjection;
        public readonly Matrix4x4 RightEyeProjection;
        public readonly Vector3 LeftEyePosition;
        public readonly Vector3 RightEyePosition;
        public readonly Quaternion LeftEyeRotation;
        public readonly Quaternion RightEyeRotation;

        public HmdPoseState(
            Matrix4x4 leftEyeProjection,
            Matrix4x4 rightEyeProjection,
            Vector3 leftEyePosition,
            Vector3 rightEyePosition,
            Quaternion leftEyeRotation,
            Quaternion rightEyeRotation)
        {
            LeftEyeProjection = leftEyeProjection;
            RightEyeProjection = rightEyeProjection;
            LeftEyePosition = leftEyePosition;
            RightEyePosition = rightEyePosition;
            LeftEyeRotation = leftEyeRotation;
            RightEyeRotation = rightEyeRotation;
        }

        public Vector3 GetEyePosition(VREye eye)
        {
            switch (eye)
            {
                case VREye.Left: return LeftEyePosition;
                case VREye.Right: return RightEyePosition;
                default: throw new VeldridException($"Invalid {nameof(VREye)}: {eye}.");
            }
        }

        public Quaternion GetEyeRotation(VREye eye)
        {
            switch (eye)
            {
                case VREye.Left: return LeftEyeRotation;
                case VREye.Right: return RightEyeRotation;
                default: throw new VeldridException($"Invalid {nameof(VREye)}: {eye}.");
            }
        }

        public Matrix4x4 CreateView(VREye eye, Vector3 positionOffset, Vector3 forward, Vector3 up)
        {
            Vector3 eyePos = GetEyePosition(eye) + positionOffset;
            Quaternion eyeQuat = GetEyeRotation(eye);
            Vector3 forwardTransformed = Vector3.Transform(forward, eyeQuat);
            Vector3 upTransformed = Vector3.Transform(up, eyeQuat);
            return Matrix4x4.CreateLookAt(eyePos, eyePos + forwardTransformed, upTransformed);
        }
    }
}
