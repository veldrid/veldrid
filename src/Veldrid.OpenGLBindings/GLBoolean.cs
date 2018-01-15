using System;

namespace Veldrid.OpenGLBinding
{
    /// <summary>
    /// A boolean value stored in an unsigned byte.
    /// </summary>
    public struct GLboolean : IEquatable<GLboolean>
    {
        /// <summary>
        /// The raw value of the <see cref="GLboolean"/>. A value of 0 represents "false", all other values represent "true".
        /// </summary>
        public byte Value;

        /// <summary>
        /// Constructs a new <see cref="GLboolean"/> with the given raw value. 
        /// </summary>
        /// <param name="value"></param>
        public GLboolean(byte value)
        {
            Value = value;
        }

        /// <summary>
        /// Represents the boolean "true" value. Has a raw value of 1.
        /// </summary>
        public static readonly GLboolean True = new GLboolean(1);

        /// <summary>
        /// Represents the boolean "true" value. Has a raw value of 0.
        /// </summary>
        public static readonly GLboolean False = new GLboolean(0);

        /// <summary>
        /// Returns whether another <see cref="GLboolean"/> value is considered equal to this one.
        /// Two <see cref="GLboolean"/>s are considered equal when their raw values are equal.
        /// </summary>
        /// <param name="other">The value to compare to.</param>
        /// <returns>True if the other value's underlying raw value is equal to this instance's. False otherwise.</returns>
        public bool Equals(GLboolean other)
        {
            return Value.Equals(other.Value);
        }

        public override bool Equals(object obj)
        {
            return obj is GLboolean b && Equals(b);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{(this ? "True" : "False")} ({Value})";
        }

        public static implicit operator bool(GLboolean b) => b.Value != 0;
        public static implicit operator uint(GLboolean b) => b.Value;
        public static implicit operator GLboolean(bool b) => b ? True : False;
        public static implicit operator GLboolean(byte value) => new GLboolean(value);

        public static bool operator ==(GLboolean left, GLboolean right) => left.Value == right.Value;
        public static bool operator !=(GLboolean left, GLboolean right) => left.Value != right.Value;
    }
}
