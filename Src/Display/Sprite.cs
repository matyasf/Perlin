using Perlin.Geom;
using SixLabors.ImageSharp.PixelFormats;

namespace Perlin.Display
{
    /// <summary>
    /// A lightweight class to create images to and colored rectangles to show on the display.
    /// Note that you need to add the created instance to the Stage or one of its children (or its children..)
    /// for it (or its children) to be displayed!
    /// </summary>
    public class Sprite : DisplayObject
    {
        /// <summary>
        /// Creates a Sprite that displays the given image, its size will be the image's dimensions.
        /// Note that it uses the <code>ImageManager</code> class to load and store the image in the memory,
        /// the image will stay there even if this Sprite is removed from the Stage.
        /// </summary>
        /// <param name="imagePath">The path to the image.</param>
        /// <param name="mipmap">Whether to create mipmaps for the image. Images with mipmaps look better when
        /// scaled, but take up ~1.5x more GPU memory.</param>
        /// <param name="textureSubRegion">A region of the image that should be displayed. The rectangle must be
        /// inside the image's dimensions. If null the whole image is displayed.</param>
        public Sprite(string imagePath, bool mipmap = false, Rectangle textureSubRegion = null) : this()
        {
            LoadImage(imagePath, mipmap, textureSubRegion);
        }
        
        /// <summary>
        /// Creates a Sprite with the given color.
        /// </summary>
        /// <param name="width">Width of the Sprite</param>
        /// <param name="height">Height of the Sprite</param>
        /// <param name="color">the color + transparency (alpha) of the image.</param>
        public Sprite(float width, float height, Rgba32 color) : this()
        {
            OriginalWidth = width;
            OriginalHeight = height;
            ResSet = PerlinApp.ImageManager.CreateColoredTexture((uint)width, (uint)height, color);
        }
        
        /// <summary>
        /// Creates an empty Sprite. This will not render anything, you can use it to group other DisplayObjects
        /// together.
        /// </summary>
        public Sprite()
        {
            Name = "Sprite";
        }
        
        /// <summary>
        /// Loads the given image to the Sprite.
        /// </summary>
        /// <param name="path">The path to the image</param>
        /// <param name="mipmap">Whether to create mipmaps for the image. Images with mipmaps look better when
        /// scaled, but take up ~1.5x more GPU memory.</param>
        /// <param name="textureSubRegion">A region of the image that should be displayed. The rectangle must be
        /// inside the image's dimensions. If null the whole image is displayed.</param>
        public void LoadImage(string path, bool mipmap = false, Rectangle textureSubRegion = null)
        {
            var set = PerlinApp.ImageManager.Load(path, mipmap);
            ResSet = set.ret;
            if (textureSubRegion != null)
            {
                OriginalWidth = textureSubRegion.Width;
                OriginalHeight = textureSubRegion.Height;
                TextureSubRegionNormalized.X = textureSubRegion.X / set.texture.Width;
                TextureSubRegionNormalized.Y = textureSubRegion.Y / set.texture.Height;
                TextureSubRegionNormalized.Width = textureSubRegion.Width / set.texture.Width;
                TextureSubRegionNormalized.Height = textureSubRegion.Height / set.texture.Height;
            }
            else
            {
                OriginalWidth = set.texture.Width;
                OriginalHeight = set.texture.Height;
                TextureSubRegionNormalized = new Rectangle(0,0,1,1);
            }
        }
        
        public override string ToString()
        {
            return Name;
        }
    }
}

