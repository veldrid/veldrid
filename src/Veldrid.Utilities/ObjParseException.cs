using System;

namespace Veldrid.Utilities
{
    /// <summary>
    /// An parsing error for Wavefront OBJ files.
    /// </summary>
    public class ObjParseException : Exception
    {
        public ObjParseException(string message) : base(message)
        {
        }

        public ObjParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
