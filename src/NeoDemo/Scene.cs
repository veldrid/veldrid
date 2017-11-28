using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
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
                sc.ShadowMapTexture.Width,
                out BoundingFrustum lightFrustum);
            cl.UpdateBuffer(sc.LightViewProjectionBuffer0, 0, ref viewProj0);
            cl.SetFramebuffer(sc.NearShadowMapFramebuffer);
            cl.SetFullViewports();
            cl.ClearDepthTarget(01f);
            Render(gd, cl, sc, RenderPasses.ShadowMapNear, lightFrustum, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            // Mid
            Matrix4x4 viewProj1 = UpdateDirectionalLightMatrices(
                sc,
                _nearCascadeLimit,
                _midCascadeLimit,
                sc.ShadowMapTexture.Width,
                out lightFrustum);
            cl.UpdateBuffer(sc.LightViewProjectionBuffer1, 0, ref viewProj1);
            cl.SetFramebuffer(sc.MidShadowMapFramebuffer);
            cl.SetFullViewports();
            cl.ClearDepthTarget(1f);
            Render(gd, cl, sc, RenderPasses.ShadowMapMid, lightFrustum, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            // Far
            Matrix4x4 viewProj2 = UpdateDirectionalLightMatrices(
                sc,
                _midCascadeLimit,
                _farCascadeLimit,
                sc.ShadowMapTexture.Width,
                out lightFrustum);
            cl.UpdateBuffer(sc.LightViewProjectionBuffer2, 0, ref viewProj2);
            cl.SetFramebuffer(sc.FarShadowMapFramebuffer);
            cl.SetFullViewports();
            cl.ClearDepthTarget(1f);
            Render(gd, cl, sc, RenderPasses.ShadowMapFar, lightFrustum, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            cl.SetFramebuffer(sc.MainSceneFramebuffer);
            float fbWidth = sc.MainSceneFramebuffer.Width;
            float fbHeight = sc.MainSceneFramebuffer.Height;
            cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetFullViewports();
            cl.SetFullScissorRects();
            cl.ClearDepthTarget(1f);
            BoundingFrustum cameraFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
            Render(gd, cl, sc, RenderPasses.Standard, cameraFrustum, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            Render(gd, cl, sc, RenderPasses.AlphaBlend, cameraFrustum, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            Render(gd, cl, sc, RenderPasses.Overlay, cameraFrustum, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            if (sc.MainSceneColorTexture.SampleCount != TextureSampleCount.Count1)
            {
                cl.ResolveTexture(sc.MainSceneColorTexture, sc.MainSceneResolvedColorTexture);
            }

            cl.SetFramebuffer(sc.DuplicatorFramebuffer);
            fbWidth = sc.DuplicatorFramebuffer.Width;
            fbHeight = sc.DuplicatorFramebuffer.Height;
            cl.SetFullViewports();
            Render(gd, cl, sc, RenderPasses.Duplicator, new BoundingFrustum(), _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            cl.SetFramebuffer(gd.SwapchainFramebuffer);
            fbWidth = gd.SwapchainFramebuffer.Width;
            fbHeight = gd.SwapchainFramebuffer.Height;
            cl.SetFullViewports();
            Render(gd, cl, sc, RenderPasses.SwapchainOutput, new BoundingFrustum(), _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            cl.End();

            _resourceUpdateCL.Begin();
            foreach (Renderable renderable in _allPerFrameRenderablesSet)
            {
                renderable.UpdatePerFrameResources(gd, _resourceUpdateCL, sc);
            }
            _resourceUpdateCL.End();
            gd.ExecuteCommands(_resourceUpdateCL);

            gd.ExecuteCommands(cl);
        }

        private void RenderAllMultiThreaded(GraphicsDevice gd, CommandList cl, SceneContext sc)
        {
            Matrix4x4 cameraProj = Camera.ProjectionMatrix;
            Vector4 nearLimitCS = Vector4.Transform(new Vector3(0, 0, -_nearCascadeLimit), cameraProj);
            Vector4 midLimitCS = Vector4.Transform(new Vector3(0, 0, -_midCascadeLimit), cameraProj);
            Vector4 farLimitCS = Vector4.Transform(new Vector3(0, 0, -_farCascadeLimit), cameraProj);

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
                    sc,
                    Camera.NearDistance,
                    _nearCascadeLimit,
                    sc.ShadowMapTexture.Width,
                    out BoundingFrustum lightFrustum0);
                cls[1].UpdateBuffer(sc.LightViewProjectionBuffer0, 0, ref viewProj0);

                cls[1].SetFramebuffer(sc.NearShadowMapFramebuffer);
                cls[1].SetViewport(0, new Viewport(0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height, 0, 1));
                cls[1].SetScissorRect(0, 0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height);
                cls[1].ClearDepthTarget(1f);
                Render(gd, cls[1], sc, RenderPasses.ShadowMapNear, lightFrustum0, _renderQueues[0], _cullableStage[0], _renderableStage[0], null, true);
            });

            _tasks[1] = Task.Run(() =>
            {
                // Mid
                Matrix4x4 viewProj1 = UpdateDirectionalLightMatrices(
                    sc,
                    _nearCascadeLimit,
                    _midCascadeLimit,
                    sc.ShadowMapTexture.Width,
                    out var lightFrustum1);
                cls[2].UpdateBuffer(sc.LightViewProjectionBuffer1, 0, ref viewProj1);
                
                cls[2].SetFramebuffer(sc.MidShadowMapFramebuffer);
                cls[2].SetViewport(0, new Viewport(0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height, 0, 1));
                cls[2].SetScissorRect(0, 0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height);
                cls[2].ClearDepthTarget(1f);
                Render(gd, cls[2], sc, RenderPasses.ShadowMapMid, lightFrustum1, _renderQueues[1], _cullableStage[1], _renderableStage[1], null, true);
            });

            _tasks[2] = Task.Run(() =>
            {
                // Far
                Matrix4x4 viewProj2 = UpdateDirectionalLightMatrices(
                    sc,
                    _midCascadeLimit,
                    _farCascadeLimit,
                    sc.ShadowMapTexture.Width,
                    out var lightFrustum2);
                cls[3].UpdateBuffer(sc.LightViewProjectionBuffer2, 0, ref viewProj2);
                
                cls[3].SetFramebuffer(sc.FarShadowMapFramebuffer);
                cls[3].SetViewport(0, new Viewport(0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height, 0, 1));
                cls[3].SetScissorRect(0, 0, 0, sc.ShadowMapTexture.Width, sc.ShadowMapTexture.Height);
                cls[3].ClearDepthTarget(1f);
                Render(gd, cls[3], sc, RenderPasses.ShadowMapFar, lightFrustum2, _renderQueues[2], _cullableStage[2], _renderableStage[2], null, true);
            });

            _tasks[3] = Task.Run(() =>
            {
                cls[4].SetFramebuffer(sc.MainSceneFramebuffer);
                float scWidth = sc.MainSceneFramebuffer.Width;
                float scHeight = sc.MainSceneFramebuffer.Height;
                cls[4].SetViewport(0, new Viewport(0, 0, scWidth, scHeight, 0, 1));
                cls[4].SetScissorRect(0, 0, 0, (uint)scWidth, (uint)scHeight);
                cls[4].ClearColorTarget(0, RgbaFloat.Black);
                cls[4].ClearDepthTarget(1f);
                BoundingFrustum cameraFrustum = new BoundingFrustum(_camera.ViewMatrix * _camera.ProjectionMatrix);
                Render(gd, cls[4], sc, RenderPasses.Standard, cameraFrustum, _renderQueues[3], _cullableStage[3], _renderableStage[3], null, true);
                Render(gd, cls[4], sc, RenderPasses.AlphaBlend, cameraFrustum, _renderQueues[3], _cullableStage[3], _renderableStage[3], null, true);
                Render(gd, cls[4], sc, RenderPasses.Overlay, cameraFrustum, _renderQueues[3], _cullableStage[3], _renderableStage[3], null, true);
            });

            Task.WaitAll(_tasks);

            foreach (Renderable renderable in _allPerFrameRenderablesSet)
            {
                renderable.UpdatePerFrameResources(gd, _resourceUpdateCL, sc);
            }
            _resourceUpdateCL.End();
            gd.ExecuteCommands(_resourceUpdateCL);

            for (int i = 0; i < cls.Length; i++) { cls[i].End(); gd.ExecuteCommands(cls[i]); cls[i].Dispose(); }

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
            Render(gd, cl, sc, RenderPasses.Duplicator, new BoundingFrustum(), _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);
            cl.SetFramebuffer(gd.SwapchainFramebuffer);
            fbWidth = gd.SwapchainFramebuffer.Width;
            fbHeight = gd.SwapchainFramebuffer.Height;
            cl.SetViewport(0, new Viewport(0, 0, fbWidth, fbHeight, 0, 1));
            cl.SetScissorRect(0, 0, 0, fbWidth, fbHeight);
            Render(gd, cl, sc, RenderPasses.SwapchainOutput, new BoundingFrustum(), _renderQueues[0], _cullableStage[0], _renderableStage[0], null, false);

            cl.End();
            gd.ExecuteCommands(cl);
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

            lightFrustum = new BoundingFrustum(viewProjectionMatrix);
            return viewProjectionMatrix;
        }

        public void Render(
            GraphicsDevice gd,
            CommandList rc,
            SceneContext sc,
            RenderPasses pass,
            BoundingFrustum frustum,
            RenderQueue renderQueue,
            List<CullRenderable> cullRenderableList,
            List<Renderable> renderableList,
            Comparer<RenderItemIndex> comparer,
            bool threaded)
        {
            renderQueue.Clear();

            cullRenderableList.Clear();
            CollectVisibleObjects(ref frustum, pass, cullRenderableList);
            renderQueue.AddRange(cullRenderableList, _camera.Position);

            renderableList.Clear();
            CollectFreeObjects(pass, renderableList);
            renderQueue.AddRange(renderableList, _camera.Position);

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
            gd.SetResourceName(_resourceUpdateCL, "Scene Resource Update Command List");
        }

        private class RenderPassesComparer : IEqualityComparer<RenderPasses>
        {
            public bool Equals(RenderPasses x, RenderPasses y) => x == y;
            public int GetHashCode(RenderPasses obj) => ((byte)obj).GetHashCode();
        }
    }
}
