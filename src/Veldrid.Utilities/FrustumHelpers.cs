using System;
using System.Numerics;

namespace Veldrid.Utilities
{
    public static class FrustumHelpers
    {
        public static void ComputePerspectiveFrustumCorners(
            Vector3 viewPosition,
            Vector3 viewDirection,
            Vector3 globalUpDirection,
            float fov,
            float nearDistance,
            float farDistance,
            float aspectRatio,
            out FrustumCorners corners)
        {
            float nearHeight = MathF.Tan(fov / 2f) * nearDistance;
            float nearWidth = nearHeight * aspectRatio;
            float farHeight = MathF.Tan(fov / 2f) * farDistance;
            float farWidth = farHeight * aspectRatio;

            Vector3 right = Vector3.Normalize(Vector3.Cross(viewDirection, globalUpDirection));
            Vector3 up = Vector3.Normalize(Vector3.Cross(right, viewDirection));

            Vector3 nearCenter = viewPosition + viewDirection * nearDistance;
            Vector3 farCenter = viewPosition + viewDirection * farDistance;

            Vector3 nearWidthRight = nearWidth * right;
            Vector3 nearHeightUp = nearHeight * up;

            Vector3 farWidthRight = farWidth * right;
            Vector3 farHeightUp = farHeight * up;

            corners.NearTopLeft = nearCenter - nearWidthRight + nearHeightUp;
            corners.NearTopRight = nearCenter + nearWidthRight + nearHeightUp;
            corners.NearBottomLeft = nearCenter - nearWidthRight - nearHeightUp;
            corners.NearBottomRight = nearCenter + nearWidthRight - nearHeightUp;

            corners.FarTopLeft = farCenter - farWidthRight + farHeightUp;
            corners.FarTopRight = farCenter + farWidthRight + farHeightUp;
            corners.FarBottomLeft = farCenter - farWidthRight - farHeightUp;
            corners.FarBottomRight = farCenter + farWidthRight - farHeightUp;
        }

        public static unsafe void ComputeOrthographicBoundsForPerpectiveFrustum(
            in FrustumCorners corners,
            Vector3 lightDir,
            float cameraFarDistance,
            out Matrix4x4 lightView,
            out BoundingBox bounds)
        {
            float nearClipOffset = 40.0f;
            Vector3 centroid =
                (corners.NearTopLeft + corners.NearTopRight + corners.NearBottomLeft + corners.NearBottomRight
                + corners.FarTopLeft + corners.FarTopRight + corners.FarBottomLeft + corners.FarBottomRight)
                / 8f;
            Vector3 lightOrigin = centroid - (lightDir * (cameraFarDistance + nearClipOffset));
            lightView = Matrix4x4.CreateLookAt(lightOrigin, centroid, Vector3.UnitY);

            Vector3* lightSpaceCorners = stackalloc Vector3[8];

            // Light-view-space
            lightSpaceCorners[0] = Vector3.Transform(corners.NearTopLeft, lightView);
            lightSpaceCorners[1] = Vector3.Transform(corners.NearTopRight, lightView);
            lightSpaceCorners[2] = Vector3.Transform(corners.NearBottomLeft, lightView);
            lightSpaceCorners[3] = Vector3.Transform(corners.NearBottomRight, lightView);

            lightSpaceCorners[4] = Vector3.Transform(corners.FarTopLeft, lightView);
            lightSpaceCorners[5] = Vector3.Transform(corners.FarTopRight, lightView);
            lightSpaceCorners[6] = Vector3.Transform(corners.FarBottomLeft, lightView);
            lightSpaceCorners[7] = Vector3.Transform(corners.FarBottomRight, lightView);

            bounds.Min = lightSpaceCorners[0];
            bounds.Max = lightSpaceCorners[0];

            for (int i = 1; i < 8; i++)
            {
                Vector3 corner = lightSpaceCorners[i];
                bounds.Min = Vector3.Min(bounds.Min, corner);
                bounds.Max = Vector3.Max(bounds.Max, corner);
            }
        }
    }
}
