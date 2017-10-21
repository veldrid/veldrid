using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Vd2.Utilities;

namespace Vd2.NeoDemo
{
    public class Scene
    {
        private readonly Octree<CullRenderable> _octree
            = new Octree<CullRenderable>(new BoundingBox(Vector3.One * -50, Vector3.One * 50), 2);

        private readonly List<Renderable> _freeRenderables = new List<Renderable>();
        private readonly List<IUpdateable> _updateables = new List<IUpdateable>();

        private readonly Dictionary<RenderPasses, Func<CullRenderable, bool>> _filters
            = new Dictionary<RenderPasses, Func<CullRenderable, bool>>(new RenderPassesComparer());

        private readonly Camera _camera;

        public Camera Camera => _camera;

        float _lScale = 1f;
        float _rScale = 1f;
        float _tScale = 1f;
        float _bScale = 1f;
        float _nScale = 4f;
        float _fScale = 4f;

        float _nearCascadeLimit = 100;
        float _midCascadeLimit = 300;
        float _farCascadeLimit;

        public Scene(int viewWidth, int viewHeight)
        {
            _camera = new Camera(viewWidth, viewHeight);
            _farCascadeLimit = _camera.FarDistance;
            _updateables.Add(_camera);
        }

        public void AddRenderable(Renderable r)
        {
            if (r is CullRenderable cr)
            {
                _octree.AddItem(cr.BoundingBox, cr);
            }
            else
            {
                _freeRenderables.Add(r);
            }
        }

        public void AddUpdateable(IUpdateable updateable)
        {
            Debug.Assert(updateable != null);
            _updateables.Add(updateable);
        }

        public void Update(float deltaSeconds)
        {
            foreach (IUpdateable updateable in _updateables)
            {
                updateable.Update(deltaSeconds);
            }
        }

        public void RenderAllStages(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            Matrix4x4 cameraProj = Camera.ProjectionMatrix;
            Vector4 nearLimitCS = Vector4.Transform(new Vector3(0, 0, -_nearCascadeLimit), cameraProj);
            Vector4 midLimitCS = Vector4.Transform(new Vector3(0, 0, -_midCascadeLimit), cameraProj);
            Vector4 farLimitCS = Vector4.Transform(new Vector3(0, 0, -_farCascadeLimit), cameraProj);

            cl.UpdateBuffer(sc.DepthLimitsBuffer, 0, new DepthCascadeLimits
            {
                NearLimit = nearLimitCS.Z,
                MidLimit = midLimitCS.Z,
                FarLimit = farLimitCS.Z
            });

            cl.UpdateBuffer(sc.LightInfoBuffer, 0, sc.DirectionalLight.GetInfo());

            // Near
            Matrix4x4 viewProj0 = UpdateDirectionalLightMatrices(
                sc,
                Camera.NearDistance,
                _nearCascadeLimit,
                sc.NearShadowMapTexture.Width,
                out BoundingFrustum lightFrustum);
            cl.UpdateBuffer(sc.LightViewProjectionBuffer0, 0, ref viewProj0);
            sc.CurrentLightViewProjectionBuffer = 0;
            cl.SetFramebuffer(sc.NearShadowMapFramebuffer);
            cl.SetViewport(0, new Viewport(0, 0, sc.NearShadowMapTexture.Width, sc.NearShadowMapTexture.Height, 0, 1));
            cl.SetScissorRect(0, 0, 0, sc.NearShadowMapTexture.Width, sc.NearShadowMapTexture.Height);
            cl.ClearDepthTarget(1f);
            Render(gd, cl, sc, RenderPasses.ShadowMap, lightFrustum, null);

            // Mid
            Matrix4x4 viewProj1 = UpdateDirectionalLightMatrices(
                sc,
                _nearCascadeLimit,
                _midCascadeLimit,
                sc.MidShadowMapTexture.Width,
                out lightFrustum);
            cl.UpdateBuffer(sc.LightViewProjectionBuffer1, 0, ref viewProj1);
            sc.CurrentLightViewProjectionBuffer = 1;
            cl.SetFramebuffer(sc.MidShadowMapFramebuffer);
            cl.SetViewport(0, new Viewport(0, 0, sc.MidShadowMapTexture.Width, sc.MidShadowMapTexture.Height, 0, 1));
            cl.SetScissorRect(0, 0, 0, sc.MidShadowMapTexture.Width, sc.MidShadowMapTexture.Height);
            cl.ClearDepthTarget(1f);
            Render(gd, cl, sc, RenderPasses.ShadowMap, lightFrustum, null);

            // Far
            Matrix4x4 viewProj2 = UpdateDirectionalLightMatrices(
                sc,
                _midCascadeLimit,
                _farCascadeLimit,
                sc.FarShadowMapTexture.Width,
                out lightFrustum);
            cl.UpdateBuffer(sc.LightViewProjectionBuffer2, 0, ref viewProj2);
            sc.CurrentLightViewProjectionBuffer = 2;
            cl.SetFramebuffer(sc.FarShadowMapFramebuffer);
            cl.SetViewport(0, new Viewport(0, 0, sc.FarShadowMapTexture.Width, sc.FarShadowMapTexture.Height, 0, 1));
            cl.SetScissorRect(0, 0, 0, sc.FarShadowMapTexture.Width, sc.FarShadowMapTexture.Height);
            cl.ClearDepthTarget(1f);
            Render(gd, cl, sc, RenderPasses.ShadowMap, lightFrustum, null);

            cl.SetFramebuffer(gd.SwapchainFramebuffer);
            Texture2D colorTex = (Texture2D)gd.SwapchainFramebuffer.ColorTextures[0];
            float scWidth = colorTex.Width;
            float scHeight = colorTex.Height;
            cl.SetViewport(0, new Viewport(0, 0, scWidth, scHeight, 0, 1));
            cl.SetScissorRect(0, 0, 0, (uint)scWidth, (uint)scHeight);
            cl.ClearColorTarget(0, RgbaFloat.Black);
            cl.ClearDepthTarget(1f);
            BoundingFrustum cameraFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
            Render(gd, cl, sc, RenderPasses.Standard, cameraFrustum, null);
            Render(gd, cl, sc, RenderPasses.AlphaBlend, cameraFrustum, null);
            Render(gd, cl, sc, RenderPasses.Overlay, cameraFrustum, null);
        }

        private Matrix4x4 UpdateDirectionalLightMatrices(
            SceneContext sc,
            float near,
            float far,
            uint shadowMapWidth,
            out BoundingFrustum lightFrustum)
        {
            Vector3 lightDir = sc.DirectionalLight.Direction;
            Vector3 viewDir = sc.Camera.LookDirection;
            Vector3 viewPos = sc.Camera.Position;
            Vector3 unitY = Vector3.UnitY;
            FrustumHelpers.ComputePerspectiveFrustumCorners(
                ref viewPos,
                ref viewDir,
                ref unitY,
                sc.Camera.FieldOfView,
                near,
                far,
                sc.Camera.AspectRatio,
                out FrustumCorners cameraCorners);

            // Approach used: http://alextardif.com/ShadowMapping.html

            Vector3 frustumCenter = Vector3.Zero;
            frustumCenter += cameraCorners.NearTopLeft;
            frustumCenter += cameraCorners.NearTopRight;
            frustumCenter += cameraCorners.NearBottomLeft;
            frustumCenter += cameraCorners.NearBottomRight;
            frustumCenter += cameraCorners.FarTopLeft;
            frustumCenter += cameraCorners.FarTopRight;
            frustumCenter += cameraCorners.FarBottomLeft;
            frustumCenter += cameraCorners.FarBottomRight;
            frustumCenter /= 8f;

            float radius = (cameraCorners.NearTopLeft - cameraCorners.FarBottomRight).Length() / 2.0f;
            float texelsPerUnit = shadowMapWidth / (radius * 2.0f);

            Matrix4x4 scalar = Matrix4x4.CreateScale(texelsPerUnit, texelsPerUnit, texelsPerUnit);

            Vector3 baseLookAt = -lightDir;

            Matrix4x4 lookat = Matrix4x4.CreateLookAt(Vector3.Zero, baseLookAt, Vector3.UnitY);
            lookat = scalar * lookat;
            Matrix4x4.Invert(lookat, out Matrix4x4 lookatInv);

            frustumCenter = Vector3.Transform(frustumCenter, lookat);
            frustumCenter.X = (int)frustumCenter.X;
            frustumCenter.Y = (int)frustumCenter.Y;
            frustumCenter = Vector3.Transform(frustumCenter, lookatInv);

            Vector3 lightPos = frustumCenter - (lightDir * radius * 2f);

            Matrix4x4 lightView = Matrix4x4.CreateLookAt(lightPos, frustumCenter, Vector3.UnitY);

            Matrix4x4 lightProjection = Matrix4x4.CreateOrthographicOffCenter(
                -radius * _lScale,
                radius * _rScale,
                -radius * _bScale,
                radius * _tScale,
                -radius * _nScale,
                radius * _fScale);
            Matrix4x4 viewProjectionMatrix = lightView * lightProjection;

            lightFrustum = new BoundingFrustum(lightProjection);
            return viewProjectionMatrix;
        }

        public void Render(
            GraphicsDevice gd,
            CommandList rc,
            SceneContext sc,
            RenderPasses pass,
            BoundingFrustum frustum,
            Comparer<RenderItemIndex> comparer = null)
        {
            _renderQueue.Clear();

            _cullableStage.Clear();
            CollectVisibleObjects(ref frustum, pass, _cullableStage);
            _renderQueue.AddRange(_cullableStage, _camera.Position);

            _renderableStage.Clear();
            CollectFreeObjects(pass, _renderableStage);
            _renderQueue.AddRange(_renderableStage, _camera.Position);

            if (comparer == null)
            {
                _renderQueue.Sort();
            }
            else
            {
                _renderQueue.Sort(comparer);
            }

            foreach (Renderable renderable in _renderQueue)
            {
                renderable.Render(gd, rc, sc, pass);
            }
        }

        private readonly RenderQueue _renderQueue = new RenderQueue();
        private readonly List<CullRenderable> _shadowmapStage = new List<CullRenderable>();
        private readonly List<CullRenderable> _cullableStage = new List<CullRenderable>();
        private readonly List<Renderable> _renderableStage = new List<Renderable>();

        private void CollectVisibleObjects(
            ref BoundingFrustum frustum,
            RenderPasses renderPass,
            List<CullRenderable> renderables)
        {
            _octree.GetContainedObjects(frustum, renderables, GetFilter(renderPass));
        }

        private void CollectFreeObjects(RenderPasses renderPass, List<Renderable> renderables)
        {
            foreach (Renderable r in _freeRenderables)
            {
                if ((r.RenderPasses & renderPass) != 0)
                {
                    renderables.Add(r);
                }
            }
        }

        private Func<CullRenderable, bool> GetFilter(RenderPasses passes)
        {
            if (!_filters.TryGetValue(passes, out Func<CullRenderable, bool> filter))
            {
                filter = CreateFilter(passes);
                _filters.Add(passes, filter);
            }

            return filter;
        }

        private static Func<CullRenderable, bool> CreateFilter(RenderPasses rp)
        {
            // This cannot be inlined into GetFilter -- a Roslyn bug causes copious allocations.
            // https://github.com/dotnet/roslyn/issues/22589
            return cr => (cr.RenderPasses & rp) == rp;
        }

        internal void DestroyAllDeviceObjects()
        {
            _cullableStage.Clear();
            _octree.GetAllContainedObjects(_cullableStage);
            foreach (CullRenderable cr in _cullableStage)
            {
                cr.DestroyDeviceObjects();
            }
            foreach (Renderable r in _freeRenderables)
            {
                r.DestroyDeviceObjects();
            }
        }

        internal void CreateAllDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            _cullableStage.Clear();
            _octree.GetAllContainedObjects(_cullableStage);
            foreach (CullRenderable cr in _cullableStage)
            {
                cr.CreateDeviceObjects(gd, cl, sc);
            }
            foreach (Renderable r in _freeRenderables)
            {
                r.CreateDeviceObjects(gd, cl, sc);
            }
        }

        private class RenderPassesComparer : IEqualityComparer<RenderPasses>
        {
            public bool Equals(RenderPasses x, RenderPasses y) => x == y;
            public int GetHashCode(RenderPasses obj) => ((byte)obj).GetHashCode();
        }
    }
}
