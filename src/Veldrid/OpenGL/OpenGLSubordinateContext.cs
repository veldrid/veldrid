using System.Collections.Concurrent;
using System.Threading;
using static Veldrid.OpenGLBinding.OpenGLNative;
using static Veldrid.OpenGL.OpenGLUtil;
using Veldrid.OpenGLBinding;
using System;

namespace Veldrid.OpenGL
{
    internal class OpenGLSubordinateContext
    {
        private readonly OpenGLPlatformInfo _info;
        private readonly OpenGLSwapchainFramebuffer _masterFB;
        private readonly BlockingCollection<WorkItem> _workQueue = new BlockingCollection<WorkItem>();
        private bool _terminated;

        private uint _framebuffer;
        private uint _width;
        private uint _height;
        private bool _needsResize = true;

        public OpenGLSubordinateContext(OpenGLPlatformInfo info, OpenGLSwapchainFramebuffer masterFB)
        {
            _info = info;
            _masterFB = masterFB;
            new Thread(RunLoop).Start();
        }

        public void SetSyncToVerticalBlank(bool value)
        {
            _workQueue.Add(new WorkItem { Type = WorkItemType.SetSyncToVerticalBlank, UInt0 = value ? 1u : 0u });
        }

        public void Resize(uint width, uint height)
        {
            _workQueue.Add(new WorkItem { Type = WorkItemType.Resize, UInt0 = width, UInt1 = height });
        }

        public void SwapBuffers(IntPtr sync)
        {
            _workQueue.Add(new WorkItem { Type = WorkItemType.SwapBuffers, Ptr0 = sync });
        }

        public void Terminate()
        {
            ManualResetEventSlim mre = new ManualResetEventSlim();
            _workQueue.Add(new WorkItem() { Type = WorkItemType.Terminate, ResetEvent = mre });
            mre.Wait();
            mre.Dispose();
        }

        private void RunLoop()
        {
            _info.MakeCurrent(_info.OpenGLContextHandle);

            while (!_terminated)
            {
                WorkItem workItem = _workQueue.Take();
                ExecuteWorkItem(workItem);
            }
        }

        private void ExecuteWorkItem(WorkItem workItem)
        {
            switch (workItem.Type)
            {
                case WorkItemType.SetSyncToVerticalBlank:
                {
                    _info.SetSyncToVerticalBlank(workItem.UInt0 == 1);
                    break;
                }
                case WorkItemType.Resize:
                {
                    _width = workItem.UInt0;
                    _height = workItem.UInt1;

                    _needsResize = true;
                    break;
                }
                case WorkItemType.SwapBuffers:
                {
                    IntPtr sync = workItem.Ptr0;
                    glWaitSync(sync, 0, 0xFFFFFFFFFFFFFFFF);
                    CheckLastError();

                    glDeleteSync(sync);
                    CheckLastError();

                    if (_needsResize)
                    {
                        _needsResize = false;

                        if (_framebuffer != 0)
                        {
                            glDeleteFramebuffers(1, ref _framebuffer);
                            CheckLastError();
                        }

                        OpenGLFramebuffer newFB = _masterFB.Framebuffer;
                        glGenFramebuffers(1, out _framebuffer);
                        CheckLastError();

                        glBindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
                        CheckLastError();

                        uint colorTex = Util.AssertSubtype<Texture, OpenGLTexture>(newFB.ColorTargets[0].Target).Texture;
                        glBindTexture(TextureTarget.Texture2D, colorTex);
                        CheckLastError();

                        glFramebufferTexture2D(
                            FramebufferTarget.Framebuffer,
                            GLFramebufferAttachment.ColorAttachment0,
                            TextureTarget.Texture2D,
                            colorTex,
                            0);
                        CheckLastError();

                        if (newFB.DepthTarget != null)
                        {
                            OpenGLTexture glDepthTex = Util.AssertSubtype<Texture, OpenGLTexture>(newFB.DepthTarget.Value.Target);
                            uint depthTexID = glDepthTex.Texture;
                            GLFramebufferAttachment framebufferAttachment = GLFramebufferAttachment.DepthAttachment;
                            if (FormatHelpers.IsStencilFormat(glDepthTex.Format))
                            {
                                framebufferAttachment = GLFramebufferAttachment.DepthStencilAttachment;
                            }

                            glBindTexture(TextureTarget.Texture2D, depthTexID);
                            CheckLastError();

                            glFramebufferTexture2D(
                                FramebufferTarget.Framebuffer,
                                framebufferAttachment,
                                TextureTarget.Texture2D,
                                depthTexID,
                                0);
                            CheckLastError();
                        }

                        FramebufferErrorCode errorCode = glCheckFramebufferStatus(FramebufferTarget.Framebuffer);
                        CheckLastError();
                        if (errorCode != FramebufferErrorCode.FramebufferComplete)
                        {
                            throw new VeldridException("Framebuffer was not successfully created: " + errorCode);
                        }
                    }

                    glBindFramebuffer(FramebufferTarget.ReadFramebuffer, _framebuffer);
                    CheckLastError();

                    glBindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
                    CheckLastError();

                    glBlitFramebuffer(
                        0, 0, (int)_width, (int)_height,
                        0, 0, (int)_width, (int)_height,
                        ClearBufferMask.ColorBufferBit,
                        BlitFramebufferFilter.Nearest);
                    CheckLastError();

                    _info.SwapBuffers();
                    break;
                }
                case WorkItemType.Terminate:
                {
                    glDeleteFramebuffers(1, ref _framebuffer);
                    CheckLastError();

                    _info.DeleteContext(_info.OpenGLContextHandle);
                    _terminated = true;
                    workItem.ResetEvent.Set();
                    break;
                }
                default: throw new InvalidOperationException();
            }
        }

        private struct WorkItem
        {
            public WorkItemType Type;
            public uint UInt0;
            public uint UInt1;
            public IntPtr Ptr0;
            public ManualResetEventSlim ResetEvent;
        }

        private enum WorkItemType
        {
            Resize,
            SetSyncToVerticalBlank,
            SwapBuffers,
            Terminate,
        }
    }
}
