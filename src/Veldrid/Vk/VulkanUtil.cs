using System;
using System.Diagnostics;
using TerraFX.Interop.Vulkan;
using static TerraFX.Interop.Vulkan.Vulkan;
using static TerraFX.Interop.Vulkan.VkImageLayout;
using static TerraFX.Interop.Vulkan.VkAccessFlags;
using static TerraFX.Interop.Vulkan.VkPipelineStageFlags;

namespace Veldrid.Vulkan
{
    internal unsafe static class VulkanUtil
    {
        public static void CheckResult(VkResult result)
        {
            if (result != VkResult.VK_SUCCESS)
            {
                ThrowResult(result);
            }
        }

        public static void ThrowResult(VkResult result)
        {
            if (result == VkResult.VK_ERROR_OUT_OF_DEVICE_MEMORY ||
                result == VkResult.VK_ERROR_OUT_OF_HOST_MEMORY)
            {
                throw new VeldridOutOfMemoryException(GetExceptionMessage(result));
            }
            else
            {
                throw new VeldridException(GetExceptionMessage(result));
            }
        }

        private static string GetExceptionMessage(VkResult result)
        {
            return "Unsuccessful VkResult: " + result;
        }

        public static bool TryFindMemoryType(
            VkPhysicalDeviceMemoryProperties memProperties, uint typeFilter, VkMemoryPropertyFlags properties, out uint typeIndex)
        {
            for (uint i = 0; i < memProperties.memoryTypeCount; i++)
            {
                if (((typeFilter & (1u << (int)i)) != 0)
                    && (memProperties.memoryTypes[(int)i].propertyFlags & properties) == properties)
                {
                    typeIndex = i;
                    return true;
                }
            }

            typeIndex = 0;
            return false;
        }

        public static string[] EnumerateInstanceLayers()
        {
            uint propCount = 0;
            VkResult result = vkEnumerateInstanceLayerProperties(&propCount, null);
            CheckResult(result);
            if (propCount == 0)
            {
                return Array.Empty<string>();
            }

            VkLayerProperties[] props = new VkLayerProperties[propCount];
            string[] ret = new string[propCount];

            fixed (VkLayerProperties* propPtr = props)
            {
                vkEnumerateInstanceLayerProperties(&propCount, propPtr);

                for (int i = 0; i < propCount; i++)
                {
                    sbyte* layerNamePtr = propPtr[i].layerName;
                    ret[i] = Util.GetString(layerNamePtr);
                }
            }

            return ret;
        }

        public static string[] EnumerateInstanceExtensions()
        {
            uint propCount = 0;
            VkResult result = vkEnumerateInstanceExtensionProperties((sbyte*)null, &propCount, null);
            if (result != VkResult.VK_SUCCESS)
            {
                return Array.Empty<string>();
            }

            if (propCount == 0)
            {
                return Array.Empty<string>();
            }

            VkExtensionProperties[] props = new VkExtensionProperties[propCount];
            string[] ret = new string[propCount];

            fixed (VkExtensionProperties* propPtr = props)
            {
                vkEnumerateInstanceExtensionProperties((sbyte*)null, &propCount, propPtr);

                for (int i = 0; i < propCount; i++)
                {
                    sbyte* extensionNamePtr = propPtr[i].extensionName;
                    ret[i] = Util.GetString(extensionNamePtr);
                }
            }

            return ret;
        }

        public static IntPtr GetInstanceProcAddr(VkInstance instance, string name)
        {
            Span<byte> byteBuffer = stackalloc byte[1024];

            Util.GetNullTerminatedUtf8(name, ref byteBuffer);
            fixed (byte* utf8Ptr = byteBuffer)
            {
                return (IntPtr)vkGetInstanceProcAddr(instance, (sbyte*)utf8Ptr);
            }
        }

        public static void TransitionImageLayout(
            VkCommandBuffer cb,
            VkImage image,
            uint baseMipLevel,
            uint levelCount,
            uint baseArrayLayer,
            uint layerCount,
            VkImageAspectFlags aspectMask,
            VkImageLayout oldLayout,
            VkImageLayout newLayout)
        {
            Debug.Assert(oldLayout != newLayout);
            VkImageMemoryBarrier barrier = new()
            {
                sType = VkStructureType.VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER,
                oldLayout = oldLayout,
                newLayout = newLayout,
                srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                image = image,
                subresourceRange = new VkImageSubresourceRange()
                {
                    aspectMask = aspectMask,
                    baseMipLevel = baseMipLevel,
                    levelCount = levelCount,
                    baseArrayLayer = baseArrayLayer,
                    layerCount = layerCount
                }
            };

            VkPipelineStageFlags srcStageFlags = VK_PIPELINE_STAGE_NONE_KHR;
            VkPipelineStageFlags dstStageFlags = VK_PIPELINE_STAGE_NONE_KHR;

            if ((oldLayout == VK_IMAGE_LAYOUT_UNDEFINED || oldLayout == VK_IMAGE_LAYOUT_PREINITIALIZED) && newLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_NONE_KHR;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_PREINITIALIZED && newLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_NONE_KHR;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_PREINITIALIZED && newLayout == VK_IMAGE_LAYOUT_GENERAL)
            {
                barrier.srcAccessMask = VK_ACCESS_NONE_KHR;
                barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_PREINITIALIZED && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_NONE_KHR;
                barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_GENERAL && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
                barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_GENERAL)
            {
                barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
                barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT;
            }

            else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
                barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_LATE_FRAGMENT_TESTS_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_PRESENT_SRC_KHR)
            {
                barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_MEMORY_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_PRESENT_SRC_KHR)
            {
                barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_MEMORY_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_LATE_FRAGMENT_TESTS_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_GENERAL && newLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_SHADER_WRITE_BIT;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else if (oldLayout == VK_IMAGE_LAYOUT_PRESENT_SRC_KHR && newLayout == VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL)
            {
                barrier.srcAccessMask = VK_ACCESS_MEMORY_READ_BIT;
                barrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
                srcStageFlags = VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT;
                dstStageFlags = VK_PIPELINE_STAGE_TRANSFER_BIT;
            }
            else
            {
                Debug.Fail("Invalid image layout transition.");
            }

            vkCmdPipelineBarrier(
                cb,
                srcStageFlags,
                dstStageFlags,
                0,
                0, null,
                0, null,
                1, &barrier);
        }
    }

    internal unsafe static class VkPhysicalDeviceMemoryPropertiesEx
    {
        public static VkMemoryType GetMemoryType(this VkPhysicalDeviceMemoryProperties memoryProperties, uint index)
        {
            return memoryProperties.memoryTypes[(int)index];
        }
    }
}
