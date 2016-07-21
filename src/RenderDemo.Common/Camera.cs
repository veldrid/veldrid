using System.Numerics;
using Veldrid.Graphics;
using Veldrid.Platform;

namespace Veldrid.RenderDemo
{
    public class Camera
    {
        public DynamicDataProvider<Matrix4x4> ViewProvider { get; } = new DynamicDataProvider<Matrix4x4>();
        public DynamicDataProvider<Matrix4x4> ProjectionProvider { get; } = new DynamicDataProvider<Matrix4x4>();
        public DynamicDataProvider<Vector4> CameraInfoProvider { get; } = new DynamicDataProvider<Vector4>();
        private readonly Window _window;

        private Vector3 _position = Vector3.Zero;
        private Vector3 _lookDirection = -Vector3.UnitZ;
        private bool _ortho = false;
        private float _fov = 1.0f;
        private float _nearPlaneDistance = 1f;
        private float _farPlaneDistance = 250f;
        private float _orthographicWidth = 30f;

        public Vector3 Position { get { return _position; } set { _position = value; UpdateViewMatrix(); } }
        public Vector3 LookDirection { get { return _lookDirection; } set { _lookDirection = value; UpdateViewMatrix(); } }
        public bool UseOrthographicProjection { get { return _ortho; } set { _ortho = value; UpdateProjectionMatrix(); } }
        public float FieldOfViewRadians { get { return _fov; } set { _fov = value; UpdateProjectionMatrix(); } }
        public float NearPlaneDistance { get { return _nearPlaneDistance; } set { _nearPlaneDistance = value; UpdateProjectionMatrix(); } }
        public float FarPlaneDistance { get { return _farPlaneDistance; } set { _farPlaneDistance = value; UpdateProjectionMatrix(); } }
        public float OrthographicWidth { get { return _orthographicWidth; } set { _orthographicWidth = value; UpdateProjectionMatrix(); } }

        private float AspectRatio { get { return (float)_window.Width / _window.Height; } }

        public Camera(Window window)
        {
            _window = window;
            _window.Resized += () => UpdateProjectionMatrix();
            UpdateProjectionMatrix();
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            ViewProvider.Data = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY);
            CameraInfoProvider.Data = new Vector4(_position, 1f);
        }

        private void UpdateProjectionMatrix()
        {
            ProjectionProvider.Data = _ortho
                ? Matrix4x4.CreateOrthographic(_orthographicWidth, _orthographicWidth / AspectRatio, _nearPlaneDistance, _farPlaneDistance)
                : Matrix4x4.CreatePerspectiveFieldOfView(_fov, AspectRatio, _nearPlaneDistance, _farPlaneDistance);
        }

        public Ray GetRayFromScreenPoint(float screenX, float screenY)
        {
            // Normalized Device Coordinates
            float x = (2.0f * screenX) / _window.Width - 1.0f;
            float y = 1.0f - (2.0f * screenY) / _window.Height;
            float z = 1.0f;
            Vector3 deviceCoords = new Vector3(x, y, z);

            // Clip Coordinates
            Vector4 clipCoords = new Vector4(deviceCoords.X, deviceCoords.Y, -1.0f, 1.0f);

            // View Coordinates
            Matrix4x4 invProj;
            Matrix4x4.Invert(ProjectionProvider.Data, out invProj);
            Vector4 viewCoords = Vector4.Transform(clipCoords, invProj);
            viewCoords.Z = -1.0f;
            viewCoords.W = 0.0f;

            Matrix4x4 invView;
            Matrix4x4.Invert(ViewProvider.Data, out invView);
            Vector3 worldCoords = Vector4.Transform(viewCoords, invView).XYZ();
            worldCoords = Vector3.Normalize(worldCoords);

            return new Ray(_position, worldCoords);
        }
    }
}
