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

        public void RenderAllStages(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            if (ThreadedRendering)
            {
                RenderAllMultiThreaded(gd, cl, sc);
            }
            else
            {
                RenderAllSingleThread(gd, cl, sc);
            }
        }

        private void RenderAllSingleThread(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            float depthClear = gd.IsDepthRangeZeroToOne ? 0f : 1f;
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

            Vector3 lightPos = sc.DirectionalLight.Transform.Position - sc.DirectionalLight.Direction * 1000f;
            // Near
            cl.PushDebugGroup("Shadow Map - Near Cascade");
            Matrix4x4 viewProj0 = UpdateDirectionalLightMatrices(
                gd,
                sc,
                Camera.NearDistance,
                _nearCascadeLimit,
                sc.ShadowMapTexture.Width,
                out BoundingFrustum lightFrustum);
            cl.UpdateBuffer(sc.LightViewProjectionBuffer0, 0, ref viewProj0);
            cl.SetFramebuffer(sc.NearShadowMapFramebuffer);
            cl.SetFullViewports();
            cl.ClearDepthStencil(depthClear);
            Render(gd, cl, sc, RenderPasses.ShadowMapNear, lightFrustum, lightPos, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cl.PopDebugGroup();

            // Mid
            cl.PushDebugGroup("Shadow Map - Mid Cascade");
            Matrix4x4 viewProj1 = UpdateDirectionalLightMatrices(
                gd,
                sc,
                _nearCascadeLimit,
                _midCascadeLimit,
                sc.ShadowMapTexture.Width,
                out lightFrustum);
            cl.UpdateBuffer(sc.LightViewProjectionBuffer1, 0, ref viewProj1);
            cl.SetFramebuffer(sc.MidShadowMapFramebuffer);
            cl.SetFullViewports();
            cl.ClearDepthStencil(depthClear);
            Render(gd, cl, sc, RenderPasses.ShadowMapMid, lightFrustum, lightPos, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cl.PopDebugGroup();

            // Far
            cl.PushDebugGroup("Shadow Map - Far Cascade");
            Matrix4x4 viewProj2 = UpdateDirectionalLightMatrices(
                gd,
                sc,
                _midCascadeLimit,
                _farCascadeLimit,
                sc.ShadowMapTexture.Width,
                out lightFrustum);
            cl.UpdateBuffer(sc.LightViewProjectionBuffer2, 0, ref viewProj2);
            cl.SetFramebuffer(sc.FarShadowMapFramebuffer);
            cl.SetFullViewports();
            cl.ClearDepthStencil(depthClear);
            Render(gd, cl, sc, RenderPasses.ShadowMapFar, lightFrustum, lightPos, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cl.PopDebugGroup();

            // Reflections
            cl.PushDebugGroup("Planar Reflection Map");
            cl.SetFramebuffer(sc.ReflectionFramebuffer);
            float fbWidth = sc.ReflectionFramebuffer.Width;
            float fbHeight = sc.ReflectionFramebuffer.Height;
            cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearColorTarget(0, RgbaFloat.Black);
            cl.ClearDepthStencil(depthClear);

            // Render reflected scene.
            Matrix4x4 planeReflectionMatrix = Matrix4x4.CreateReflection(MirrorMesh.Plane);
            CameraInfo camInfo = new CameraInfo();
            camInfo.CameraLookDirection = Vector3.Normalize(Vector3.Reflect(_camera.LookDirection, MirrorMesh.Plane.Normal));
            camInfo.CameraPosition_WorldSpace = Vector3.Transform(_camera.Position, planeReflectionMatrix);
            cl.UpdateBuffer(sc.CameraInfoBuffer, 0, ref camInfo);

            Matrix4x4 view = sc.Camera.ViewMatrix;
            view = planeReflectionMatrix * view;
            cl.UpdateBuffer(sc.ViewMatrixBuffer, 0, view);

            Matrix4x4 projection = _camera.ProjectionMatrix;
            cl.UpdateBuffer(sc.ReflectionViewProjBuffer, 0, view * projection);

            BoundingFrustum cameraFrustum = new BoundingFrustum(view * projection);
            Render(gd, cl, sc, RenderPasses.ReflectionMap, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            cl.GenerateMipmaps(sc.ReflectionColorTexture);
            cl.PopDebugGroup();

            // Main scene
            cl.PushDebugGroup("Main Scene Pass");
            cl.SetFramebuffer(sc.MainSceneFramebuffer);
            fbWidth = sc.MainSceneFramebuffer.Width;
            fbHeight = sc.MainSceneFramebuffer.Height;
            cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearDepthStencil(depthClear);
            sc.UpdateCameraBuffers(cl); // Re-set because reflection step changed it.
            cameraFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
            Render(gd, cl, sc, RenderPasses.Standard, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cl.PopDebugGroup();
            cl.PushDebugGroup("Transparent Pass");
            Render(gd, cl, sc, RenderPasses.AlphaBlend, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cl.PopDebugGroup();
            cl.PushDebugGroup("Overlay");
            Render(gd, cl, sc, RenderPasses.Overlay, cameraFrustum, _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cl.PopDebugGroup();

            if (sc.MainSceneColorTexture.SampleCount != TextureSampleCount.Count1)
            {
                cl.ResolveTexture(sc.MainSceneColorTexture, sc.MainSceneResolvedColorTexture);
            }

            cl.PushDebugGroup("Duplicator");
            cl.SetFramebuffer(sc.DuplicatorFramebuffer);
            fbWidth = sc.DuplicatorFramebuffer.Width;
            fbHeight = sc.DuplicatorFramebuffer.Height;
            cl.SetFullViewports();
            Render(gd, cl, sc, RenderPasses.Duplicator, new BoundingFrustum(), _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cl.PopDebugGroup();

            cl.PushDebugGroup("Swapchain Pass");
            cl.SetFramebuffer(gd.SwapchainFramebuffer);
            fbWidth = gd.SwapchainFramebuffer.Width;
            fbHeight = gd.SwapchainFramebuffer.Height;
            cl.SetFullViewports();
            Render(gd, cl, sc, RenderPasses.SwapchainOutput, new BoundingFrustum(), _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cl.PopDebugGroup();

            cl.End();

            _resourceUpdateCL.Begin();
            foreach (Renderable renderable in _allPerFrameRenderablesSet)
            {
                renderable.UpdatePerFrameResources(gd, _resourceUpdateCL, sc);
            }
            _resourceUpdateCL.End();

            gd.SubmitCommands(_resourceUpdateCL);
            gd.SubmitCommands(cl);
        }

        private void RenderAllMultiThreaded(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            float depthClear = gd.IsDepthRangeZeroToOne ? 0f : 1f;
            Matrix4x4 cameraProj = Camera.ProjectionMatrix;
            Vector4 nearLimitCS = Vector4.Transform(new Vector3(0, 0, -_nearCascadeLimit), cameraProj);
            Vector4 midLimitCS = Vector4.Transform(new Vector3(0, 0, -_midCascadeLimit), cameraProj);
            Vector4 farLimitCS = Vector4.Transform(new Vector3(0, 0, -_farCascadeLimit), cameraProj);
            Vector3 lightPos = sc.DirectionalLight.Transform.Position - sc.DirectionalLight.Direction * 1000f;

            _resourceUpdateCL.Begin();
            CommandList[] cls = new CommandList[5];
            for (int i = 0; i < cls.Length; i++) { cls[i] = gd.ResourceFactory.CreateCommandList(); cls[i].Begin(); }

            _resourceUpdateCL.UpdateBuffer(sc.DepthLimitsBuffer, 0, new DepthCascadeLimits
            {
                NearLimit = nearLimitCS.Z,
                MidLimit = midLimitCS.Z,
                FarLimit = farLimitCS.Z
            });

            _resourceUpdateCL.UpdateBuffer(sc.LightInfoBuffer, 0, sc.DirectionalLight.GetInfo());

            _allPerFrameRenderablesSet.Clear();
            _tasks[0] = Task.Run(() =>
            {
                // Near
                Matrix4x4 viewProj0 = UpdateDirectionalLightMatrices(
                    gd,
                    sc,
                    Camera.NearDistance,
                    _nearCascadeLimit,
                    sc.ShadowMapTexture.Width,
                    out BoundingFrustum lightFrustum0);
                cls[1].UpdateBuffer(sc.LightViewProjectionBuffer0, 0, ref viewProj0);

                cls[1].SetFramebuffer(sc.NearShadowMapFramebuffer);
                cls[1].SetViewport(0, new Viewport(0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height, 0, 1));
                cls[1].SetScissorRect(0, 0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height);
                cls[1].ClearDepthStencil(depthClear);
                Render(gd, cls[1], sc, RenderPasses.ShadowMapNear, lightFrustum0, lightPos, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, true);
            });

            _tasks[1] = Task.Run(() =>
            {
                // Mid
                Matrix4x4 viewProj1 = UpdateDirectionalLightMatrices(
                    gd,
                    sc,
                    _nearCascadeLimit,
                    _midCascadeLimit,
                    sc.ShadowMapTexture.Width,
                    out var lightFrustum1);
                cls[2].UpdateBuffer(sc.LightViewProjectionBuffer1, 0, ref viewProj1);

                cls[2].SetFramebuffer(sc.MidShadowMapFramebuffer);
                cls[2].SetViewport(0, new Viewport(0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height, 0, 1));
                cls[2].SetScissorRect(0, 0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height);
                cls[2].ClearDepthStencil(depthClear);
                Render(gd, cls[2], sc, RenderPasses.ShadowMapMid, lightFrustum1, lightPos, _renderQueues[1], _cullableStage[1], _renderableStage[1], null, true);
            });

            _tasks[2] = Task.Run(() =>
            {
                // Far
                Matrix4x4 viewProj2 = UpdateDirectionalLightMatrices(
                    gd,
                    sc,
                    _midCascadeLimit,
                    _farCascadeLimit,
                    sc.ShadowMapTexture.Width,
                    out var lightFrustum2);
                cls[3].UpdateBuffer(sc.LightViewProjectionBuffer2, 0, ref viewProj2);

                cls[3].SetFramebuffer(sc.FarShadowMapFramebuffer);
                cls[3].SetViewport(0, new Viewport(0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height, 0, 1));
                cls[3].SetScissorRect(0, 0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height);
                cls[3].ClearDepthStencil(depthClear);
                Render(gd, cls[3], sc, RenderPasses.ShadowMapFar, lightFrustum2, lightPos, _renderQueues[2], _cullableStage[2], _renderableStage[2], null, true);
            });

            _tasks[3] = Task.Run(() =>
            {
                // Reflections
                cls[4].SetFramebuffer(sc.ReflectionFramebuffer);
                float scWidth = sc.ReflectionFramebuffer.Width;
                float scHeight = sc.ReflectionFramebuffer.Height;
                cls[4].SetViewport(0, new Viewport(0, 0, scWidth, scHeight, 0, 1));
                cls[4].SetFullViewports();
                cls[4].SetFullScissorRects();
                cls[4].ClearColorTarget(0, RgbaFloat.Black);
                cls[4].ClearDepthStencil(depthClear);

                // Render reflected scene.
                Matrix4x4 planeReflectionMatrix = Matrix4x4.CreateReflection(MirrorMesh.Plane);
                CameraInfo camInfo = new CameraInfo();
                camInfo.CameraLookDirection = Vector3.Normalize(Vector3.Reflect(_camera.LookDirection, MirrorMesh.Plane.Normal));
                camInfo.CameraPosition_WorldSpace = Vector3.Transform(_camera.Position, planeReflectionMatrix);
                cls[4].UpdateBuffer(sc.CameraInfoBuffer, 0, ref camInfo);

                Matrix4x4 view = sc.Camera.ViewMatrix;
                view = planeReflectionMatrix * view;
                cls[4].UpdateBuffer(sc.ViewMatrixBuffer, 0, view);

                Matrix4x4 projection = _camera.ProjectionMatrix;
                cls[4].UpdateBuffer(sc.ReflectionViewProjBuffer, 0, view * projection);

                BoundingFrustum cameraFrustum = new BoundingFrustum(view * projection);
                Render(gd, cls[4], sc, RenderPasses.ReflectionMap, cameraFrustum, _camera.Position, _renderQueues[3], _cullableStage[3], _renderableStage[3], null, true);

                cl.GenerateMipmaps(sc.ReflectionColorTexture);

                // Main scene
                cls[4].SetFramebuffer(sc.MainSceneFramebuffer);
                scWidth = sc.MainSceneFramebuffer.Width;
                scHeight = sc.MainSceneFramebuffer.Height;
                cls[4].SetViewport(0, new Viewport(0, 0, scWidth, scHeight, 0, 1));
                cls[4].SetScissorRect(0, 0, 0, (uint)scWidth, (uint)scHeight);
                cls[4].ClearColorTarget(0, RgbaFloat.Black);
                cls[4].ClearDepthStencil(depthClear);
                sc.UpdateCameraBuffers(cls[4]);
                cameraFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
                Render(gd, cls[4], sc, RenderPasses.Standard, cameraFrustum, _camera.Position, _renderQueues[3], _cullableStage[3], _renderableStage[3], null, true);
                Render(gd, cls[4], sc, RenderPasses.AlphaBlend, cameraFrustum, _camera.Position, _renderQueues[3], _cullableStage[3], _renderableStage[3], null, true);
                Render(gd, cls[4], sc, RenderPasses.Overlay, cameraFrustum, _camera.Position, _renderQueues[3], _cullableStage[3], _renderableStage[3], null, true);
            });

            Task.WaitAll(_tasks);

            foreach (Renderable renderable in _allPerFrameRenderablesSet)
            {
                renderable.UpdatePerFrameResources(gd, _resourceUpdateCL, sc);
            }
            _resourceUpdateCL.End();
            gd.SubmitCommands(_resourceUpdateCL);

            for (int i = 0; i < cls.Length; i++) { cls[i].End(); gd.SubmitCommands(cls[i]); cls[i].Dispose(); }

            if (sc.MainSceneColorTexture.SampleCount != TextureSampleCount.Count1)
            {
                cl.ResolveTexture(sc.MainSceneColorTexture, sc.MainSceneResolvedColorTexture);
            }

            cl.SetFramebuffer(sc.DuplicatorFramebuffer);
            uint fbWidth = sc.DuplicatorFramebuffer.Width;
            uint fbHeight = sc.DuplicatorFramebuffer.Height;
            cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetViewport(1, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetScissorRect(0, 0, 0, fbWidth, fbHeight);
            cl.SetScissorRect(1, 0, 0, fbWidth, fbHeight);
            Render(gd, cl, sc, RenderPasses.Duplicator, new BoundingFrustum(), _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            cl.SetFramebuffer(gd.SwapchainFramebuffer);
            fbWidth = gd.SwapchainFramebuffer.Width;
            fbHeight = gd.SwapchainFramebuffer.Height;
            cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetScissorRect(0, 0, 0, fbWidth, fbHeight);
            Render(gd, cl, sc, RenderPasses.SwapchainOutput, new BoundingFrustum(), _camera.Position, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            cl.End();
            gd.SubmitCommands(cl);
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
            CommandList rc,
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
                renderable.Render(gd, rc, sc, pass);
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
        private CommandList _resourceUpdateCL;

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

            _resourceUpdateCL.Dispose();
        }

        internal void CreateAllDeviceObjects(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            _cullableStage[0].Clear();
            _octree.GetAllContainedObjects(_cullableStage[0]);
            foreach (CullRenderable cr in _cullableStage[0])
            {
                cr.CreateDeviceObjects(gd, cl, sc);
            }
            foreach (Renderable r in _freeRenderables)
            {
                r.CreateDeviceObjects(gd, cl, sc);
            }

            _resourceUpdateCL = gd.ResourceFactory.CreateCommandList();
            _resourceUpdateCL.Name = "Scene Resource Update Command List";
        }

        private class RenderPassesComparer : IEqualityComparer<RenderPasses>
        {
            public bool Equals(RenderPasses x, RenderPasses y) => x == y;
            public int GetHashCode(RenderPasses obj) => ((byte)obj).GetHashCode();
        }
    }
}
