using System;

namespace Vd2.OpenGLBinding
{
    public enum DrawBufferMode
    {
        None = 0,
        NoneOes = 0,
        FrontLeft = 1024,
        FrontRight = 1025,
        BackLeft = 1026,
        BackRight = 1027,
        Front = 1028,
        Back = 1029,
        Left = 1030,
        Right = 1031,
        FrontAndBack = 1032,
        Aux0 = 1033,
        Aux1 = 1034,
        Aux2 = 1035,
        Aux3 = 1036,
        ColorAttachment0 = 36064,
        ColorAttachment1 = 36065,
        ColorAttachment2 = 36066,
        ColorAttachment3 = 36067,
        ColorAttachment4 = 36068,
        ColorAttachment5 = 36069,
        ColorAttachment6 = 36070,
        ColorAttachment7 = 36071,
        ColorAttachment8 = 36072,
        ColorAttachment9 = 36073,
        ColorAttachment10 = 36074,
        ColorAttachment11 = 36075,
        ColorAttachment12 = 36076,
        ColorAttachment13 = 36077,
        ColorAttachment14 = 36078,
        ColorAttachment15 = 36079
    }

    [Flags]
    public enum ClearBufferMask
    {
        None = 0,
        DepthBufferBit = 256,
        AccumBufferBit = 512,
        StencilBufferBit = 1024,
        ColorBufferBit = 16384,
        CoverageBufferBitNv = 32768
    }

    public enum PrimitiveType
    {
        Points = 0,
        Lines = 1,
        LineLoop = 2,
        LineStrip = 3,
        Triangles = 4,
        TriangleStrip = 5,
        TriangleFan = 6,
        Quads = 7,
        QuadsExt = 7,
        QuadStrip = 8,
        Polygon = 9,
        LinesAdjacency = 10,
        LinesAdjacencyArb = 10,
        LinesAdjacencyExt = 10,
        LineStripAdjacency = 11,
        LineStripAdjacencyArb = 11,
        LineStripAdjacencyExt = 11,
        TrianglesAdjacency = 12,
        TrianglesAdjacencyArb = 12,
        TrianglesAdjacencyExt = 12,
        TriangleStripAdjacency = 13,
        TriangleStripAdjacencyArb = 13,
        TriangleStripAdjacencyExt = 13,
        Patches = 14,
        PatchesExt = 14
    }

    public enum DrawElementsType
    {
        UnsignedByte = 5121,
        UnsignedShort = 5123,
        UnsignedInt = 5125
    }

    public enum TextureUnit
    {
        Texture0 = 33984,
        Texture1 = 33985,
        Texture2 = 33986,
        Texture3 = 33987,
        Texture4 = 33988,
        Texture5 = 33989,
        Texture6 = 33990,
        Texture7 = 33991,
        Texture8 = 33992,
        Texture9 = 33993,
        Texture10 = 33994,
        Texture11 = 33995,
        Texture12 = 33996,
        Texture13 = 33997,
        Texture14 = 33998,
        Texture15 = 33999,
        Texture16 = 34000,
        Texture17 = 34001,
        Texture18 = 34002,
        Texture19 = 34003,
        Texture20 = 34004,
        Texture21 = 34005,
        Texture22 = 34006,
        Texture23 = 34007,
        Texture24 = 34008,
        Texture25 = 34009,
        Texture26 = 34010,
        Texture27 = 34011,
        Texture28 = 34012,
        Texture29 = 34013,
        Texture30 = 34014,
        Texture31 = 34015
    }

    public enum FramebufferTarget
    {
        ReadFramebuffer = 36008,
        DrawFramebuffer = 36009,
        Framebuffer = 36160,
        FramebufferExt = 36160
    }

    public enum FramebufferAttachment
    {
        FrontLeft = 1024,
        FrontRight = 1025,
        BackLeft = 1026,
        BackRight = 1027,
        Aux0 = 1033,
        Aux1 = 1034,
        Aux2 = 1035,
        Aux3 = 1036,
        Color = 6144,
        Depth = 6145,
        Stencil = 6146,
        DepthStencilAttachment = 33306,
        ColorAttachment0 = 36064,
        ColorAttachment0Ext = 36064,
        ColorAttachment1 = 36065,
        ColorAttachment1Ext = 36065,
        ColorAttachment2 = 36066,
        ColorAttachment2Ext = 36066,
        ColorAttachment3 = 36067,
        ColorAttachment3Ext = 36067,
        ColorAttachment4 = 36068,
        ColorAttachment4Ext = 36068,
        ColorAttachment5 = 36069,
        ColorAttachment5Ext = 36069,
        ColorAttachment6 = 36070,
        ColorAttachment6Ext = 36070,
        ColorAttachment7 = 36071,
        ColorAttachment7Ext = 36071,
        ColorAttachment8 = 36072,
        ColorAttachment8Ext = 36072,
        ColorAttachment9 = 36073,
        ColorAttachment9Ext = 36073,
        ColorAttachment10 = 36074,
        ColorAttachment10Ext = 36074,
        ColorAttachment11 = 36075,
        ColorAttachment11Ext = 36075,
        ColorAttachment12 = 36076,
        ColorAttachment12Ext = 36076,
        ColorAttachment13 = 36077,
        ColorAttachment13Ext = 36077,
        ColorAttachment14 = 36078,
        ColorAttachment14Ext = 36078,
        ColorAttachment15 = 36079,
        ColorAttachment15Ext = 36079,
        DepthAttachment = 36096,
        DepthAttachmentExt = 36096,
        StencilAttachment = 36128,
        StencilAttachmentExt = 36128
    }

    public enum TextureTarget
    {
        Texture1D = 3552,
        Texture2D = 3553,
        ProxyTexture1D = 32867,
        ProxyTexture1DExt = 32867,
        ProxyTexture2D = 32868,
        ProxyTexture2DExt = 32868,
        Texture3D = 32879,
        Texture3DExt = 32879,
        Texture3DOes = 32879,
        ProxyTexture3D = 32880,
        ProxyTexture3DExt = 32880,
        DetailTexture2DSgis = 32917,
        Texture4DSgis = 33076,
        ProxyTexture4DSgis = 33077,
        TextureMinLod = 33082,
        TextureMinLodSgis = 33082,
        TextureMaxLod = 33083,
        TextureMaxLodSgis = 33083,
        TextureBaseLevel = 33084,
        TextureBaseLevelSgis = 33084,
        TextureMaxLevel = 33085,
        TextureMaxLevelSgis = 33085,
        TextureRectangle = 34037,
        TextureRectangleArb = 34037,
        TextureRectangleNv = 34037,
        ProxyTextureRectangle = 34039,
        TextureCubeMap = 34067,
        TextureBindingCubeMap = 34068,
        TextureCubeMapPositiveX = 34069,
        TextureCubeMapNegativeX = 34070,
        TextureCubeMapPositiveY = 34071,
        TextureCubeMapNegativeY = 34072,
        TextureCubeMapPositiveZ = 34073,
        TextureCubeMapNegativeZ = 34074,
        ProxyTextureCubeMap = 34075,
        Texture1DArray = 35864,
        ProxyTexture1DArray = 35865,
        Texture2DArray = 35866,
        ProxyTexture2DArray = 35867,
        TextureBuffer = 35882,
        TextureCubeMapArray = 36873,
        ProxyTextureCubeMapArray = 36875,
        Texture2DMultisample = 37120,
        ProxyTexture2DMultisample = 37121,
        Texture2DMultisampleArray = 37122,
        ProxyTexture2DMultisampleArray = 37123
    }

    public enum DrawBuffersEnum
    {
        None = 0,
        FrontLeft = 1024,
        FrontRight = 1025,
        BackLeft = 1026,
        BackRight = 1027,
        Aux0 = 1033,
        Aux1 = 1034,
        Aux2 = 1035,
        Aux3 = 1036,
        ColorAttachment0 = 36064,
        ColorAttachment1 = 36065,
        ColorAttachment2 = 36066,
        ColorAttachment3 = 36067,
        ColorAttachment4 = 36068,
        ColorAttachment5 = 36069,
        ColorAttachment6 = 36070,
        ColorAttachment7 = 36071,
        ColorAttachment8 = 36072,
        ColorAttachment9 = 36073,
        ColorAttachment10 = 36074,
        ColorAttachment11 = 36075,
        ColorAttachment12 = 36076,
        ColorAttachment13 = 36077,
        ColorAttachment14 = 36078,
        ColorAttachment15 = 36079
    }

    public enum FramebufferErrorCode
    {
        FramebufferUndefined = 33305,
        FramebufferComplete = 36053,
        FramebufferCompleteExt = 36053,
        FramebufferIncompleteAttachment = 36054,
        FramebufferIncompleteAttachmentExt = 36054,
        FramebufferIncompleteMissingAttachment = 36055,
        FramebufferIncompleteMissingAttachmentExt = 36055,
        FramebufferIncompleteDimensionsExt = 36057,
        FramebufferIncompleteFormatsExt = 36058,
        FramebufferIncompleteDrawBuffer = 36059,
        FramebufferIncompleteDrawBufferExt = 36059,
        FramebufferIncompleteReadBuffer = 36060,
        FramebufferIncompleteReadBufferExt = 36060,
        FramebufferUnsupported = 36061,
        FramebufferUnsupportedExt = 36061,
        FramebufferIncompleteMultisample = 36182,
        FramebufferIncompleteLayerTargets = 36264,
        FramebufferIncompleteLayerCount = 36265
    }

    public enum BufferTarget
    {
        ArrayBuffer = 34962,
        ElementArrayBuffer = 34963,
        PixelPackBuffer = 35051,
        PixelUnpackBuffer = 35052,
        UniformBuffer = 35345,
        TextureBuffer = 35882,
        TransformFeedbackBuffer = 35982,
        CopyReadBuffer = 36662,
        CopyWriteBuffer = 36663,
        DrawIndirectBuffer = 36671,
        ShaderStorageBuffer = 37074,
        DispatchIndirectBuffer = 37102,
        QueryBuffer = 37266,
        AtomicCounterBuffer = 37568
    }

    public enum GLPixelFormat
    {
        UnsignedShort = 5123,
        UnsignedInt = 5125,
        ColorIndex = 6400,
        StencilIndex = 6401,
        DepthComponent = 6402,
        Red = 6403,
        RedExt = 6403,
        Green = 6404,
        Blue = 6405,
        Alpha = 6406,
        Rgb = 6407,
        Rgba = 6408,
        Luminance = 6409,
        LuminanceAlpha = 6410,
        AbgrExt = 32768,
        CmykExt = 32780,
        CmykaExt = 32781,
        Bgr = 32992,
        Bgra = 32993,
        Ycrcb422Sgix = 33211,
        Ycrcb444Sgix = 33212,
        Rg = 33319,
        RgInteger = 33320,
        R5G6B5IccSgix = 33894,
        R5G6B5A8IccSgix = 33895,
        Alpha16IccSgix = 33896,
        Luminance16IccSgix = 33897,
        Luminance16Alpha8IccSgix = 33899,
        DepthStencil = 34041,
        RedInteger = 36244,
        GreenInteger = 36245,
        BlueInteger = 36246,
        AlphaInteger = 36247,
        RgbInteger = 36248,
        RgbaInteger = 36249,
        BgrInteger = 36250,
        BgraInteger = 36251
    }

    public enum GLPixelType
    {
        Byte = 5120,
        UnsignedByte = 5121,
        Short = 5122,
        UnsignedShort = 5123,
        Int = 5124,
        UnsignedInt = 5125,
        Float = 5126,
        HalfFloat = 5131,
        Bitmap = 6656,
        UnsignedByte332 = 32818,
        UnsignedByte332Ext = 32818,
        UnsignedShort4444 = 32819,
        UnsignedShort4444Ext = 32819,
        UnsignedShort5551 = 32820,
        UnsignedShort5551Ext = 32820,
        UnsignedInt8888 = 32821,
        UnsignedInt8888Ext = 32821,
        UnsignedInt1010102 = 32822,
        UnsignedInt1010102Ext = 32822,
        UnsignedByte233Reversed = 33634,
        UnsignedShort565 = 33635,
        UnsignedShort565Reversed = 33636,
        UnsignedShort4444Reversed = 33637,
        UnsignedShort1555Reversed = 33638,
        UnsignedInt8888Reversed = 33639,
        UnsignedInt2101010Reversed = 33640,
        UnsignedInt248 = 34042,
        UnsignedInt10F11F11FRev = 35899,
        UnsignedInt5999Rev = 35902,
        Float32UnsignedInt248Rev = 36269
    }

    public enum PixelStoreParameter
    {
        UnpackSwapBytes = 3312,
        UnpackLsbFirst = 3313,
        UnpackRowLength = 3314,
        UnpackRowLengthExt = 3314,
        UnpackSkipRows = 3315,
        UnpackSkipRowsExt = 3315,
        UnpackSkipPixels = 3316,
        UnpackSkipPixelsExt = 3316,
        UnpackAlignment = 3317,
        PackSwapBytes = 3328,
        PackLsbFirst = 3329,
        PackRowLength = 3330,
        PackSkipRows = 3331,
        PackSkipPixels = 3332,
        PackAlignment = 3333,
        PackSkipImages = 32875,
        PackSkipImagesExt = 32875,
        PackImageHeight = 32876,
        PackImageHeightExt = 32876,
        UnpackSkipImages = 32877,
        UnpackSkipImagesExt = 32877,
        UnpackImageHeight = 32878,
        UnpackImageHeightExt = 32878,
        PackSkipVolumesSgis = 33072,
        PackImageDepthSgis = 33073,
        UnpackSkipVolumesSgis = 33074,
        UnpackImageDepthSgis = 33075,
        PixelTileWidthSgix = 33088,
        PixelTileHeightSgix = 33089,
        PixelTileGridWidthSgix = 33090,
        PixelTileGridHeightSgix = 33091,
        PixelTileGridDepthSgix = 33092,
        PixelTileCacheSizeSgix = 33093,
        PackResampleSgix = 33836,
        UnpackResampleSgix = 33837,
        PackSubsampleRateSgix = 34208,
        UnpackSubsampleRateSgix = 34209,
        PackResampleOml = 35204,
        UnpackResampleOml = 35205,
        UnpackCompressedBlockWidth = 37159,
        UnpackCompressedBlockHeight = 37160,
        UnpackCompressedBlockDepth = 37161,
        UnpackCompressedBlockSize = 37162,
        PackCompressedBlockWidth = 37163,
        PackCompressedBlockHeight = 37164,
        PackCompressedBlockDepth = 37165,
        PackCompressedBlockSize = 37166
    }

    public enum ShaderType
    {
        FragmentShader = 35632,
        VertexShader = 35633,
        GeometryShader = 36313,
        GeometryShaderExt = 36313,
        TessEvaluationShader = 36487,
        TessControlShader = 36488,
        ComputeShader = 37305
    }

    public enum ShaderParameter
    {
        ShaderType = 35663,
        DeleteStatus = 35712,
        CompileStatus = 35713,
        InfoLogLength = 35716,
        ShaderSourceLength = 35720
    }

    public enum SamplerParameterName
    {
        TextureBorderColor = 4100,
        TextureMagFilter = 10240,
        TextureMinFilter = 10241,
        TextureWrapS = 10242,
        TextureWrapT = 10243,
        TextureWrapR = 32882,
        TextureMinLod = 33082,
        TextureMaxLod = 33083,
        TextureMaxAnisotropyExt = 34046,
        TextureLodBias = 34049,
        TextureCompareMode = 34892,
        TextureCompareFunc = 34893
    }

    public enum TextureWrapMode
    {
        Clamp = 10496,
        Repeat = 10497,
        ClampToBorder = 33069,
        ClampToBorderArb = 33069,
        ClampToBorderNv = 33069,
        ClampToBorderSgis = 33069,
        ClampToEdge = 33071,
        ClampToEdgeSgis = 33071,
        MirroredRepeat = 33648
    }

    public enum TextureMinFilter
    {
        Nearest = 9728,
        Linear = 9729,
        NearestMipmapNearest = 9984,
        LinearMipmapNearest = 9985,
        NearestMipmapLinear = 9986,
        LinearMipmapLinear = 9987,
        Filter4Sgis = 33094,
        LinearClipmapLinearSgix = 33136,
        PixelTexGenQCeilingSgix = 33156,
        PixelTexGenQRoundSgix = 33157,
        PixelTexGenQFloorSgix = 33158,
        NearestClipmapNearestSgix = 33869,
        NearestClipmapLinearSgix = 33870,
        LinearClipmapNearestSgix = 33871
    }

    public enum TextureMagFilter
    {
        Nearest = 9728,
        Linear = 9729,
        LinearDetailSgis = 32919,
        LinearDetailAlphaSgis = 32920,
        LinearDetailColorSgis = 32921,
        LinearSharpenSgis = 32941,
        LinearSharpenAlphaSgis = 32942,
        LinearSharpenColorSgis = 32943,
        Filter4Sgis = 33094,
        PixelTexGenQCeilingSgix = 33156,
        PixelTexGenQRoundSgix = 33157,
        PixelTexGenQFloorSgix = 33158
    }

    public enum TextureCompareMode
    {
        None = 0,
        CompareRefToTexture = 34894,
        CompareRToTexture = 34894
    }

    public enum DepthFunction
    {
        Never = 512,
        Less = 513,
        Equal = 514,
        Lequal = 515,
        Greater = 516,
        Notequal = 517,
        Gequal = 518,
        Always = 519
    }

    public enum BlendingFactorSrc
    {
        Zero = 0,
        One = 1,
        SrcColor = 768,
        OneMinusSrcColor = 769,
        SrcAlpha = 770,
        OneMinusSrcAlpha = 771,
        DstAlpha = 772,
        OneMinusDstAlpha = 773,
        DstColor = 774,
        OneMinusDstColor = 775,
        SrcAlphaSaturate = 776,
        ConstantColor = 32769,
        ConstantColorExt = 32769,
        OneMinusConstantColor = 32770,
        OneMinusConstantColorExt = 32770,
        ConstantAlpha = 32771,
        ConstantAlphaExt = 32771,
        OneMinusConstantAlpha = 32772,
        OneMinusConstantAlphaExt = 32772,
        Src1Alpha = 34185,
        Src1Color = 35065,
        OneMinusSrc1Color = 35066,
        OneMinusSrc1Alpha = 35067
    }

    public enum BlendingFactorDest
    {
        Zero = 0,
        One = 1,
        SrcColor = 768,
        OneMinusSrcColor = 769,
        SrcAlpha = 770,
        OneMinusSrcAlpha = 771,
        DstAlpha = 772,
        OneMinusDstAlpha = 773,
        DstColor = 774,
        OneMinusDstColor = 775,
        SrcAlphaSaturate = 776,
        ConstantColor = 32769,
        ConstantColorExt = 32769,
        OneMinusConstantColor = 32770,
        OneMinusConstantColorExt = 32770,
        ConstantAlpha = 32771,
        ConstantAlphaExt = 32771,
        OneMinusConstantAlpha = 32772,
        OneMinusConstantAlphaExt = 32772,
        Src1Alpha = 34185,
        Src1Color = 35065,
        OneMinusSrc1Color = 35066,
        OneMinusSrc1Alpha = 35067
    }

    public enum EnableCap
    {
        PointSmooth = 2832,
        LineSmooth = 2848,
        LineStipple = 2852,
        PolygonSmooth = 2881,
        PolygonStipple = 2882,
        CullFace = 2884,
        Lighting = 2896,
        ColorMaterial = 2903,
        Fog = 2912,
        DepthTest = 2929,
        StencilTest = 2960,
        Normalize = 2977,
        AlphaTest = 3008,
        Dither = 3024,
        Blend = 3042,
        IndexLogicOp = 3057,
        ColorLogicOp = 3058,
        ScissorTest = 3089,
        TextureGenS = 3168,
        TextureGenT = 3169,
        TextureGenR = 3170,
        TextureGenQ = 3171,
        AutoNormal = 3456,
        Map1Color4 = 3472,
        Map1Index = 3473,
        Map1Normal = 3474,
        Map1TextureCoord1 = 3475,
        Map1TextureCoord2 = 3476,
        Map1TextureCoord3 = 3477,
        Map1TextureCoord4 = 3478,
        Map1Vertex3 = 3479,
        Map1Vertex4 = 3480,
        Map2Color4 = 3504,
        Map2Index = 3505,
        Map2Normal = 3506,
        Map2TextureCoord1 = 3507,
        Map2TextureCoord2 = 3508,
        Map2TextureCoord3 = 3509,
        Map2TextureCoord4 = 3510,
        Map2Vertex3 = 3511,
        Map2Vertex4 = 3512,
        Texture1D = 3552,
        Texture2D = 3553,
        PolygonOffsetPoint = 10753,
        PolygonOffsetLine = 10754,
        ClipDistance0 = 12288,
        ClipPlane0 = 12288,
        ClipDistance1 = 12289,
        ClipPlane1 = 12289,
        ClipDistance2 = 12290,
        ClipPlane2 = 12290,
        ClipDistance3 = 12291,
        ClipPlane3 = 12291,
        ClipDistance4 = 12292,
        ClipPlane4 = 12292,
        ClipDistance5 = 12293,
        ClipPlane5 = 12293,
        ClipDistance6 = 12294,
        ClipDistance7 = 12295,
        Light0 = 16384,
        Light1 = 16385,
        Light2 = 16386,
        Light3 = 16387,
        Light4 = 16388,
        Light5 = 16389,
        Light6 = 16390,
        Light7 = 16391,
        Convolution1D = 32784,
        Convolution1DExt = 32784,
        Convolution2D = 32785,
        Convolution2DExt = 32785,
        Separable2D = 32786,
        Separable2DExt = 32786,
        Histogram = 32804,
        HistogramExt = 32804,
        MinmaxExt = 32814,
        PolygonOffsetFill = 32823,
        RescaleNormal = 32826,
        RescaleNormalExt = 32826,
        Texture3DExt = 32879,
        VertexArray = 32884,
        NormalArray = 32885,
        ColorArray = 32886,
        IndexArray = 32887,
        TextureCoordArray = 32888,
        EdgeFlagArray = 32889,
        InterlaceSgix = 32916,
        Multisample = 32925,
        MultisampleSgis = 32925,
        SampleAlphaToCoverage = 32926,
        SampleAlphaToMaskSgis = 32926,
        SampleAlphaToOne = 32927,
        SampleAlphaToOneSgis = 32927,
        SampleCoverage = 32928,
        SampleMaskSgis = 32928,
        TextureColorTableSgi = 32956,
        ColorTable = 32976,
        ColorTableSgi = 32976,
        PostConvolutionColorTable = 32977,
        PostConvolutionColorTableSgi = 32977,
        PostColorMatrixColorTable = 32978,
        PostColorMatrixColorTableSgi = 32978,
        Texture4DSgis = 33076,
        PixelTexGenSgix = 33081,
        SpriteSgix = 33096,
        ReferencePlaneSgix = 33149,
        IrInstrument1Sgix = 33151,
        CalligraphicFragmentSgix = 33155,
        FramezoomSgix = 33163,
        FogOffsetSgix = 33176,
        SharedTexturePaletteExt = 33275,
        DebugOutputSynchronous = 33346,
        AsyncHistogramSgix = 33580,
        PixelTextureSgis = 33619,
        AsyncTexImageSgix = 33628,
        AsyncDrawPixelsSgix = 33629,
        AsyncReadPixelsSgix = 33630,
        FragmentLightingSgix = 33792,
        FragmentColorMaterialSgix = 33793,
        FragmentLight0Sgix = 33804,
        FragmentLight1Sgix = 33805,
        FragmentLight2Sgix = 33806,
        FragmentLight3Sgix = 33807,
        FragmentLight4Sgix = 33808,
        FragmentLight5Sgix = 33809,
        FragmentLight6Sgix = 33810,
        FragmentLight7Sgix = 33811,
        FogCoordArray = 33879,
        ColorSum = 33880,
        SecondaryColorArray = 33886,
        TextureRectangle = 34037,
        TextureCubeMap = 34067,
        ProgramPointSize = 34370,
        VertexProgramPointSize = 34370,
        VertexProgramTwoSide = 34371,
        DepthClamp = 34383,
        TextureCubeMapSeamless = 34895,
        PointSprite = 34913,
        SampleShading = 35894,
        RasterizerDiscard = 35977,
        PrimitiveRestartFixedIndex = 36201,
        FramebufferSrgb = 36281,
        SampleMask = 36433,
        PrimitiveRestart = 36765,
        DebugOutput = 37600
    }

    public enum BlendEquationMode
    {
        FuncAdd = 32774,
        Min = 32775,
        Max = 32776,
        FuncSubtract = 32778,
        FuncReverseSubtract = 32779
    }

    public enum CullFaceMode
    {
        Front = 1028,
        Back = 1029,
        FrontAndBack = 1032
    }

    public enum MaterialFace
    {
        Front = 1028,
        Back = 1029,
        FrontAndBack = 1032
    }

    public enum PolygonMode
    {
        Point = 6912,
        Line = 6913,
        Fill = 6914
    }

    public enum GetProgramParameterName
    {
        ProgramBinaryRetrievableHint = 33367,
        ProgramSeparable = 33368,
        GeometryShaderInvocations = 34943,
        GeometryVerticesOut = 35094,
        GeometryInputType = 35095,
        GeometryOutputType = 35096,
        ActiveUniformBlockMaxNameLength = 35381,
        ActiveUniformBlocks = 35382,
        DeleteStatus = 35712,
        LinkStatus = 35714,
        ValidateStatus = 35715,
        InfoLogLength = 35716,
        AttachedShaders = 35717,
        ActiveUniforms = 35718,
        ActiveUniformMaxLength = 35719,
        ActiveAttributes = 35721,
        ActiveAttributeMaxLength = 35722,
        TransformFeedbackVaryingMaxLength = 35958,
        TransformFeedbackBufferMode = 35967,
        TransformFeedbackVaryings = 35971,
        TessControlOutputVertices = 36469,
        TessGenMode = 36470,
        TessGenSpacing = 36471,
        TessGenVertexOrder = 36472,
        TessGenPointMode = 36473,
        MaxComputeWorkGroupSize = 37311,
        ActiveAtomicCounterBuffers = 37593
    }

    public enum BufferRangeTarget
    {
        UniformBuffer = 35345,
        TransformFeedbackBuffer = 35982,
        ShaderStorageBuffer = 37074,
        AtomicCounterBuffer = 37568
    }

    public enum DebugSource
    {
        DebugSourceApi = 33350,
        DebugSourceWindowSystem = 33351,
        DebugSourceShaderCompiler = 33352,
        DebugSourceThirdParty = 33353,
        DebugSourceApplication = 33354,
        DebugSourceOther = 33355
    }

    public enum DebugType
    {
        DebugTypeError = 33356,
        DebugTypeDeprecatedBehavior = 33357,
        DebugTypeUndefinedBehavior = 33358,
        DebugTypePortability = 33359,
        DebugTypePerformance = 33360,
        DebugTypeOther = 33361,
        DebugTypeMarker = 33384,
        DebugTypePushGroup = 33385,
        DebugTypePopGroup = 33386
    }

    public enum DebugSeverity
    {
        DebugSeverityNotification = 33387,
        DebugSeverityHigh = 37190,
        DebugSeverityMedium = 37191,
        DebugSeverityLow = 37192
    }
}
