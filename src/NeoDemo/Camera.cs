using ImGuiNET;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid.Sdl2;

namespace Veldrid.NeoDemo
{
    public class Camera : IUpdateable
    {
        private float _fov = 1f;
        private float _near = 1f;
        private float _far = 1000f;

        private Matrix4x4 _viewMatrix;
        private Matrix4x4 _projectionMatrix;

        private Vector3 _position = new Vector3(0, 3, 0);
        private Vector3 _lookDirection = new Vector3(0, -.3f, -1f);
        private float _moveSpeed = 10.0f;

        private float _yaw;
        private float _pitch;

        private Vector2 _previousMousePos;
        private float _windowWidth;
        private float _windowHeight;

        public event Action<Matrix4x4> ProjectionChanged;
        public event Action<Matrix4x4> ViewChanged;

        public Camera(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix(width, height);
            UpdateViewMatrix();
        }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        public Vector3 Position { get => _position; set { _position = value; UpdateViewMatrix(); } }
        public Vector3 LookDirection => _lookDirection;

        public float FarDistance => _far;

        public float FieldOfView => _fov;
        public float NearDistance => _near;

        public float AspectRatio => _windowWidth / _windowHeight;

        public float Yaw { get => _yaw; set { _yaw = value; UpdateViewMatrix(); } }
        public float Pitch { get => _pitch; set { _pitch = value; UpdateViewMatrix(); } }

        public void Update(float deltaSeconds)
        {
            float sprintFactor = InputTracker.GetKey(Key.ControlLeft)
                ? 0.1f
                : InputTracker.GetKey(Key.ShiftLeft)
                    ? 2.5f
                    : 1f;
            Vector3 motionDir = Vector3.Zero;
            if (InputTracker.GetKey(Key.A))
            {
                motionDir += -Vector3.UnitX;
            }
            if (InputTracker.GetKey(Key.D))
            {
                motionDir += Vector3.UnitX;
            }
            if (InputTracker.GetKey(Key.W))
            {
                motionDir += -Vector3.UnitZ;
            }
            if (InputTracker.GetKey(Key.S))
            {
                motionDir += Vector3.UnitZ;
            }
            if (InputTracker.GetKey(Key.Q))
            {
                motionDir += -Vector3.UnitY;
            }
            if (InputTracker.GetKey(Key.E))
            {
                motionDir += Vector3.UnitY;
            }

            if (motionDir != Vector3.Zero)
            {
                Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
                motionDir = Vector3.Transform(motionDir, lookRotation);
                _position += motionDir * _moveSpeed * sprintFactor * deltaSeconds;
                UpdateViewMatrix();
            }

            Vector2 mouseDelta = InputTracker.MousePosition - _previousMousePos;
            _previousMousePos = InputTracker.MousePosition;

            if (!ImGui.IsAnyWindowHovered() && (InputTracker.GetMouseButton(MouseButton.Left) || InputTracker.GetMouseButton(MouseButton.Right)))
            {
                Yaw += -mouseDelta.X * 0.01f;
                Pitch += -mouseDelta.Y * 0.01f;
                Pitch = Math.Clamp(Pitch, -1.55f, 1.55f);

                UpdateViewMatrix();
            }
        }

        public void WindowResized(float width, float height)
        {
            _windowWidth = width;
            _windowHeight = height;
            UpdatePerspectiveMatrix(width, height);
        }

        private void UpdatePerspectiveMatrix(float width, float height)
        {
            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(_fov, width / height, _near, _far);
            ProjectionChanged?.Invoke(_projectionMatrix);
        }

        private void UpdateViewMatrix()
        {
            Quaternion lookRotation = Quaternion.CreateFromYawPitchRoll(Yaw, Pitch, 0f);
            Vector3 lookDir = Vector3.Transform(-Vector3.UnitZ, lookRotation);
            _lookDirection = lookDir;
            _viewMatrix = Matrix4x4.CreateLookAt(_position, _position + _lookDirection, Vector3.UnitY);
            ViewChanged?.Invoke(_viewMatrix);
        }

        public CameraInfo GetCameraInfo() => new CameraInfo
        {
            CameraPosition_WorldSpace = _position,
            CameraLookDirection = _lookDirection
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraInfo
    {
        public Vector3 CameraPosition_WorldSpace;
        private float _padding1;
        public Vector3 CameraLookDirection;
        private float _padding2;
    }
}
