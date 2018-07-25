using System;
using System.Runtime.CompilerServices;

namespace Veldrid
{
    /// <summary>
    /// Describes a single shader specialization constant. Used to substitute new values into Shaders when constructing a
    /// <see cref="Pipeline"/>.
    /// </summary>
    public struct SpecializationConstant : IEquatable<SpecializationConstant>
    {
        /// <summary>
        /// The constant variable ID, as defined in the <see cref="Shader"/>.
        /// </summary>
        public uint ID;
        /// <summary>
        /// The type of data stored in this instance. Must be a scalar numeric type.
        /// </summary>
        public ShaderConstantType Type;
        /// <summary>
        /// An 8-byte block storing the contents of the specialization value. This is treated as an untyped buffer and is
        /// interepreted according to <see cref="Type"/>.
        /// </summary>
        public ulong Data;

        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/>.
        /// </summary>
        /// <param name="id">The constant variable ID, as defined in the <see cref="Shader"/>.</param>
        /// <param name="type">The type of data stored in this instance. Must be a scalar numeric type.</param>
        /// <param name="data">An 8-byte block storing the contents of the specialization value. This is treated as an untyped
        /// buffer and is interepreted according to <see cref="Type"/>.</param>
        public SpecializationConstant(uint id, ShaderConstantType type, ulong data)
        {
            ID = id;
            Type = type;
            Data = data;
        }

        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a boolean.
        /// </summary>
        /// <param name="id">The constant variable ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, bool value) : this(id, ShaderConstantType.Bool, Store(value ? (byte)1u : (byte)0u)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 16-bit unsigned integer.
        /// </summary>
        /// <param name="id">The constant variable ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, ushort value) : this(id, ShaderConstantType.UInt16, Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 16-bit signed integer.
        /// </summary>
        /// <param name="id">The constant variable ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, short value) : this(id, ShaderConstantType.Int16, Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 32-bit unsigned integer.
        /// </summary>
        /// <param name="id">The constant variable ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, uint value) : this(id, ShaderConstantType.UInt32, Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 32-bit signed integer.
        /// </summary>
        /// <param name="id">The constant variable ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, int value) : this(id, ShaderConstantType.Int32, Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 64-bit unsigned integer.
        /// </summary>
        /// <param name="id">The constant variable ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, ulong value) : this(id, ShaderConstantType.UInt64, Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 64-bit signed integer.
        /// </summary>
        /// <param name="id">The constant variable ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, long value) : this(id, ShaderConstantType.Int64, Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 32-bit floating-point value.
        /// </summary>
        /// <param name="id">The constant variable ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, float value) : this(id, ShaderConstantType.Float, Store(value)) { }
        /// <summary>
        /// Constructs a new <see cref="SpecializationConstant"/> for a 64-bit floating-point value.
        /// </summary>
        /// <param name="id">The constant variable ID.</param>
        /// <param name="value">The constant value.</param>
        public SpecializationConstant(uint id, double value) : this(id, ShaderConstantType.Double, Store(value)) { }

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
            return ID.Equals(other.ID) && Type == other.Type && Data.Equals(other.Data);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return HashHelper.Combine(ID.GetHashCode(), (int)Type, Data.GetHashCode());
        }
    }
}
