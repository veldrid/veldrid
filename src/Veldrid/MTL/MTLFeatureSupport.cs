using System;
using System.Collections.Generic;
using Veldrid.MetalBindings;

namespace Veldrid.MTL
{
    internal class MTLFeatureSupport
    {
        private readonly HashSet<MTLFeatureSet> _supportedFeatureSets = new HashSet<MTLFeatureSet>();

        public bool IsMacOS { get; }

        public MTLFeatureSet MaxFeatureSet { get; }

        public MTLFeatureSupport(MTLDevice device)
        {
            foreach (MTLFeatureSet set in Enum.GetValues(typeof(MTLFeatureSet)))
            {
                if (device.supportsFeatureSet(set))
                {
                    _supportedFeatureSets.Add(set);
                    MaxFeatureSet = set;
                }
            }

            IsMacOS = IsSupported(MTLFeatureSet.macOS_GPUFamily1_v1)
                || IsSupported(MTLFeatureSet.macOS_GPUFamily1_v2)
                || IsSupported(MTLFeatureSet.macOS_GPUFamily1_v3);
        }

        public bool IsSupported(MTLFeatureSet featureSet) => _supportedFeatureSets.Contains(featureSet);

        internal bool IsDrawBaseVertexInstanceSupported()
        {
            return IsSupported(MTLFeatureSet.iOS_GPUFamily3_v1)
                || IsSupported(MTLFeatureSet.iOS_GPUFamily3_v2)
                || IsSupported(MTLFeatureSet.iOS_GPUFamily3_v3)
                || IsSupported(MTLFeatureSet.iOS_GPUFamily4_v1)
                || IsSupported(MTLFeatureSet.tvOS_GPUFamily2_v1)
                || IsMacOS;
        }
    }
}
