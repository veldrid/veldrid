#import <Foundation/Foundation.h>

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
                    NSUInteger destinationOriginZ);

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
                     NSUInteger destinationOriginZ);
