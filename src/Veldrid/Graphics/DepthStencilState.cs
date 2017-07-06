using System;

namespace Veldrid.Graphics
{
    /// <summary>
    /// Describes how depth comparisons are performed in the output merger.
    /// </summary>
    public interface DepthStencilState : IDisposable
    {
        /// <summary>
        /// Returns whether or not depth testing is enabled.
        /// </summary>
        bool IsDepthEnabled { get; }

        /// <summary>
        /// Returns whether depth writing is enabled.
        /// </summary>
        bool IsDepthWriteEnabled { get; }
        
        /// <summary>
        /// Returns what kind of comparison function is used to determine if a pixel
        /// passes the output merger's depth test, assuming depth-testing is enabled.
        /// </summary>
        DepthComparison DepthComparison { get; }
    }
}
