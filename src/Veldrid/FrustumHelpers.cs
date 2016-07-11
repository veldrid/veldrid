using System;
using System.Numerics;

namespace Veldrid
{
    public static class FrustumHelpers
    {
        public static void ComputePerspectiveFrustumCorners(
            ref Vector3 viewPosition,
            ref Vector3 viewDirection,
            ref Vector3 upDirection,
            float fov,
            float nearDistance,
            float farDistance,
            float aspectRatio,
            out FrustumCorners corners)
        {
            float nearHeight = 2 * (float)Math.Tan(fov / 2f) * nearDistance;
            float nearWidth = nearHeight * aspectRatio;
            float farHeight = 2 * (float)Math.Tan(fov / 2f) * farDistance;
            float farWidth = farHeight * aspectRatio;

            Vector3 right = Vector3.Cross(viewDirection, upDirection);
            Vector3 up = Vector3.Cross(right, viewDirection);

            Vector3 nearCenter = viewPosition + viewDirection * nearDistance;
            Vector3 farCenter = viewPosition + viewDirection * farDistance;

            corners.NearTopLeft = nearCenter - ((nearWidth / 2f) * right) + ((nearHeight / 2) * up);
            corners.NearTopRight = nearCenter + ((nearWidth / 2f) * right) + ((nearHeight / 2) * up);
            corners.NearBottomLeft = nearCenter - ((nearWidth / 2f) * right) - ((nearHeight / 2) * up);
            corners.NearBottomRight = nearCenter + ((nearWidth / 2f) * right) - ((nearHeight / 2) * up);

            corners.FarTopLeft = farCenter - ((farWidth / 2f) * right) + ((farHeight / 2) * up);
            corners.FarTopRight = farCenter + ((farWidth / 2f) * right) + ((farHeight / 2) * up);
            corners.FarBottomLeft = farCenter - ((farWidth / 2f) * right) - ((farHeight / 2) * up);
            corners.FarBottomRight = farCenter + ((farWidth / 2f) * right) - ((farHeight / 2) * up);
        }
    }

    public struct FrustumCorners
    {
        public Vector3 NearTopLeft;
        public Vector3 NearTopRight;
        public Vector3 NearBottomLeft;
        public Vector3 NearBottomRight;
        public Vector3 FarTopLeft;
        public Vector3 FarTopRight;
        public Vector3 FarBottomLeft;
        public Vector3 FarBottomRight;
    }
}
