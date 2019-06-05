using System;

namespace Veldrid.SampleGallery
{
    public class SampleException : Exception
    {
        public SampleException() { }
        public SampleException(string message) : base(message) { }
        public SampleException(string message, Exception inner) : base(message, inner) { }
    }
}
