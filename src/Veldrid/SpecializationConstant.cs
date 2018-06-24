using System;
using System.Runtime.CompilerServices;

namespace Veldrid
{
    /// <summary>
    /// Describes a single SPIR-V Specialization Costant. Used to substitute new values into Shaders when constructing a
    /// <see cref="Pipeline"/>.
    /// </summary>
    public struct SpecializationConstant : IEquatable<SpecializationConstant>
    {
        /// <summary>
        /// The SPIR-V Specialization Constant ID.
        /// </summary>
        public uint ID;
        /// <summary>
        /// The size of data contained in this constant, expressed in bytes. The size of a Specialization Constant should match
        /// the size of the SPIR-V data type that is being specialized by this instance.
        /// </summary>
        public uint DataSize;
        /// <summary>
        /// An 8-byte block storing the contents of the specialization value. This is treated as an untyped buffer and is
        /// interepreted according to the data type specified in the SPIR-V shader.
        /// </summary>
        public ulong Data;


        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/>.
        /// </summary>
        /// <param name="id">The Specialization Constant ID.</param>
        /// <param name="dataSize">The size of data contained in this constant, expressed in bytes. Legal values are 2, 4, and 8
        /// bytes.</param>
        /// <param name="data">An 8-byte block storing the contents of the specialization value. This is treated as an untyped
        /// buffer and is  interepreted according to the data type specified in the SPIR-V shader.</param>
        public SpecializationConstant(uint id, uint dataSize, ulong data)
        {
            ID = id;
            DataSize = dataSize;
            Data = data;
        }

        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a boolean.
        /// </summary>
        /// <param name="id">The Specialization Constant ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, bool value) : this(id, sizeof(uint), Store(value ? 1u : 0u)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 32-bit unsigned integer.
        /// </summary>
        /// <param name="id">The Specialization Constant ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, uint value) : this(id, sizeof(uint), Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 32-bit signed integer.
        /// </summary>
        /// <param name="id">The Specialization Constant ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, int value) : this(id, sizeof(int), Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 64-bit unsigned integer.
        /// </summary>
        /// <param name="id">The Specialization Constant ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, ulong value) : this(id, sizeof(ulong), Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 64-bit signed integer.
        /// </summary>
        /// <param name="id">The Specialization Constant ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, long value) : this(id, sizeof(long), Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 32-bit floating-point value.
        /// </summary>
        /// <param name="id">The Specialization Constant ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, float value) : this(id, sizeof(float), Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 64-bit floating-point value.
        /// </summary>
        /// <param name="id">The Specialization Constant ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, double value) : this(id, sizeof(double), Store(value)) { }

        internal static unsafe ulong Store<T>(T value)
        {
            ulong ret;
            Unsafe.Write(&ret, value);
            return ret;
        }

        /// <summary>
        /// Element-wise equality.
        /// </summary>
        /// <param name="other">The instance to compare to.</param>
        /// <returns>True if all elements are equal; false otherswise.</returns>
        public bool Equals(SpecializationConstant other)
        {
            return ID.Equals(other.ID) && DataSize.Equals(other.DataSize) && Data.Equals(other.Data);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(ID.GetHashCode(), DataSize.GetHashCode(), Data.GetHashCode());
        }
    }
}
