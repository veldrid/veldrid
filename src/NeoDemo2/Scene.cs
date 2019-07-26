using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid.NeoDemo.Objects;
using Veldrid.Sdl2;
using Veldrid.Utilities;

namespace Veldrid.NeoDemo
{
    public class Scene
    {
        private readonly Octree<CullRenderable> _octree
            = new Octree<CullRenderable>(new BoundingBox(Vector3.One * -50, Vector3.One * 50), 2);

        private readonly List<Renderable> _freeRenderables = new List<Renderable>();
        private readonly List<IUpdateable> _updateables = new List<IUpdateable>();

        private readonly ConcurrentDictionary<RenderPasses, Func<CullRenderable, bool>> _filters
            = new ConcurrentDictionary<RenderPasses, Func<CullRenderable, bool>>(new RenderPassesComparer());

        internal MirrorMesh MirrorMesh { get; set; } = new MirrorMesh();

        private readonly Camera _camera;

        public Camera Camera => _camera;

        public bool ThreadedRendering { get; set; } = false;

        float _lScale = 1f;
        float _rScale = 1f;
        float _tScale = 1f;
        float _bScale = 1f;
        float _nScale = 4f;
        float _fScale = 4f;

        float _nearCascadeLimit = 100;
        float _midCascadeLimit = 300;
        float _farCascadeLimit;

        public Scene(GraphicsDevice gd, Sdl2Window window, Sdl2ControllerTracker controller)
        {
            _camera = new Camera(gd, window, controller);
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

        private readonly Task[] _tasks = new Task[4];

        public void RenderAllStages(GraphicsDevice gd, CommandBuffer cb, SceneContext sc, Framebuffer fb)
        {
            if (ThreadedRendering)
            {
                // RenderAllMultiThreaded(gd, cl, sc);
            }
            else
            {
                RenderAllSingleThread(gd, cb, sc, fb);
            }
        }

        private void RenderAllSingleThread(GraphicsDevice gd, CommandBuffer cb, SceneContext sc, Framebuffer framebuffer)
        {
            float clearDepth = gd.IsDepthRangeZeroToOne ? 0f : 1f;
            Matrix4x4 cameraProj = Camera.ProjectionMatrix;
            Vector4 nearLimitCS = Vector4.Transform(new Vector3(0, 0, -_nearCascadeLimit), cameraProj);
            Vector4 midLimitCS = Vector4.Transform(new Vector3(0, 0, -_midCascadeLimit), cameraProj);
            Vector4 farLimitCS = Vector4.Transform(new Vector3(0, 0, -_farCascadeLimit), cameraProj);

            cb.UpdateBuffer(sc.DepthLimitsBuffer, 0, new DepthCascadeLimits
            {
                NearLimit = nearLimitCS.Z,
                MidLimit = midLimitCS.Z,
                FarLimit = farLimitCS.Z
            });

            cb.UpdateBuffer(sc.LightInfoBuffer, 0, sc.DirectionalLight.GetInfo());

            Vector3 lightPos = sc.DirectionalLight.Transform.Position - sc.DirectionalLight.Direction * 1000f;
            // Near
            cb.PushDebugGroup("Shadow Map - Near Cascade");
            Matrix4x4 viewProj0 = UpdateDirectionalLightMatrices(
                gd,
                sc,
                Camera.NearDistance,
                _nearCascadeLimit,
                sc.ShadowMapTexture.Width,
                out BoundingFrustum lightFrustum);
            cb.UpdateBuffer(sc.LightViewProjectionBuffer0, 0, ref viewProj0);
            cb.BeginRenderPass(sc.NearShadowMapFramebuffer, LoadAction.Clear, StoreAction.Store, RgbaFloat.Clear, clearDepth);
            Render(gd, cb, sc, RenderPasses.ShadowMapNear, lightFrustum, lightPos, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cb.EndRenderPass();
            cb.PopDebugGroup();

            cb.MemoryBarrier(sc.NearShadowMapView.Target, 0, 1, 0, 1, ShaderStages.Fragment, ShaderStages.Fragment);

            // Mid
            cb.PushDebugGroup("Shadow Map - Mid Cascade");
            Matrix4x4 viewProj1 = UpdateDirectionalLightMatrices(
                gd,
                sc,
                _nearCascadeLimit,
                _midCascadeLimit,
                sc.ShadowMapTexture.Width,
                out lightFrustum);
            cb.UpdateBuffer(sc.LightViewProjectionBuffer1, 0, ref viewProj1);
            cb.BeginRenderPass(sc.MidShadowMapFramebuffer, LoadAction.Clear, StoreAction.Store, RgbaFloat.Clear, clearDepth);
            Render(gd, cb, sc, RenderPasses.ShadowMapMid, lightFrustum, lightPos, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cb.EndRenderPass();
            cb.PopDebugGroup();

            cb.MemoryBarrier(sc.MidShadowMapView.Target, 0, 1, 0, 1, ShaderStages.Fragment, ShaderStages.Fragment);

            // Far
            cb.PushDebugGroup("Shadow Map - Far Cascade");
            Matrix4x4 viewProj2 = UpdateDirectionalLightMatrices(
                gd,
                sc,
                _midCascadeLimit,
                _farCascadeLimit,
                sc.ShadowMapTexture.Width,
                out lightFrustum);
            cb.UpdateBuffer(sc.LightViewProjectionBuffer2, 0, ref viewProj2);
            cb.BeginRenderPass(sc.FarShadowMapFramebuffer, LoadAction.Clear, StoreAction.Store, RgbaFloat.Clear, clearDepth);
            Render(gd, cb, sc, RenderPasses.ShadowMapFar, lightFrustum, lightPos, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cb.EndRenderPass();
            cb.PopDebugGroup();

            cb.MemoryBarrier(sc.FarShadowMapView.Target, 0, 1, 0, 1, ShaderStages.Fragment, ShaderStages.Fragment);

            // Reflections
            cb.PushDebugGroup("Planar Reflection Map");
            // Render reflected scene.
            Matrix4x4 planeReflectionMatrix = Matrix4x4.CreateReflection(MirrorMesh.Plane);
            CameraInfo camInfo = new CameraInfo();
            camInfo.CameraLookDirection = Vector3.Normalize(Vector3.Reflect(_camera.LookDirection, MirrorMesh.Plane.Normal));
            camInfo.CameraPosition_WorldSpace = Vector3.Transform(_camera.Position, planeReflectionMatrix);
            cb.UpdateBuffer(sc.CameraInfoBuffer, 0, ref camInfo);

            Matrix4x4 view = sc.Camera.ViewMatrix;
            view = planeReflectionMatrix * view;
            cb.UpdateBuffer(sc.ViewMatrixBuffer, 0, view);

            Matrix4x4 projection = _camera.ProjectionMatrix;
            cb.UpdateBuffer(sc.ReflectionViewProjBuffer, 0, view * projection);

            cb.BeginRenderPass(sc.ReflectionFramebuffer, LoadAction.Clear, StoreAction.Store, RgbaFloat.Black, clearDepth);
            BoundingFrustum cameraFrustum = new BoundingFrustum(view * projection);
            Render(gd, cb, sc, RenderPasses.ReflectionMap, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cb.EndRenderPass();

            cb.MemoryBarrier(sc.ReflectionColorTexture, 0, 1, 0, 1, ShaderStages.Fragment, ShaderStages.Fragment);

            cb.GenerateMipmaps(sc.ReflectionColorTexture);
            cb.PopDebugGroup();

            // Main scene
            sc.UpdateCameraBuffers(cb); // Re-set because reflection step changed it.
            cameraFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);

            RenderPassDescription rpd = RenderPassDescription.Create(sc.MainSceneFramebuffer);
            rpd.SetDepthAttachment(LoadAction.Clear, StoreAction.DontCare, clearDepth);
            rpd.SetColorAttachment(
                0,
                LoadAction.DontCare,
                sc.MainSceneSampleCount == TextureSampleCount.Count1 ? StoreAction.Store : StoreAction.DontCare,
                default);
            cb.BeginRenderPass(rpd);

            cb.PushDebugGroup("Main Scene Pass");
            Render(gd, cb, sc, RenderPasses.Standard, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cb.PopDebugGroup();

            cb.PushDebugGroup("Transparent Pass");
            Render(gd, cb, sc, RenderPasses.AlphaBlend, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cb.PopDebugGroup();

            cb.PushDebugGroup("Overlay");
            Render(gd, cb, sc, RenderPasses.Overlay, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cb.PopDebugGroup();

            cb.EndRenderPass();

            cb.MemoryBarrier(sc.MainSceneColorTexture, 0, 1, 0, 1, ShaderStages.Fragment, ShaderStages.Fragment);

            cb.PushDebugGroup("Duplicator");
            cb.BeginRenderPass(sc.DuplicatorFramebuffer, LoadAction.DontCare, StoreAction.Store, RgbaFloat.Clear, 0f);
            Render(gd, cb, sc, RenderPasses.Duplicator, new BoundingFrustum(), _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cb.PopDebugGroup();
            cb.EndRenderPass();

            cb.MemoryBarrier(sc.DuplicatorTarget0, 0, 1, 0, 1, ShaderStages.Fragment, ShaderStages.Fragment);
            cb.MemoryBarrier(sc.DuplicatorTarget1, 0, 1, 0, 1, ShaderStages.Fragment, ShaderStages.Fragment);

            cb.PushDebugGroup("Swapchain Pass");
            cb.BeginRenderPass(framebuffer, LoadAction.DontCare, StoreAction.Store, RgbaFloat.Clear, 0f);
            Render(gd, cb, sc, RenderPasses.SwapchainOutput, new BoundingFrustum(), _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cb.EndRenderPass();
            cb.PopDebugGroup();

            foreach (Renderable renderable in _allPerFrameRenderablesSet)
            {
                renderable.UpdatePerFrameResources(gd, _resourceUpdateCBs[NeoDemo.FrameIndex], sc);
            }

            gd.SubmitCommands(_resourceUpdateCBs[NeoDemo.FrameIndex], null, null, null);
            // gd.SubmitCommands(cb);
        }

        private Matrix4x4 UpdateDirectionalLightMatrices(
            GraphicsDevice gd,
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
            FrustumCorners cameraCorners;

            if (gd.IsDepthRangeZeroToOne)
            {
                FrustumHelpers.ComputePerspectiveFrustumCorners(
                    ref viewPos,
                    ref viewDir,
                    ref unitY,
                    sc.Camera.FieldOfView,
                    far,
                    near,
                    sc.Camera.AspectRatio,
                    out cameraCorners);
            }
            else
            {
                FrustumHelpers.ComputePerspectiveFrustumCorners(
                    ref viewPos,
                    ref viewDir,
                    ref unitY,
                    sc.Camera.FieldOfView,
                    near,
                    far,
                    sc.Camera.AspectRatio,
                    out cameraCorners);
            }

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

            Matrix4x4 lightProjection = Util.CreateOrtho(
                gd,
                gd.IsDepthRangeZeroToOne,
                -radius * _lScale,
                radius * _rScale,
                -radius * _bScale,
                radius * _tScale,
                -radius * _nScale,
                radius * _fScale);

            Matrix4x4 viewProjectionMatrix = lightView * lightProjection;

            lightFrustum = new BoundingFrustum(viewProjectionMatrix);
            return viewProjectionMatrix;
        }

        public void Render(
            GraphicsDevice gd,
            CommandBuffer cb,
            SceneContext sc,
            RenderPasses pass,
            BoundingFrustum frustum,
            Vector3 viewPosition,
            RenderQueue renderQueue,
            List<CullRenderable> cullRenderableList,
            List<Renderable> renderableList,
            Comparer<RenderItemIndex> comparer,
            bool threaded)
        {
            renderQueue.Clear();

            cullRenderableList.Clear();
            CollectVisibleObjects(ref frustum, pass, cullRenderableList);
            renderQueue.AddRange(cullRenderableList, viewPosition);

            renderableList.Clear();
            CollectFreeObjects(pass, renderableList);
            renderQueue.AddRange(renderableList, viewPosition);

            if (comparer == null)
            {
                renderQueue.Sort();
            }
            else
            {
                renderQueue.Sort(comparer);
            }

            foreach (Renderable renderable in renderQueue)
            {
                renderable.Render(gd, cb, sc, pass);
            }

            if (threaded)
            {
                lock (_allPerFrameRenderablesSet)
                {
                    foreach (CullRenderable thing in cullRenderableList) { _allPerFrameRenderablesSet.Add(thing); }
                    foreach (Renderable thing in renderableList) { _allPerFrameRenderablesSet.Add(thing); }
                }
            }
            else
            {
                foreach (CullRenderable thing in cullRenderableList) { _allPerFrameRenderablesSet.Add(thing); }
                foreach (Renderable thing in renderableList) { _allPerFrameRenderablesSet.Add(thing); }
            }
        }

        private readonly HashSet<Renderable> _allPerFrameRenderablesSet = new HashSet<Renderable>();
        private readonly RenderQueue[] _renderQueues = Enumerable.Range(0, 4).Select(i => new RenderQueue()).ToArray();
        private readonly List<CullRenderable>[] _cullableStage = Enumerable.Range(0, 4).Select(i => new List<CullRenderable>()).ToArray();
        private readonly List<Renderable>[] _renderableStage = Enumerable.Range(0, 4).Select(i => new List<Renderable>()).ToArray();

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

        private static Func<RenderPasses, Func<CullRenderable, bool>> s_createFilterFunc = rp => CreateFilter(rp);
        private CommandBuffer[] _resourceUpdateCBs;

        private Func<CullRenderable, bool> GetFilter(RenderPasses passes)
        {
            return _filters.GetOrAdd(passes, s_createFilterFunc);
        }

        private static Func<CullRenderable, bool> CreateFilter(RenderPasses rp)
        {
            // This cannot be inlined into GetFilter -- a Roslyn bug causes copious allocations.
            // https://github.com/dotnet/roslyn/issues/22589
            return cr => (cr.RenderPasses & rp) == rp;
        }

        internal void DestroyAllDeviceObjects()
        {
            _cullableStage[0].Clear();
            _octree.GetAllContainedObjects(_cullableStage[0]);
            foreach (CullRenderable cr in _cullableStage[0])
            {
                cr.DestroyDeviceObjects();
            }
            foreach (Renderable r in _freeRenderables)
            {
                r.DestroyDeviceObjects();
            }

            foreach (CommandBuffer cb in _resourceUpdateCBs)
            {
                cb.Dispose();
            }
        }

        internal void CreateAllDeviceObjects(GraphicsDevice gd, SceneContext sc)
        {
            _cullableStage[0].Clear();
            _octree.GetAllContainedObjects(_cullableStage[0]);
            foreach (CullRenderable cr in _cullableStage[0])
            {
                cr.CreateDeviceObjects(gd, sc);
            }
            foreach (Renderable r in _freeRenderables)
            {
                r.CreateDeviceObjects(gd, sc);
            }

            _resourceUpdateCBs = new CommandBuffer[NeoDemo.SwapchainBufferCount];
            for (uint i = 0; i < _resourceUpdateCBs.Length; i++)
            {
                _resourceUpdateCBs[i] = gd.ResourceFactory.CreateCommandBuffer();
                _resourceUpdateCBs[i].Name = "Scene Resource Update Command List";
            }
        }

        private class RenderPassesComparer : IEqualityComparer<RenderPasses>
        {
            public bool Equals(RenderPasses x, RenderPasses y) => x == y;
            public int GetHashCode(RenderPasses obj) => ((byte)obj).GetHashCode();
        }
    }
}
