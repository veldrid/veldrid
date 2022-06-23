using System;
using System.Runtime.CompilerServices;

namespace Veldrid
{
    public static class BufferUsageExtensions
    {
        [SkipLocalsInit]
        public static string ToDisplayString(this BufferUsage usage)
        {
            const string separator = " | ";
            Span<char> buffer = stackalloc char[64];

            int offset = 0;
            string type = GetTypeFlagString(usage);
            type.CopyTo(buffer);
            offset += type.Length;

            string dynamic = GetDynamicFlagString(usage);
            if (dynamic.Length > 0)
            {
                if (offset != 0)
                {
                    separator.CopyTo(buffer.Slice(offset));
                    offset += separator.Length;
                }

                dynamic.CopyTo(buffer.Slice(offset));
                offset += dynamic.Length;
            }

            string staging = GetStagingFlagString(usage);
            if (staging.Length > 0)
            {
                if (offset != 0)
                {
                    separator.CopyTo(buffer.Slice(offset));
                    offset += separator.Length;
                }

                staging.CopyTo(buffer.Slice(offset));
                offset += staging.Length;
            }

            return buffer.Slice(0, offset).ToString();
        }

        public static string GetStagingFlagString(BufferUsage usage)
        {
            if ((usage & BufferUsage.StagingReadWrite) == BufferUsage.StagingReadWrite)
            {
                return "StagingRW";
            }
            else if ((usage & BufferUsage.StagingRead) != 0)
            {
                return "StagingRead";
            }
            else if ((usage & BufferUsage.StagingWrite) != 0)
            {
                return "StagingWrite";
            }
            return "";
        }

        public static string GetDynamicFlagString(BufferUsage usage)
        {
            if ((usage & BufferUsage.DynamicReadWrite) == BufferUsage.DynamicReadWrite)
            {
                return "DynamicRW";
            }
            else if ((usage & BufferUsage.DynamicRead) != 0)
            {
                return "DynamicRead";
            }
            else if ((usage & BufferUsage.DynamicWrite) != 0)
            {
                return "DynamicWrite";
            }
            return "";
        }

        public static string GetTypeFlagString(BufferUsage usage)
        {
            if ((usage & BufferUsage.VertexBuffer) != 0)
            {
                return "Vertex";
            }
            if ((usage & BufferUsage.IndexBuffer) != 0)
            {
                return "Index";
            }
            if ((usage & BufferUsage.UniformBuffer) != 0)
            {
                return "Uniform";
            }
            if ((usage & BufferUsage.StructuredBufferReadOnly) != 0)
            {
                return "Struct";
            }
            if ((usage & BufferUsage.StructuredBufferReadWrite) != 0)
            {
                return "StructRW";
            }
            if ((usage & BufferUsage.IndirectBuffer) != 0)
            {
                return "Indirect";
            }
            return "";
        }
    }
}
