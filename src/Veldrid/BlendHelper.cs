namespace Veldrid
{
    internal static class BlendHelper
    {
        /// <summary>
        /// Given a nullable <see cref="ColorWriteMask"/>, returns the mask as non-null, or <see cref="ColorWriteMask.All"/> if null.
        /// </summary>
        /// <param name="mask">A nullable mask.</param>
        /// <returns>The non-nullable mask.</returns>
        public static ColorWriteMask GetOrDefault(this ColorWriteMask? mask) => mask ?? ColorWriteMask.All;
    }
}
