using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Veldrid;

namespace SampleBase
{
    // A hand-crafted KTX file parser.
    // https://www.khronos.org/opengles/sdk/tools/KTX/file_format_spec
    public class KtxFile
    {
        public KtxHeader Header { get; }
        public KtxKeyValuePair[] KeyValuePairs { get; }
        public KtxMipmapLevel[] Mipmaps { get; }

        public KtxFile(KtxHeader header, KtxKeyValuePair[] keyValuePairs, KtxMipmapLevel[] mipmaps)
        {
            Header = header;
            KeyValuePairs = keyValuePairs;
            Mipmaps = mipmaps;
        }

        public static KtxFile Load(byte[] bytes, bool readKeyValuePairs)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return Load(ms, readKeyValuePairs);
            }
        }

        public static KtxFile Load(Stream s, bool readKeyValuePairs)
        {
            using (BinaryReader br = new BinaryReader(s))
            {
                KtxHeader header = ReadStruct<KtxHeader>(br);

                KtxKeyValuePair[] kvps = null;
                if (readKeyValuePairs)
                {
                    int keyValuePairBytesRead = 0;
                    List<KtxKeyValuePair> keyValuePairs = new List<KtxKeyValuePair>();
                    while (keyValuePairBytesRead < header.BytesOfKeyValueData)
                    {
                        int bytesRemaining = (int)(header.BytesOfKeyValueData - keyValuePairBytesRead);
                        KtxKeyValuePair kvp = ReadNextKeyValuePair(br, out int read);
                        keyValuePairBytesRead += read;
                        keyValuePairs.Add(kvp);
                    }

                    kvps = keyValuePairs.ToArray();
                }
                else
                {
                    br.BaseStream.Seek(header.BytesOfKeyValueData, SeekOrigin.Current); // Skip over header data.
                }

                uint numberOfMipmapLevels = Math.Max(1, header.NumberOfMipmapLevels);
                uint numberOfArrayElements = Math.Max(1, header.NumberOfArrayElements);
                uint numberOfFaces = Math.Max(1, header.NumberOfFaces);

                uint baseWidth = Math.Max(1, header.PixelWidth);
                uint baseHeight = Math.Max(1, header.PixelHeight);
                uint baseDepth = Math.Max(1, header.PixelDepth);

                KtxMipmapLevel[] images = new KtxMipmapLevel[numberOfMipmapLevels];
                for (int mip = 0; mip < numberOfMipmapLevels; mip++)
                {
                    uint mipWidth = Math.Max(1, baseWidth / (uint)(Math.Pow(2, mip)));
                    uint mipHeight = Math.Max(1, baseHeight / (uint)(Math.Pow(2, mip)));
                    uint mipDepth = Math.Max(1, baseDepth / (uint)(Math.Pow(2, mip)));

                    uint imageSize = br.ReadUInt32();
                    uint arrayElementSize = imageSize / numberOfArrayElements;
                    KtxArrayElement[] arrayElements = new KtxArrayElement[numberOfArrayElements];
                    for (int arr = 0; arr < numberOfArrayElements; arr++)
                    {
                        uint faceSize = arrayElementSize / numberOfFaces;
                        KtxFace[] faces = new KtxFace[numberOfFaces];
                        for (int face = 0; face < numberOfFaces; face++)
                        {
                            faces[face] = new KtxFace(br.ReadBytes((int)faceSize));
                        }

                        arrayElements[arr] = new KtxArrayElement(faces);
                    }

                    images[mip] = new KtxMipmapLevel(
                        mipWidth,
                        mipHeight,
                        mipDepth,
                        imageSize,
                        arrayElementSize,
                        arrayElements);

                    uint mipPaddingBytes = 3 - ((imageSize + 3) % 4);
                    br.BaseStream.Seek(mipPaddingBytes, SeekOrigin.Current);
                }

                return new KtxFile(header, kvps, images);
            }
        }

        private static unsafe KtxKeyValuePair ReadNextKeyValuePair(BinaryReader br, out int bytesRead)
        {
            uint keyAndValueByteSize = br.ReadUInt32();
            byte* keyAndValueBytes = stackalloc byte[(int)keyAndValueByteSize];
            ReadBytes(br, keyAndValueBytes, (int)keyAndValueByteSize);
            int paddingByteCount = (int)(3 - ((keyAndValueByteSize + 3) % 4));
            br.BaseStream.Seek(paddingByteCount, SeekOrigin.Current); // Skip padding bytes

            // Find the key's null terminator
            int i;
            for (i = 0; i < keyAndValueByteSize; i++)
            {
                if (keyAndValueBytes[i] == 0)
                {
                    break;
                }
                Debug.Assert(i != keyAndValueByteSize); // Fail
            }


            int keySize = i; // Don't include null terminator.
            string key = Encoding.UTF8.GetString(keyAndValueBytes, keySize);
            byte* valueStart = keyAndValueBytes + i + 1; // Skip null terminator
            int valueSize = (int)(keyAndValueByteSize - keySize - 1);
            byte[] value = new byte[valueSize];
            for (int v = 0; v < valueSize; v++)
            {
                value[v] = valueStart[v];
            }

            bytesRead = (int)(keyAndValueByteSize + paddingByteCount + sizeof(uint));
            return new KtxKeyValuePair(key, value);
        }

        private static unsafe T ReadStruct<T>(BinaryReader br)
        {
            int size = Unsafe.SizeOf<T>();
            byte* bytes = stackalloc byte[size];
            for (int i = 0; i < size; i++)
            {
                bytes[i] = br.ReadByte();
            }

            return Unsafe.Read<T>(bytes);
        }

        private static unsafe void ReadBytes(BinaryReader br, byte* destination, int count)
        {
            for (int i = 0; i < count; i++)
            {
                destination[i] = br.ReadByte();
            }
        }

        public static unsafe Texture LoadTexture(
            GraphicsDevice gd,
            ResourceFactory factory,
            byte[] bytes,
            PixelFormat format)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return LoadTexture(gd, factory, ms, format);
            }
        }


        public static unsafe Texture LoadTexture(
            GraphicsDevice gd,
            ResourceFactory factory,
            string assetPath,
            PixelFormat format)
        {
            using (FileStream fs = File.OpenRead(assetPath))
            {
                return LoadTexture(gd, factory, fs, format);
            }
        }

        public static unsafe Texture LoadTexture(
            GraphicsDevice gd,
            ResourceFactory factory,
            Stream assetStream,
            PixelFormat format)
        {
            KtxFile ktxTex2D = Load(assetStream, false);

            uint width = ktxTex2D.Header.PixelWidth;
            uint height = ktxTex2D.Header.PixelHeight;
            if (height == 0) height = width;

            uint arrayLayers = Math.Max(1, ktxTex2D.Header.NumberOfArrayElements);
            uint mipLevels = Math.Max(1, ktxTex2D.Header.NumberOfMipmapLevels);

            Texture ret = factory.CreateTexture(TextureDescription.Texture2D(
                width, height, mipLevels, arrayLayers,
                format, TextureUsage.Sampled));

            Texture stagingTex = factory.CreateTexture(TextureDescription.Texture2D(
                width, height, mipLevels, arrayLayers,
                format, TextureUsage.Staging));

            // Copy texture data into staging buffer
            for (uint level = 0; level < mipLevels; level++)
            {
                KtxMipmapLevel mipmap = ktxTex2D.Mipmaps[level];
                for (uint layer = 0; layer < arrayLayers; layer++)
                {
                    KtxArrayElement ktxLayer = mipmap.ArrayElements[layer];
                    Debug.Assert(ktxLayer.Faces.Length == 1);
                    byte[] pixelData = ktxLayer.Faces[0].Data;
                    fixed (byte* pixelDataPtr = &pixelData[0])
                    {
                        gd.UpdateTexture(stagingTex, (IntPtr)pixelDataPtr, (uint)pixelData.Length,
                            0, 0, 0, mipmap.Width, mipmap.Height, 1, level, layer);
                    }
                }
            }

            CommandList copyCL = factory.CreateCommandList();
            copyCL.Begin();
            for (uint level = 0; level < mipLevels; level++)
            {
                KtxMipmapLevel mipLevel = ktxTex2D.Mipmaps[level];
                for (uint layer = 0; layer < arrayLayers; layer++)
                {
                    copyCL.CopyTexture(
                        stagingTex, 0, 0, 0, level, layer,
                        ret, 0, 0, 0, level, layer,
                        mipLevel.Width, mipLevel.Height, mipLevel.Depth,
                        1);
                }
            }
            copyCL.End();
            gd.SubmitCommands(copyCL);

            gd.DisposeWhenIdle(copyCL);
            gd.DisposeWhenIdle(stagingTex);

            return ret;
        }
    }

    public class KtxKeyValuePair
    {
        public string Key { get; }
        public byte[] Value { get; }
        public KtxKeyValuePair(string key, byte[] value)
        {
            Key = key;
            Value = value;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct KtxHeader
    {
        public fixed byte Identifier[12];
        public readonly uint Endianness;
        public readonly uint GlType;
        public readonly uint GlTypeSize;
        public readonly uint GlFormat;
        public readonly uint GlInternalFormat;
        public readonly uint GlBaseInternalFormat;
        public readonly uint PixelWidth;
        private readonly uint _pixelHeight;
        public uint PixelHeight => Math.Max(1, _pixelHeight);
        public readonly uint PixelDepth;
        public readonly uint NumberOfArrayElements;
        public readonly uint NumberOfFaces;
        public readonly uint NumberOfMipmapLevels;
        public readonly uint BytesOfKeyValueData;
    }

    // for each mipmap_level in numberOfMipmapLevels
    public class KtxMipmapLevel
    {
        public KtxMipmapLevel(uint width, uint height, uint depth, uint totalSize, uint arraySliceSize, KtxArrayElement[] slices)
        {
            Width = width;
            Height = height;
            Depth = depth;
            TotalSize = totalSize;
            ArrayElementSize = arraySliceSize;
            ArrayElements = slices;
        }

        public uint Width { get; }
        public uint Height { get; }
        public uint Depth { get; }
        public uint TotalSize { get; }
        public uint ArrayElementSize { get; }
        public KtxArrayElement[] ArrayElements { get; }
    }

    // for each array_element in numberOfArrayElements
    public class KtxArrayElement
    {
        public KtxArrayElement(KtxFace[] faces)
        {
            Faces = faces;
        }

        public KtxFace[] Faces { get; }
    }

    // for each face in numberOfFaces
    public class KtxFace
    {
        public KtxFace(byte[] data)
        {
            Data = data;
        }

        public byte[] Data { get; }
    }
}
