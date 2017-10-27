using System;
using System.Numerics;

namespace Veldrid.Utilities
{
    public static class FrustumHelpers
    {
        public static void ComputePerspectiveFrustumCorners(
            ref Vector3 viewPosition,
            ref Vector3 viewDirection,
            ref Vector3 globalUpDirection,
            float fov,
            float nearDistance,
            float farDistance,
            float aspectRatio,
            out FrustumCorners corners)
        {
            float nearHeight = (float)(2 * Math.Tan(fov / 2.0) * nearDistance);
            float nearWidth = nearHeight * aspectRatio;
            float farHeight = (float)(2 * Math.Tan(fov / 2.0) * farDistance);
            float farWidth = farHeight * aspectRatio;

            Vector3 right = Vector3.Normalize(Vector3.Cross(viewDirection, globalUpDirection));
            Vector3 up = Vector3.Normalize(Vector3.Cross(right, viewDirection));

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

        public static unsafe void ComputeOrthographicBoundsForPerpectiveFrustum(
            ref FrustumCorners corners,
            ref Vector3 lightDir,
            float cameraFarDistance,
            out Matrix4x4 lightView,
            out OrthographicBounds bounds)
        {
            float nearClipOffset = 40.0f;
            Vector3 centroid =
                (corners.NearTopLeft + corners.NearTopRight + corners.NearBottomLeft + corners.NearBottomRight
                + corners.FarTopLeft + corners.FarTopRight + corners.FarBottomLeft + corners.FarBottomRight)
                / 8f;
            Vector3 lightOrigin = centroid - (lightDir * (cameraFarDistance + nearClipOffset));
            lightView = Matrix4x4.CreateLookAt(lightOrigin, centroid, Vector3.UnitY);

            float* lightSpaceCornerFloats = stackalloc float[3 * 8];
            Vector3* lightSpaceCorners = (Vector3*)lightSpaceCornerFloats;

            // Light-view-space
            lightSpaceCorners[0] = Vector3.Transform(corners.NearTopLeft, lightView);
            lightSpaceCorners[1] = Vector3.Transform(corners.NearTopRight, lightView);
            lightSpaceCorners[2] = Vector3.Transform(corners.NearBottomLeft, lightView);
            lightSpaceCorners[3] = Vector3.Transform(corners.NearBottomRight, lightView);

            lightSpaceCorners[4] = Vector3.Transform(corners.FarTopLeft, lightView);
            lightSpaceCorners[5] = Vector3.Transform(corners.FarTopRight, lightView);
            lightSpaceCorners[6] = Vector3.Transform(corners.FarBottomLeft, lightView);
            lightSpaceCorners[7] = Vector3.Transform(corners.FarBottomRight, lightView);

            bounds.MinX = lightSpaceCorners[0].X;
            bounds.MaxX = lightSpaceCorners[0].X;
            bounds.MinY = lightSpaceCorners[0].Y;
            bounds.MaxY = lightSpaceCorners[0].Y;
            bounds.MinZ = lightSpaceCorners[0].Z;
            bounds.MaxZ = lightSpaceCorners[0].Z;

            for (int i = 1; i < 8; i++)
            {
                if (lightSpaceCorners[i].X < bounds.MinX) bounds.MinX = lightSpaceCorners[i].X;
                if (lightSpaceCorners[i].X > bounds.MaxX) bounds.MaxX = lightSpaceCorners[i].X;

                if (lightSpaceCorners[i].Y < bounds.MinY) bounds.MinY = lightSpaceCorners[i].Y;
                if (lightSpaceCorners[i].Y > bounds.MaxY) bounds.MaxY = lightSpaceCorners[i].Y;

                if (lightSpaceCorners[i].Z < bounds.MinZ) bounds.MinZ = lightSpaceCorners[i].Z;
                if (lightSpaceCorners[i].Z > bounds.MaxZ) bounds.MaxZ = lightSpaceCorners[i].Z;
            }
        }
    }
}
