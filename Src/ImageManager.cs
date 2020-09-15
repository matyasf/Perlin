using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.ImageSharp;

namespace Perlin
{
    // TODO only allow disposing of GPU stuff here
    /// <summary>
    /// This class manages the loading, storing and retrieving of images used in your app.
    /// </summary>
    public class ImageManager
    {
        private readonly Dictionary<(string path, bool mipmap), (ResourceSet, Texture)> _loadedImages = 
            new Dictionary<(string path, bool mipmap), (ResourceSet, Texture)>();

        /// <summary>
        /// Loads an image from the disk and uploads it to the GPU. If called again with the same parameters,
        /// it will return the already stored image.
        /// </summary>
        /// <param name="imagePath">the path to the image</param>
        /// <param name="mipmap">Whether to create mipmaps for the image. Images with MipMaps look better when scaled
        /// but take up more GPU memory (around 1.5x more).</param>
        public (ResourceSet ret, Texture texture) Load(string imagePath, bool mipmap = false)
        {
            if (!_loadedImages.TryGetValue((imagePath, mipmap), out (ResourceSet, Texture) ret))
            {
                GraphicsDevice gd = PerlinApp.DefaultGraphicsDevice;
                var imTex = new ImageSharpTexture(imagePath, mipmap);
                //Image<Rgba32> im = Image.Load<Rgba32>(imagePath); // alternative way
                //ImageSharpTexture imTex = new ImageSharpTexture(im, mipmap);
                var tex = imTex.CreateDeviceTexture(gd, gd.ResourceFactory);
                var view = gd.ResourceFactory.CreateTextureView(tex);
                ResourceSet set = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                    PerlinApp.Pipeline.TexLayout,
                    view,
                    gd.PointSampler)); 
                ret = (set, tex);
                _loadedImages.Add((imagePath, mipmap), ret);
            }
            return ret;
        }

        /// <summary>
        /// Deletes the resource from the memory and the GPU. Does nothing if the resource does not exist.
        /// You should call Delete after removing every object that uses this image from the display list,
        /// if you try to use a deleted resource your code will throw an exception.
        /// </summary>
        /// <param name="imagePath">Should be the same path that you used with <code>Load()</code>.</param>
        /// <param name="mipmap">whether to delete the MipMap version.</param>
        public void Delete(string imagePath, bool mipmap)
        {
            if (_loadedImages.TryGetValue((imagePath, mipmap), out (ResourceSet resourceSet, Texture texture) ret))
            {
                ret.resourceSet.Dispose();
                ret.texture.Dispose();
                _loadedImages.Remove((imagePath, mipmap));
            }
        }

        /// <summary>
        /// Creates a texture with the given color.
        /// This is not optimal, because it will not be batchable with anything, you should
        /// rather create textures from parts of texture atlases.
        /// </summary>
        /// <param name="width">The width of the texture in pixels</param>
        /// <param name="height">The height of the texture in pixels</param>
        /// <param name="color">The color + transparency (alpha) of the texture</param>
        public unsafe ResourceSet CreateColoredTexture(uint width, uint height, Rgba32 color)
        {
            GraphicsDevice gd = PerlinApp.DefaultGraphicsDevice;
            var texture = gd.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D(width, height, 1, 1,
                    PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            var textureView = gd.ResourceFactory.CreateTextureView(texture);
            var resSet = gd.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    PerlinApp.Pipeline.TexLayout,
                    textureView,
                    gd.PointSampler));
            uint size = width * height;
            Rgba32[] arr = new Rgba32[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = color;
            }
            Span<Rgba32> span = new Span<Rgba32>(arr);
            fixed (void* data = &MemoryMarshal.GetReference(span))
            {
                PerlinApp.DefaultGraphicsDevice.UpdateTexture(
                    texture,
                    (IntPtr)data, size * 4, 0, 0, 0, 
                    width, height, 1, 0, 0);
            }
            return resSet;
        }
    }
}