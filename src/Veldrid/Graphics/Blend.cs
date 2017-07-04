namespace Veldrid.Graphics
{
    /// <summary>
    /// A blend factor which controls blending behavior.
    /// </summary>
    public enum Blend : byte
    {
        Zero,
        One,
        SourceAlpha,
        InverseSourceAlpha,
        DestinationAlpha,
        InverseDestinationAlpha,
        SourceColor,
        InverseSourceColor,
        DestinationColor,
        InverseDestinationColor,
        BlendFactor,
        InverseBlendFactor
    }
}
