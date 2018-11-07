using System;

namespace Veldrid
{
    public struct DeviceBufferRange : BindableResource, IEquatable<DeviceBufferRange>
    {
        public DeviceBuffer Buffer;
        public uint Offset;
        public uint SizeInBytes;

        public DeviceBufferRange(DeviceBuffer buffer, uint offset, uint sizeInBytes)
        {
            Buffer = buffer;
            Offset = offset;
            SizeInBytes = sizeInBytes;
        }

        public bool Equals(DeviceBufferRange other)
        {
            return Buffer == other.Buffer && Offset.Equals(other.Offset) && SizeInBytes.Equals(other.SizeInBytes);
        }

        public override int GetHashCode()
        {
            int bufferHash = Buffer?.GetHashCode() ?? 0;
            return HashHelper.Combine(bufferHash, Offset.GetHashCode(), SizeInBytes.GetHashCode());
        }
    }
}
