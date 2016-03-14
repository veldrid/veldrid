using ImGuiNET;
using OpenTK;
using OpenTK.Input;
using SharpDX.Mathematics.Interop;
using System;
using System.Numerics;
using Veldrid.Graphics;
using Veldrid.Platform;

namespace Veldrid.RenderDemo
{
    public class ImGuiRenderer : RenderItem
    {
        private readonly DynamicDataProvider<Matrix4x4> _projectionMatrixProvider;
        private readonly Material _material;
        private TextureData _fontTexture;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private float _wheelPosition;

        public ImGuiRenderer(RenderContext rc, NativeWindow window)
        {
            ResourceFactory factory = rc.ResourceFactory;
            _vertexBuffer = factory.CreateVertexBuffer(500, true);
            _indexBuffer = factory.CreateIndexBuffer(200);
            CreateFontsTexture(rc);
            _projectionMatrixProvider = new DynamicDataProvider<Matrix4x4>();

            _material = factory.CreateMaterial("imgui-vertex", "imgui-frag",
                new MaterialVertexInput(32, new MaterialVertexInputElement[]
                {
                    new MaterialVertexInputElement("in_position", VertexSemanticType.Position, VertexElementFormat.Float2),
                    new MaterialVertexInputElement("in_texcoord", VertexSemanticType.TextureCoordinate, VertexElementFormat.Float2),
                    new MaterialVertexInputElement("in_color", VertexSemanticType.Color, VertexElementFormat.Byte4)
                }),
                new MaterialInputs<MaterialGlobalInputElement>(new MaterialGlobalInputElement[]
                {
                    new MaterialGlobalInputElement("ProjectionMatrixUniform", MaterialInputType.Matrix4x4, _projectionMatrixProvider)
                }),
                MaterialInputs<MaterialPerObjectInputElement>.Empty,
                new MaterialTextureInputs(new MaterialTextureInputElement[]
                {
                    new MaterialTextureInputElement("surfaceTexture", _fontTexture)
                }));

            SetPerFrameImGuiData(rc);
            UpdateImGuiInput(window);

            ImGui.NewFrame();
        }

        public unsafe void Render(RenderContext rc)
        {
            ImGui.Render();
            RenderImDrawData(ImGui.GetDrawData(), rc);
            ImGui.NewFrame();
        }

        public RenderOrderKey GetRenderOrderKey()
        {
            return new RenderOrderKey();
        }

        public unsafe void SetPerFrameImGuiData(RenderContext rc)
        {
            IO io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                rc.Window.Width,
                rc.Window.Height);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1.0f);
            io.DeltaTime = 1f / 60f; // TODO: Wrong value
        }

        public unsafe void UpdateImGuiInput(NativeWindow window)
        {
            IO io = ImGui.GetIO();
            MouseState cursorState = Mouse.GetCursorState();
            MouseState mouseState = Mouse.GetState();

            if (window.Bounds.Contains(cursorState.X, cursorState.Y))
            {
                Point windowPoint = window.PointToClient(new Point(cursorState.X, cursorState.Y));
                io.MousePosition = new System.Numerics.Vector2(
                    windowPoint.X / 1f,
                    windowPoint.Y / 1f);
            }
            else
            {
                io.MousePosition = new System.Numerics.Vector2(-1f, -1f);
            }

            io.MouseDown[0] = mouseState.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = mouseState.RightButton == ButtonState.Pressed;
            io.MouseDown[2] = mouseState.MiddleButton == ButtonState.Pressed;

            float newWheelPos = mouseState.WheelPrecise;
            float delta = newWheelPos - _wheelPosition;
            _wheelPosition = newWheelPos;
            io.MouseWheel = delta;
        }

        private unsafe void CreateFontsTexture(RenderContext rc)
        {
            ImGui.LoadDefaultFont();
            IO io = ImGui.GetIO();

            // Build
            var textureData = io.FontAtlas.GetTexDataAsRGBA32();
            int[] pixels = new int[textureData.Width * textureData.Height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = ((int*)textureData.Pixels)[i];
            }

            _fontTexture = new RawTextureDataArray<int>(pixels, textureData.Width, textureData.Height, textureData.BytesPerPixel, PixelFormat.R8_G8_B8_A8);

            // Store our identifier
            io.FontAtlas.SetTexID(420);

            // Cleanup (don't clear the input data if you want to append new fonts later)
            io.FontAtlas.ClearTexData();
        }

        private unsafe void RenderImDrawData(DrawData* draw_data, RenderContext rc)
        {
            VertexDescriptor descriptor = new VertexDescriptor((byte)sizeof(DrawVert), 3, 0, IntPtr.Zero);

            int vertexOffsetInVertices = 0;
            int indexOffsetInElements = 0;

            for (int i = 0; i < draw_data->CmdListsCount; i++)
            {
                DrawList* cmd_list = draw_data->CmdLists[i];

                {
                    DrawVert[] tempVerts = new DrawVert[cmd_list->VtxBuffer.Size];
                    int[] tempIndices = new int[cmd_list->IdxBuffer.Size];

                    for (int g = 0; g < tempVerts.Length; g++)
                    {
                        tempVerts[g] = ((DrawVert*)cmd_list->VtxBuffer.Data)[g];
                    }
                    for (int g = 0; g < tempIndices.Length; g++)
                    {
                        tempIndices[g] = ((ushort*)cmd_list->IdxBuffer.Data)[g];
                    }

                    _vertexBuffer.SetVertexData(tempVerts, descriptor, vertexOffsetInVertices);
                    _indexBuffer.SetIndices(tempIndices, 0, indexOffsetInElements);
                }

                //_vertexBuffer.SetVertexData(new IntPtr(cmd_list->VtxBuffer.Data), descriptor, cmd_list->VtxBuffer.Size, vertexOffsetInVertices);
                //_indexBuffer.SetIndices(new IntPtr(cmd_list->IdxBuffer.Data), IndexFormat.UInt16, sizeof(ushort), cmd_list->IdxBuffer.Size, indexOffsetInElements);

                vertexOffsetInVertices += cmd_list->VtxBuffer.Size;
                indexOffsetInElements += cmd_list->IdxBuffer.Size;
            }

            // Setup orthographic projection matrix into our constant buffer
            {
                var io = ImGui.GetIO();
                Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0.0f,
                    io.DisplaySize.X / io.DisplayFramebufferScale.X,
                    io.DisplaySize.Y / io.DisplayFramebufferScale.Y,
                    0.0f,
                    -1.0f,
                    1.0f);

                _projectionMatrixProvider.Data = mvp;
            }

            rc.SetVertexBuffer(_vertexBuffer);
            rc.SetIndexBuffer(_indexBuffer);
            rc.SetMaterial(_material);

            ImGui.ScaleClipRects(draw_data, ImGui.GetIO().DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data->CmdListsCount; n++)
            {
                DrawList* cmd_list = draw_data->CmdLists[n];
                for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++)
                {
                    DrawCmd* pcmd = &(((DrawCmd*)cmd_list->CmdBuffer.Data)[cmd_i]);
                    if (pcmd->UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        //Console.WriteLine(pcmd->TextureId);
                        rc.SetScissorRectangle(
                            (int)pcmd->ClipRect.X,
                            (int)pcmd->ClipRect.Y,
                            (int)pcmd->ClipRect.Z,
                            (int)pcmd->ClipRect.W);

                        rc.DrawIndexedPrimitives(idx_offset, (int)pcmd->ElemCount, vtx_offset);
                    }

                    idx_offset += (int)pcmd->ElemCount;
                }
                vtx_offset += cmd_list->VtxBuffer.Size;
            }
        }
    }
}
