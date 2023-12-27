@import Metal;

#import "metal_mono_workaround.h"

void copyFromBuffer(id<MTLBlitCommandEncoder> encoder,
                    id<MTLBuffer> sourceBuffer,
                    NSUInteger sourceOffset,
                    NSUInteger sourceBytesPerRow,
                    NSUInteger sourceBytesPerImage,
                    MTLSize sourceSize,
                    id<MTLTexture> destinationTexture,
                    NSUInteger destinationSlice,
                    NSUInteger destinationLevel,
                    NSUInteger destinationOriginX,
                    NSUInteger destinationOriginY,
                    NSUInteger destinationOriginZ)
{
    [encoder copyFromBuffer:sourceBuffer
               sourceOffset:sourceOffset
          sourceBytesPerRow:sourceBytesPerRow
        sourceBytesPerImage:sourceBytesPerImage
                 sourceSize:sourceSize
                  toTexture:destinationTexture
           destinationSlice:destinationSlice
           destinationLevel:destinationLevel
          destinationOrigin:MTLOriginMake(destinationOriginX, destinationOriginY, destinationOriginZ)];
}

void copyFromTexture(id<MTLBlitCommandEncoder> encoder,
                     id<MTLTexture> sourceTexture,
                     NSUInteger sourceSlice,
                     NSUInteger sourceLevel,
                     MTLOrigin sourceOrigin,
                     MTLSize sourceSize,
                     id<MTLTexture> destinationTexture,
                     NSUInteger destinationSlice,
                     NSUInteger destinationLevel,
                     NSUInteger destinationOriginX,
                     NSUInteger destinationOriginY,
                     NSUInteger destinationOriginZ)
{
    [encoder copyFromTexture:sourceTexture
                 sourceSlice:sourceSlice
                 sourceLevel:sourceLevel
                sourceOrigin:sourceOrigin
                  sourceSize:sourceSize
                   toTexture:destinationTexture
            destinationSlice:destinationSlice
            destinationLevel:destinationLevel
           destinationOrigin:MTLOriginMake(destinationOriginX, destinationOriginY, destinationOriginZ)];
}
