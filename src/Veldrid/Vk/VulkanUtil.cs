using System;
using System.Diagnostics;
using Vulkan;
using static Vulkan.VulkanNative;

namespace Veldrid.Vk
{
    internal unsafe static class VulkanUtil
    {
        [Conditional("DEBUG")]
        public static void CheckResult(VkResult result)
        {
            if (result != VkResult.Success)
            {
                throw new VeldridException("Unsuccessful VkResult: " + result);
            }
        }

        public static uint FindMemoryType(VkPhysicalDeviceMemoryProperties memProperties, uint typeFilter, VkMemoryPropertyFlags properties)
        {
            for (int i = 0; i < memProperties.memoryTypeCount; i++)
            {
                if (((typeFilter & (1 << i)) != 0)
                    && (memProperties.GetMemoryType((uint)i).propertyFlags & properties) == properties)
                {
                    return (uint)i;
                }
            }

            throw new VeldridException("No suitable memory type.");
        }

        public static void CreateImage(
            VkDevice device,
            VkPhysicalDeviceMemoryProperties physicalDeviceMemProperties,
            VkDeviceMemoryManager memoryManager,
            uint width,
            uint height,
            uint depth,
            uint arrayLayers,
            VkFormat format,
            VkImageTiling tiling,
            VkImageUsageFlags usage,
            VkMemoryPropertyFlags properties,
            out VkImage image,
            out VkMemoryBlock memory)
        {
            VkImageCreateInfo imageCI = VkImageCreateInfo.New();
            imageCI.imageType = VkImageType.Image2D;
            imageCI.extent.width = width;
            imageCI.extent.height = height;
            imageCI.extent.depth = depth;
            imageCI.mipLevels = 1;
            imageCI.arrayLayers = arrayLayers;
            imageCI.format = format;
            imageCI.tiling = tiling;
            imageCI.initialLayout = VkImageLayout.Preinitialized;
            imageCI.usage = usage;
            imageCI.sharingMode = VkSharingMode.Exclusive;
            imageCI.samples = VkSampleCountFlags.Count1;

            VkResult result = vkCreateImage(device, ref imageCI, null, out image);
            CheckResult(result);

            vkGetImageMemoryRequirements(device, image, out VkMemoryRequirements memRequirements);
            VkMemoryBlock memoryToken = memoryManager.Allocate(
                physicalDeviceMemProperties,
                memRequirements.memoryTypeBits,
                properties,
                false,
                memRequirements.size,
                memRequirements.alignment);
            memory = memoryToken;
            result = vkBindImageMemory(device, image, memory.DeviceMemory, memory.Offset);
            CheckResult(result);
        }

        public static string[] EnumerateInstanceLayers()
        {
            uint propCount = 0;
            VkResult result = vkEnumerateInstanceLayerProperties(ref propCount, null);
            CheckResult(result);
            if (propCount == 0)
            {
                return Array.Empty<string>();
            }

            VkLayerProperties[] props = new VkLayerProperties[propCount];
            vkEnumerateInstanceLayerProperties(ref propCount, ref props[0]);

            string[] ret = new string[propCount];
            for (int i = 0; i < propCount; i++)
            {
                fixed (byte* layerNamePtr = props[i].layerName)
                {
                    ret[i] = Util.GetString(layerNamePtr);
                }
            }

            return ret;
        }

        public static string[] EnumerateInstanceExtensions()
        {
            uint propCount = 0;
            VkResult result = vkEnumerateInstanceExtensionProperties((byte*)null, ref propCount, null);
            CheckResult(result);
            if (propCount == 0)
            {
                return Array.Empty<string>();
            }

            VkExtensionProperties[] props = new VkExtensionProperties[propCount];
            vkEnumerateInstanceExtensionProperties((byte*)null, ref propCount, ref props[0]);

            string[] ret = new string[propCount];
            for (int i = 0; i < propCount; i++)
            {
                fixed (byte* extensionNamePtr = props[i].extensionName)
                {
                    ret[i] = Util.GetString(extensionNamePtr);
                }
            }

            return ret;
        }

        public static void TransitionImageLayout(
            VkCommandBuffer cb,
            VkImage image,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount,
            VkImageLayout oldLayout,
            VkImageLayout newLayout)
        {
            Debug.Assert(oldLayout != newLayout);
            VkImageMemoryBarrier barrier = VkImageMemoryBarrier.New();
            barrier.oldLayout = oldLayout;
            barrier.newLayout = newLayout;
            barrier.srcQueueFamilyIndex = QueueFamilyIgnored;
            barrier.dstQueueFamilyIndex = QueueFamilyIgnored;
            barrier.image = image;
            barrier.subresourceRange.aspectMask = VkImageAspectFlags.Color;
            barrier.subresourceRange.baseMipLevel = baseMipLevel;
            barrier.subresourceRange.levelCount = levelCount;
            barrier.subresourceRange.baseArrayLayer = baseArrayLayer;
            barrier.subresourceRange.layerCount = layerCount;

            VkPipelineStageFlags srcStageFlags = VkPipelineStageFlags.None;
            VkPipelineStageFlags dstStageFlags = VkPipelineStageFlags.None;

            if ((oldLayout == VkImageLayout.Undefined || oldLayout == VkImageLayout.Preinitialized) && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags.None;
                barrier.dstAccessMask = VkAccessFlags.TransferWrite;
                srcStageFlags = VkPipelineStageFlags.TopOfPipe;
                dstStageFlags = VkPipelineStageFlags.Transfer;
            }
            else if (oldLayout == VkImageLayout.ShaderReadOnlyOptimal && newLayout == VkImageLayout.TransferDstOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags.ShaderRead;
                barrier.dstAccessMask = VkAccessFlags.TransferWrite;
                srcStageFlags = VkPipelineStageFlags.FragmentShader;
                dstStageFlags = VkPipelineStageFlags.Transfer;
            }
            else if (oldLayout == VkImageLayout.Preinitialized && newLayout == VkImageLayout.TransferSrcOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags.None;
                barrier.dstAccessMask = VkAccessFlags.TransferRead;
                srcStageFlags = VkPipelineStageFlags.TopOfPipe;
                dstStageFlags = VkPipelineStageFlags.Transfer;
            }
            else if (oldLayout == VkImageLayout.TransferDstOptimal && newLayout == VkImageLayout.ShaderReadOnlyOptimal)
            {
                barrier.srcAccessMask = VkAccessFlags.TransferWrite;
                barrier.dstAccessMask = VkAccessFlags.ShaderRead;
                srcStageFlags = VkPipelineStageFlags.Transfer;
                dstStageFlags = VkPipelineStageFlags.FragmentShader;
            }
            else
            {
                Debug.Fail("Invalid image layout transition.");
            }

            vkCmdPipelineBarrier(
                cb,
                srcStageFlags,
                dstStageFlags,
                VkDependencyFlags.None,
                0, null,
                0, null,
                1, &barrier);
        }
    }

    internal unsafe static class VkPhysicalDeviceMemoryPropertiesEx
    {
        public static VkMemoryType GetMemoryType(this VkPhysicalDeviceMemoryProperties memoryProperties, uint index)
        {
            return (&memoryProperties.memoryTypes_0)[index];
        }
    }
}
