using System;
using System.Runtime.InteropServices;

namespace Veldrid.OpenGLBinding
{
    // uint = uint
    // GLuint = uint
    // GLuint64 = uint64
    // GLenum = uint
    // Glclampf = 32-bit float, [0, 1]
    public static unsafe class OpenGLNative
    {
        private static Func<string, IntPtr> s_getProcAddress;

        private delegate void glGenVertexArrays_t(uint n, out uint arrays);
        private static glGenVertexArrays_t p_glGenVertexArrays;
        public static void glGenVertexArrays(uint n, out uint arrays) => p_glGenVertexArrays(n, out arrays);

        private delegate uint glGetError_t();
        private static glGetError_t p_glGetError;
        public static uint glGetError() => p_glGetError();

        private delegate void glBindVertexArray_t(uint array);
        private static glBindVertexArray_t p_glBindVertexArray;
        public static void glBindVertexArray(uint array) => p_glBindVertexArray(array);

        private delegate void glClearColor_t(float red, float green, float blue, float alpha);
        private static glClearColor_t p_glClearColor;
        public static void glClearColor(float red, float green, float blue, float alpha)
            => p_glClearColor(red, green, blue, alpha);

        private delegate void glDrawBuffer_t(DrawBufferMode mode);
        private static glDrawBuffer_t p_glDrawBuffer;
        public static void glDrawBuffer(DrawBufferMode mode) => p_glDrawBuffer(mode);

        private delegate void glDrawBuffers_t(uint n, DrawBuffersEnum* bufs);
        private static glDrawBuffers_t p_glDrawBuffers;
        public static void glDrawBuffers(uint n, DrawBuffersEnum* bufs) => p_glDrawBuffers(n, bufs);

        private delegate void glClear_t(ClearBufferMask mask);
        private static glClear_t p_glClear;
        public static void glClear(ClearBufferMask mask) => p_glClear(mask);

        private delegate void glClearDepth_t(double depth);
        private static glClearDepth_t p_glClearDepth;
        public static void glClearDepth(double depth) => p_glClearDepth(depth);

        private delegate void glClearDepthf_t(float depth);
        private static glClearDepthf_t p_glClearDepthf;
        public static void glClearDepthf(float depth) => p_glClearDepthf(depth);

        private delegate void glDrawElements_t(PrimitiveType mode, uint count, DrawElementsType type, void* indices);
        private static glDrawElements_t p_glDrawElements;
        public static void glDrawElements(PrimitiveType mode, uint count, DrawElementsType type, void* indices)
            => p_glDrawElements(mode, count, type, indices);

        private delegate void glDrawElementsBaseVertex_t(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            int basevertex);
        private static glDrawElementsBaseVertex_t p_glDrawElementsBaseVertex;
        public static void glDrawElementsBaseVertex(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            int basevertex) => p_glDrawElementsBaseVertex(mode, count, type, indices, basevertex);

        private delegate void glDrawElementsInstanced_t(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount);
        private static glDrawElementsInstanced_t p_glDrawElementsInstanced;
        public static void glDrawElementsInstanced(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount) => p_glDrawElementsInstanced(mode, count, type, indices, primcount);

        private delegate void glDrawElementsInstancedBaseVertex_t(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount,
            int basevertex);
        private static glDrawElementsInstancedBaseVertex_t p_glDrawElementsInstancedBaseVertex;
        public static void glDrawElementsInstancedBaseVertex(
            PrimitiveType mode,
            uint count,
            DrawElementsType type,
            void* indices,
            uint primcount,
            int basevertex) => p_glDrawElementsInstancedBaseVertex(mode, count, type, indices, primcount, basevertex);

        private delegate void glGenBuffers_t(uint n, out uint buffers);
        private static glGenBuffers_t p_glGenBuffers;
        public static void glGenBuffers(uint n, out uint buffers) => p_glGenBuffers(n, out buffers);

        private delegate void glDeleteBuffers_t(uint n, ref uint buffers);
        private static glDeleteBuffers_t p_glDeleteBuffers;
        public static void glDeleteBuffers(uint n, ref uint buffers) => p_glDeleteBuffers(n, ref buffers);

        private delegate void glGenFramebuffers_t(uint n, out uint ids);
        private static glGenFramebuffers_t p_glGenFramebuffers;
        public static void glGenFramebuffers(uint n, out uint ids) => p_glGenFramebuffers(n, out ids);

        private delegate void glActiveTexture_t(TextureUnit texture);
        private static glActiveTexture_t p_glActiveTexture;
        public static void glActiveTexture(TextureUnit texture) => p_glActiveTexture(texture);

        private delegate void glFramebufferTexture2D_t(
            FramebufferTarget target,
            FramebufferAttachment attachment,
            TextureTarget textarget,
            uint texture,
            int level);
        private static glFramebufferTexture2D_t p_glFramebufferTexture2D;
        public static void glFramebufferTexture2D(
            FramebufferTarget target,
            FramebufferAttachment attachment,
            TextureTarget textarget,
            uint texture,
            int level) => p_glFramebufferTexture2D(target, attachment, textarget, texture, level);

        private delegate void glBindTexture_t(TextureTarget target, uint texture);
        private static glBindTexture_t p_glBindTexture;
        public static void glBindTexture(TextureTarget target, uint texture) => p_glBindTexture(target, texture);

        private delegate void glBindFramebuffer_t(FramebufferTarget target, uint framebuffer);
        private static glBindFramebuffer_t p_glBindFramebuffer;
        public static void glBindFramebuffer(FramebufferTarget target, uint framebuffer)
            => p_glBindFramebuffer(target, framebuffer);

        private delegate void glDeleteFramebuffers_t(uint n, ref uint framebuffers);
        private static glDeleteFramebuffers_t p_glDeleteFramebuffers;
        public static void glDeleteFramebuffers(uint n, ref uint framebuffers) => p_glDeleteFramebuffers(n, ref framebuffers);

        private delegate void glGenTextures_t(uint n, out uint textures);
        private static glGenTextures_t p_glGenTextures;
        public static void glGenTextures(uint n, out uint textures) => p_glGenTextures(n, out textures);

        private delegate void glDeleteTextures_t(uint n, ref uint textures);
        private static glDeleteTextures_t p_glDeleteTextures;
        public static void glDeleteTextures(uint n, ref uint textures) => p_glDeleteTextures(n, ref textures);

        private delegate FramebufferErrorCode glCheckFramebufferStatus_t(FramebufferTarget target);
        private static glCheckFramebufferStatus_t p_glCheckFramebufferStatus;
        public static FramebufferErrorCode glCheckFramebufferStatus(FramebufferTarget target)
            => p_glCheckFramebufferStatus(target);

        private delegate void glBindBuffer_t(BufferTarget target, uint buffer);
        private static glBindBuffer_t p_glBindBuffer;
        public static void glBindBuffer(BufferTarget target, uint buffer) => p_glBindBuffer(target, buffer);

        private delegate void glViewportIndexedf_t(uint index, float x, float y, float w, float h);
        private static glViewportIndexedf_t p_glViewportIndexed;
        public static void glViewportIndexed(uint index, float x, float y, float w, float h)
            => p_glViewportIndexed(index, x, y, w, h);

        private delegate void glDepthRangeIndexed_t(uint index, double nearVal, double farVal);
        private static glDepthRangeIndexed_t p_glDepthRangeIndexed;
        public static void glDepthRangeIndexed(uint index, double nearVal, double farVal)
            => p_glDepthRangeIndexed(index, nearVal, farVal);

        private delegate void glBufferSubData_t(BufferTarget target, IntPtr offset, UIntPtr size, void* data);
        private static glBufferSubData_t p_glBufferSubData;
        public static void glBufferSubData(BufferTarget target, IntPtr offset, UIntPtr size, void* data)
            => p_glBufferSubData(target, offset, size, data);

        private delegate void glNamedBufferSubData_t(uint buffer, IntPtr offset, uint size, void* data);
        private static glNamedBufferSubData_t p_glNamedBufferSubData;
        public static void glNamedBufferSubData(uint buffer, IntPtr offset, uint size, void* data)
            => p_glNamedBufferSubData(buffer, offset, size, data);

        private delegate void glScissorIndexed_t(uint index, int left, int bottom, uint width, uint height);
        private static glScissorIndexed_t p_glScissorIndexed;
        public static void glScissorIndexed(uint index, int left, int bottom, uint width, uint height)
            => p_glScissorIndexed(index, left, bottom, width, height);

        private delegate void glPixelStorei_t(PixelStoreParameter pname, int param);
        private static glPixelStorei_t p_glPixelStorei;
        public static void glPixelStorei(PixelStoreParameter pname, int param) => p_glPixelStorei(pname, param);

        private delegate void glTexSubImage2D_t(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            uint width,
            uint height,
            GLPixelFormat format,
            GLPixelType type,
            void* pixels);
        private static glTexSubImage2D_t p_glTexSubImage2D;
        public static void glTexSubImage2D(
            TextureTarget target,
            int level,
            int xoffset,
            int yoffset,
            uint width,
            uint height,
            GLPixelFormat format,
            GLPixelType type,
            void* pixels) => p_glTexSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixels);

        private delegate void glShaderSource_t(uint shader, uint count, byte** @string, int* length);
        private static glShaderSource_t p_glShaderSource;
        public static void glShaderSource(uint shader, uint count, byte** @string, int* length)
            => p_glShaderSource(shader, count, @string, length);

        private delegate uint glCreateShader_t(ShaderType shaderType);
        private static glCreateShader_t p_glCreateShader;
        public static uint glCreateShader(ShaderType shaderType) => p_glCreateShader(shaderType);

        private delegate void glCompileShader_t(uint shader);
        private static glCompileShader_t p_glCompileShader;
        public static void glCompileShader(uint shader) => p_glCompileShader(shader);

        private delegate void glGetShaderiv_t(uint shader, ShaderParameter pname, int* @params);
        private static glGetShaderiv_t p_glGetShaderiv;
        public static void glGetShaderiv(uint shader, ShaderParameter pname, int* @params)
            => p_glGetShaderiv(shader, pname, @params);

        private delegate void glGetShaderInfoLog_t(uint shader, uint maxLength, uint* length, byte* infoLog);
        private static glGetShaderInfoLog_t p_glGetShaderInfoLog;
        public static void glGetShaderInfoLog(uint shader, uint maxLength, uint* length, byte* infoLog)
            => p_glGetShaderInfoLog(shader, maxLength, length, infoLog);

        private delegate void glDeleteShader_t(uint shader);
        private static glDeleteShader_t p_glDeleteShader;
        public static void glDeleteShader(uint shader) => p_glDeleteShader(shader);

        private delegate void glGenSamplers_t(uint n, out uint samplers);
        private static glGenSamplers_t p_glGenSamplers;
        public static void glGenSamplers(uint n, out uint samplers) => p_glGenSamplers(n, out samplers);

        private delegate void glSamplerParameterf_t(uint sampler, SamplerParameterName pname, float param);
        private static glSamplerParameterf_t p_glSamplerParameterf;
        public static void glSamplerParameterf(uint sampler, SamplerParameterName pname, float param)
            => p_glSamplerParameterf(sampler, pname, param);

        private delegate void glSamplerParameteri_t(uint sampler, SamplerParameterName pname, int param);
        private static glSamplerParameteri_t p_glSamplerParameteri;
        public static void glSamplerParameteri(uint sampler, SamplerParameterName pname, int param)
            => p_glSamplerParameteri(sampler, pname, param);

        private delegate void glSamplerParameterfv_t(uint sampler, SamplerParameterName pname, float* @params);
        private static glSamplerParameterfv_t p_glSamplerParameterfv;
        public static void glSamplerParameterfv(uint sampler, SamplerParameterName pname, float* @params)
            => p_glSamplerParameterfv(sampler, pname, @params);

        private delegate void glBindSampler_t(uint unit, uint sampler);
        private static glBindSampler_t p_glBindSampler;
        public static void glBindSampler(uint unit, uint sampler) => p_glBindSampler(unit, sampler);

        private delegate void glDeleteSamplers_t(uint n, ref uint samplers);
        private static glDeleteSamplers_t p_glDeleteSamplers;
        public static void glDeleteSamplers(uint n, ref uint samplers) => p_glDeleteSamplers(n, ref samplers);

        private delegate void glBlendFuncSeparatei_t(
            uint buf,
            BlendingFactorSrc srcRGB,
            BlendingFactorDest dstRGB,
            BlendingFactorSrc srcAlpha,
            BlendingFactorDest dstAlpha);
        private static glBlendFuncSeparatei_t p_glBlendFuncSeparatei;
        public static void glBlendFuncSeparatei(
            uint buf,
            BlendingFactorSrc srcRGB,
            BlendingFactorDest dstRGB,
            BlendingFactorSrc srcAlpha,
            BlendingFactorDest dstAlpha) => p_glBlendFuncSeparatei(buf, srcRGB, dstRGB, srcAlpha, dstAlpha);

        private delegate void glEnable_t(EnableCap cap);
        private static glEnable_t p_glEnable;
        public static void glEnable(EnableCap cap) => p_glEnable(cap);

        private delegate void glEnablei_t(EnableCap cap, uint index);
        private static glEnablei_t p_glEnablei;
        public static void glEnablei(EnableCap cap, uint index) => p_glEnablei(cap, index);

        private delegate void glDisable_t(EnableCap cap);
        private static glDisable_t p_glDisable;
        public static void glDisable(EnableCap cap) => p_glDisable(cap);

        private delegate void glDisablei_t(EnableCap cap, uint index);
        private static glDisablei_t p_glDisablei;
        public static void glDisablei(EnableCap cap, uint index) => p_glDisablei(cap, index);

        private delegate void glBlendEquationSeparatei_t(uint buf, BlendEquationMode modeRGB, BlendEquationMode modeAlpha);
        private static glBlendEquationSeparatei_t p_glBlendEquationSeparatei;
        public static void glBlendEquationSeparatei(uint buf, BlendEquationMode modeRGB, BlendEquationMode modeAlpha)
            => p_glBlendEquationSeparatei(buf, modeRGB, modeAlpha);

        private delegate void glBlendColor_t(float red, float green, float blue, float alpha);
        private static glBlendColor_t p_glBlendColor;
        public static void glBlendColor(float red, float green, float blue, float alpha)
            => p_glBlendColor(red, green, blue, alpha);

        private delegate void glDepthFunc_t(DepthFunction func);
        private static glDepthFunc_t p_glDepthFunc;
        public static void glDepthFunc(DepthFunction func) => p_glDepthFunc(func);

        private delegate void glDepthMask_t(GLboolean flag);
        private static glDepthMask_t p_glDepthMask;
        public static void glDepthMask(GLboolean flag) => p_glDepthMask(flag);

        private delegate void glCullFace_t(CullFaceMode mode);
        private static glCullFace_t p_glCullFace;
        public static void glCullFace(CullFaceMode mode) => p_glCullFace(mode);

        private delegate void glPolygonMode_t(MaterialFace face, PolygonMode mode);
        private static glPolygonMode_t p_glPolygonMode;
        public static void glPolygonMode(MaterialFace face, PolygonMode mode) => p_glPolygonMode(face, mode);

        private delegate uint glCreateProgram_t();
        private static glCreateProgram_t p_glCreateProgram;
        public static uint glCreateProgram() => p_glCreateProgram();

        private delegate void glAttachShader_t(uint program, uint shader);
        private static glAttachShader_t p_glAttachShader;
        public static void glAttachShader(uint program, uint shader) => p_glAttachShader(program, shader);

        private delegate void glBindAttribLocation_t(uint program, uint index, byte* name);
        private static glBindAttribLocation_t p_glBindAttribLocation;
        public static void glBindAttribLocation(uint program, uint index, byte* name)
            => p_glBindAttribLocation(program, index, name);

        private delegate void glLinkProgram_t(uint program);
        private static glLinkProgram_t p_glLinkProgram;
        public static void glLinkProgram(uint program) => p_glLinkProgram(program);

        private delegate void glGetProgramiv_t(uint program, GetProgramParameterName pname, int* @params);
        private static glGetProgramiv_t p_glGetProgramiv;
        public static void glGetProgramiv(uint program, GetProgramParameterName pname, int* @params)
            => p_glGetProgramiv(program, pname, @params);

        private delegate void glGetProgramInfoLog_t(uint program, uint maxLength, uint* length, byte* infoLog);
        private static glGetProgramInfoLog_t p_glGetProgramInfoLog;
        public static void glGetProgramInfoLog(uint program, uint maxLength, uint* length, byte* infoLog)
            => p_glGetProgramInfoLog(program, maxLength, length, infoLog);

        private delegate void glUniformBlockBinding_t(uint program, uint uniformBlockIndex, uint uniformBlockBinding);
        private static glUniformBlockBinding_t p_glUniformBlockBinding;
        public static void glUniformBlockBinding(uint program, uint uniformBlockIndex, uint uniformBlockBinding)
            => p_glUniformBlockBinding(program, uniformBlockIndex, uniformBlockBinding);

        private delegate void glDeleteProgram_t(uint program);
        private static glDeleteProgram_t p_glDeleteProgram;
        public static void glDeleteProgram(uint program) => p_glDeleteProgram(program);

        private delegate void glUniform1i_t(int location, int v0);
        private static glUniform1i_t p_glUniform1i;
        public static void glUniform1i(int location, int v0) => p_glUniform1i(location, v0);

        private delegate uint glGetUniformBlockIndex_t(uint program, byte* uniformBlockName);
        private static glGetUniformBlockIndex_t p_glGetUniformBlockIndex;
        public static uint glGetUniformBlockIndex(uint program, byte* uniformBlockName)
            => p_glGetUniformBlockIndex(program, uniformBlockName);

        private delegate int glGetUniformLocation_t(uint program, byte* name);
        private static glGetUniformLocation_t p_glGetUniformLocation;
        public static int glGetUniformLocation(uint program, byte* name) => p_glGetUniformLocation(program, name);

        private delegate int glGetAttribLocation_t(uint program, byte* name);
        private static glGetAttribLocation_t p_glGetAttribLocation;
        public static int glGetAttribLocation(uint program, byte* name) => p_glGetAttribLocation(program, name);

        private delegate void glUseProgram_t(uint program);
        private static glUseProgram_t p_glUseProgram;
        public static void glUseProgram(uint program) => p_glUseProgram(program);

        private delegate void glBindBufferRange_t(
            BufferRangeTarget target,
            uint index,
            uint buffer,
            IntPtr offset,
            UIntPtr size);
        private static glBindBufferRange_t p_glBindBufferRange;
        public static void glBindBufferRange(
            BufferRangeTarget target,
            uint index,
            uint buffer,
            IntPtr offset,
            UIntPtr size) => p_glBindBufferRange(target, index, buffer, offset, size);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        public delegate void DebugProc(
            DebugSource source,
            DebugType type,
            uint id,
            DebugSeverity severity,
            uint length,
            byte* message,
            void* userParam);

        private delegate void glDebugMessageCallback_t(DebugProc callback, void* userParam);
        private static glDebugMessageCallback_t p_glDebugMessageCallback;
        public static void glDebugMessageCallback(DebugProc callback, void* userParam)
            => p_glDebugMessageCallback(callback, userParam);

        private delegate void glNamedBufferData_t(uint buffer, uint size, void* data, BufferUsageHint usage);
        private static glNamedBufferData_t p_glNamedBufferData;
        public static void glNamedBufferData(uint buffer, uint size, void* data, BufferUsageHint usage)
            => p_glNamedBufferData(buffer, size, data, usage);

        private delegate void glTexImage2D_t(
            TextureTarget target,
            int level,
            PixelInternalFormat internalFormat,
            uint width,
            uint height,
            int border,
            GLPixelFormat format,
            GLPixelType type,
            void* data);
        private static glTexImage2D_t p_glTexImage2D;
        public static void glTexImage2D(
            TextureTarget target,
            int level,
            PixelInternalFormat internalFormat,
            uint width,
            uint height,
            int border,
            GLPixelFormat format,
            GLPixelType type,
            void* data) => p_glTexImage2D(target, level, internalFormat, width, height, border, format, type, data);

        private delegate void glEnableVertexAttribArray_t(uint index);
        private static glEnableVertexAttribArray_t p_glEnableVertexAttribArray;
        public static void glEnableVertexAttribArray(uint index) => p_glEnableVertexAttribArray(index);

        private delegate void glDisableVertexAttribArray_t(uint index);
        private static glDisableVertexAttribArray_t p_glDisableVertexAttribArray;
        public static void glDisableVertexAttribArray(uint index) => p_glDisableVertexAttribArray(index);

        private delegate void glVertexAttribPointer_t(
            uint index,
            int size,
            VertexAttribPointerType type,
            GLboolean normalized,
            uint stride,
            void* pointer);
        private static glVertexAttribPointer_t p_glVertexAttribPointer;
        public static void glVertexAttribPointer(
            uint index,
            int size,
            VertexAttribPointerType type,
            GLboolean normalized,
            uint stride,
            void* pointer) => p_glVertexAttribPointer(index, size, type, normalized, stride, pointer);

        private delegate void glVertexAttribIPointer_t(
            uint index,
            int size,
            VertexAttribPointerType type,
            GLboolean normalized,
            uint stride,
            void* pointer);
        private static glVertexAttribIPointer_t p_glVertexAttribIPointer;
        public static void glVertexAttribIPointer(
            uint index,
            int size,
            VertexAttribPointerType type,
            GLboolean normalized,
            uint stride,
            void* pointer) => p_glVertexAttribIPointer(index, size, type, normalized, stride, pointer);

        private delegate void glVertexAttribDivisor_t(uint index, uint divisor);
        private static glVertexAttribDivisor_t p_glVertexAttribDivisor;
        public static void glVertexAttribDivisor(uint index, uint divisor) => p_glVertexAttribDivisor(index, divisor);

        private delegate void glFrontFace_t(FrontFaceDirection mode);
        private static glFrontFace_t p_glFrontFace;
        public static void glFrontFace(FrontFaceDirection mode) => p_glFrontFace(mode);

        private delegate void glGetIntegerv_t(GetPName pname, int* data);
        private static glGetIntegerv_t p_glGetIntegerv;
        public static void glGetIntegerv(GetPName pname, int* data) => p_glGetIntegerv(pname, data);

        private delegate void glBindTextureUnit_t(uint unit, uint texture);
        private static glBindTextureUnit_t p_glBindTextureUnit;
        public static void glBindTextureUnit(uint unit, uint texture) => p_glBindTextureUnit(unit, texture);

        private delegate void glTexParameteri_t(TextureTarget target, TextureParameterName pname, int param);
        private static glTexParameteri_t p_glTexParameteri;
        public static void glTexParameteri(TextureTarget target, TextureParameterName pname, int param)
            => p_glTexParameteri(target, pname, param);

        private delegate byte* glGetStringi_t(StringNameIndexed name, uint index);
        private static glGetStringi_t p_glGetStringi;
        public static byte* glGetStringi(StringNameIndexed name, uint index) => p_glGetStringi(name, index);

        private delegate void glObjectLabel_t(ObjectLabelIdentifier identifier, uint name, uint length, byte* label);
        private static glObjectLabel_t p_glObjectLabel;
        public static void glObjectLabel(ObjectLabelIdentifier identifier, uint name, uint length, byte* label)
            => p_glObjectLabel(identifier, name, length, label);

        public static void LoadAllFunctions(IntPtr glContext, Func<string, IntPtr> getProcAddress)
        {
            s_getProcAddress = getProcAddress;

            LoadFunction(out p_glGenVertexArrays);
            LoadFunction(out p_glGetError);
            LoadFunction(out p_glBindVertexArray);
            LoadFunction(out p_glClearColor);
            LoadFunction(out p_glDrawBuffer);
            LoadFunction(out p_glDrawBuffers);
            LoadFunction(out p_glClear);
            LoadFunction(out p_glClearDepth);
            LoadFunction(out p_glClearDepthf);
            LoadFunction(out p_glDrawElements);
            LoadFunction(out p_glDrawElementsBaseVertex);
            LoadFunction(out p_glDrawElementsInstanced);
            LoadFunction(out p_glDrawElementsInstancedBaseVertex);
            LoadFunction(out p_glGenBuffers);
            LoadFunction(out p_glDeleteBuffers);
            LoadFunction(out p_glGenFramebuffers);
            LoadFunction(out p_glActiveTexture);
            LoadFunction(out p_glFramebufferTexture2D);
            LoadFunction(out p_glBindTexture);
            LoadFunction(out p_glBindFramebuffer);
            LoadFunction(out p_glDeleteFramebuffers);
            LoadFunction(out p_glGenTextures);
            LoadFunction(out p_glDeleteTextures);
            LoadFunction(out p_glCheckFramebufferStatus);
            LoadFunction(out p_glBindBuffer);
            LoadFunction(out p_glViewportIndexed);
            LoadFunction(out p_glDepthRangeIndexed);
            LoadFunction(out p_glBufferSubData);
            LoadFunction(out p_glNamedBufferSubData);
            LoadFunction(out p_glScissorIndexed);
            LoadFunction(out p_glTexSubImage2D);
            LoadFunction(out p_glPixelStorei);
            LoadFunction(out p_glShaderSource);
            LoadFunction(out p_glCreateShader);
            LoadFunction(out p_glCompileShader);
            LoadFunction(out p_glGetShaderiv);
            LoadFunction(out p_glGetShaderInfoLog);
            LoadFunction(out p_glDeleteShader);
            LoadFunction(out p_glGenSamplers);
            LoadFunction(out p_glSamplerParameterf);
            LoadFunction(out p_glSamplerParameteri);
            LoadFunction(out p_glSamplerParameterfv);
            LoadFunction(out p_glBindSampler);
            LoadFunction(out p_glDeleteSamplers);
            LoadFunction(out p_glBlendFuncSeparatei);
            LoadFunction(out p_glEnable);
            LoadFunction(out p_glEnablei);
            LoadFunction(out p_glDisable);
            LoadFunction(out p_glDisablei);
            LoadFunction(out p_glBlendEquationSeparatei);
            LoadFunction(out p_glBlendColor);
            LoadFunction(out p_glDepthFunc);
            LoadFunction(out p_glDepthMask);
            LoadFunction(out p_glCullFace);
            LoadFunction(out p_glPolygonMode);
            LoadFunction(out p_glCreateProgram);
            LoadFunction(out p_glAttachShader);
            LoadFunction(out p_glBindAttribLocation);
            LoadFunction(out p_glLinkProgram);
            LoadFunction(out p_glGetProgramiv);
            LoadFunction(out p_glGetProgramInfoLog);
            LoadFunction(out p_glUniformBlockBinding);
            LoadFunction(out p_glDeleteProgram);
            LoadFunction(out p_glUniform1i);
            LoadFunction(out p_glGetUniformBlockIndex);
            LoadFunction(out p_glGetUniformLocation);
            LoadFunction(out p_glGetAttribLocation);
            LoadFunction(out p_glUseProgram);
            LoadFunction(out p_glBindBufferRange);
            LoadFunction(out p_glDebugMessageCallback);
            LoadFunction(out p_glNamedBufferData);
            LoadFunction(out p_glTexImage2D);
            LoadFunction(out p_glEnableVertexAttribArray);
            LoadFunction(out p_glDisableVertexAttribArray);
            LoadFunction(out p_glVertexAttribPointer);
            LoadFunction(out p_glVertexAttribIPointer);
            LoadFunction(out p_glVertexAttribDivisor);
            LoadFunction(out p_glFrontFace);
            LoadFunction(out p_glGetIntegerv);
            LoadFunction(out p_glBindTextureUnit);
            LoadFunction(out p_glTexParameteri);
            LoadFunction(out p_glGetStringi);
            LoadFunction(out p_glObjectLabel);
        }

        private static void LoadFunction<T>(out T field)
        {
            // TODO: Remove this reflection.
            string name = typeof(T).Name;
            name = name.Substring(0, name.Length - 2); // Remove _t
            IntPtr funcPtr = s_getProcAddress(name);
            if (funcPtr != IntPtr.Zero)
            {
                field = Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
            }
            else
            {
                field = default(T);
            }
        }
    }
}
