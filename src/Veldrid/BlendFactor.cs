namespace Veldrid
{
    /// <summary>
    /// Controls the influence of components in a blend operation.
    /// </summary>
    public enum BlendFactor : byte
    {
        /// <summary>
        /// Each component is multiplied by 0.
        /// </summary>
        Zero,
        /// <summary>
        /// Each component is multiplied by 1.
        /// </summary>
        One,
        /// <summary>
        /// Each component is multiplied by the source alpha component.
        /// </summary>
        SourceAlpha,
        /// <summary>
        /// Each component is multiplied by (1 - source alpha).
        /// </summary>
        InverseSourceAlpha,
        /// <summary>
        /// Each component is multiplied by the destination alpha component.
        /// </summary>
        DestinationAlpha,
        /// <summary>
        /// Each component is multiplied by (1 - destination alpha).
        /// </summary>
        InverseDestinationAlpha,
        /// <summary>
        /// Each component is multiplied by the matching component of the source color.
        /// </summary>
        SourceColor,
        /// <summary>
        /// Each component is multiplied by (1 - the matching component of the source color).
        /// </summary>
        InverseSourceColor,
        /// <summary>
        /// Each component is multiplied by the matching component of the destination color.
        /// </summary>
        DestinationColor,
        /// <summary>
        /// Each component is multiplied by (1 - the matching component of the destination color).
        /// </summary>
        InverseDestinationColor,
        /// <summary>
        /// Each component is multiplied by the matching component in constant factor specified in <see cref="BlendStateDescription.BlendFactor"/>.
        /// </summary>
        BlendFactor,
        /// <summary>
        /// Each component is multiplied by (1 - the matching component in constant factor specified in <see cref="BlendStateDescription.BlendFactor"/>).
        /// </summary>
        InverseBlendFactor,
    }
}
