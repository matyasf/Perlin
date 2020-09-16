using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Veldrid;
using Point = Perlin.Geom.Point;

namespace Perlin.Display
{
    /// <summary>
    /// TextField is used to render text. By default it will resize itself to the text it displays, you can
    /// disable this by setting <code>AutoSize</code> to false.
    /// Make sure its big enough for your text otherwise the text will not show!
    /// </summary>
    public class TextField : DisplayObject
    {
        private Font _font;
        private Image<Rgba32> _image;
        private TextureView _textureView;
        private string _text;
        private bool _needsTextureRedraw;
        private bool _needsTextureRecreate;
        private bool _autoSize;
        private Color _fontColor = Color.Black;
        private Rgba32 _backgroundColor = Color.FromRgba(255, 255, 255, 0);
        private HorizontalAlignment _horizontalAlign = HorizontalAlignment.Left;
        private VerticalAlignment _verticalAlign = VerticalAlignment.Top;

        private Texture Texture { get; set; }

        /// <summary>
        /// Creates a new TextField instance.
        /// </summary>
        /// <param name="font">The font to use. An example to load one:
        /// <code>
        /// var family = PerlinApp.Fonts.Install(Path.Combine("Assets", "Fonts", "Arial.ttf"));
        /// var font = family.CreateFont(28);
        /// </code>
        /// </param>
        /// <param name="text">The text to display.</param>
        /// <param name="autoSize">Whether to automatically set the width and height of this object to fit the text.
        /// If its set to false you need to define its Width and Height otherwise nothing will be displayed since
        /// the default Width and Height are 0.</param>
        public TextField(Font font, string text = null, bool autoSize = true)
        {
            _text = text;
            _font = font;
            AutoSize = autoSize;
            Name = "textField";
        }
        
        /// <summary>
        /// The text displayed in this TextField.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                {
                    return;
                }
                _text = value;
                _needsTextureRedraw = true;
                if (_autoSize)
                {
                    PerformAutoSize();
                }
            }
        }

        public new float Width
        {
            get => base.Width;
            set
            {
                OriginalWidth = value;
                _needsTextureRecreate = true;
            }
        }

        public new float Height
        {
            get => base.Height;
            set
            {
                OriginalHeight = value;
                _needsTextureRecreate = true;
            }
        }

        /// <summary>
        /// The textfield is resized automatically to fit the the text size if set to <code>true</code>.
        /// </summary>
        public bool AutoSize
        {
            get => _autoSize;
            set
            {
                if (_autoSize == value)
                {
                    return;
                }
                _autoSize = value;
                PerformAutoSize();
            }
        }

        public Font Font
        {
            get => _font;
            set
            {
                _needsTextureRedraw = true;
                _font = value;
            }
        }

        /// <summary>
        /// The color of the displayed font, default is <code>Color.Black</code>
        /// </summary>
        public Color FontColor
        {
            get => _fontColor;
            set
            {
                _needsTextureRedraw = true;
                _fontColor = value;
            }
        }

        /// <summary>
        /// The vertical alignment of the text inside this TextField. Default is VerticalAlignment.Top
        /// </summary>
        public VerticalAlignment VerticalAlign
        {
            get => _verticalAlign;
            set
            {
                _needsTextureRedraw = true;
                _verticalAlign = value;
            }
        }

        /// <summary>
        /// The horizontal alignment of the text inside of this TextField. Default is HorizontalAlignment.Left
        /// </summary>
        public HorizontalAlignment HorizontalAlign
        {
            get => _horizontalAlign;
            set
            {
                _needsTextureRedraw = true;
                _horizontalAlign = value;
            }
        }

        /// <summary>
        /// The background color for this TextField. Default is Rgba32(255, 255, 255, 0);
        /// </summary>
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _needsTextureRedraw = true;
                _backgroundColor = value;
            }
        }
        
        /// <summary>
        /// Sets the size of this TextField to the size of its text. This is called automatically at the latest
        /// before rendering, you usually do not need to call it.
        /// </summary>
        public void PerformAutoSize()
        {
            var size = MeasureText();
            Width = size.X;
            Height = size.Y;
        }

        private void RecreateTexture()
        {
            _needsTextureRecreate = false;
            Texture?.Dispose();
            _textureView?.Dispose();
            _image?.Dispose();
            ResSet?.Dispose();
            
            GraphicsDevice gd = PerlinApp.DefaultGraphicsDevice;
            Texture = gd.ResourceFactory.CreateTexture(
                TextureDescription.Texture2D((uint)Width, (uint)Height, 1, 1, 
                    PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled));
            _textureView = gd.ResourceFactory.CreateTextureView(Texture);
            ResSet = gd.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    PerlinApp.Pipeline.TexLayout,
                    _textureView,
                    gd.PointSampler));
            _image = new Image<Rgba32>((int)Width, (int)Height);
        }

        public override void Render(float elapsedTimeSecs)
        {
            if (!string.IsNullOrEmpty(_text) && Width > 0 && Height > 0 && Visible)
            {
                if (_needsTextureRecreate)
                {
                    RecreateTexture();
                    DrawText();
                }
                if (_needsTextureRedraw)
                {
                    DrawText();
                }
            }
            base.Render(elapsedTimeSecs);
        }
        
        /// <summary>
        /// Called when text changes. Draws the text into the internal texture.
        /// </summary>
        private unsafe void DrawText()
        {
            _needsTextureRedraw = false;
            if (Width == 0 || Height == 0)
            {
                return;
            }
            _image.TryGetSinglePixelSpan(out var span0);
            fixed (void* data = &MemoryMarshal.GetReference(span0))
            {
                Unsafe.InitBlock(data, 0, (uint)(_image.Width * _image.Height * 4));
            }
            try
            {
                _image.Mutate(ctx =>
                {
                    if (_backgroundColor.A != 0)
                    {
                        ctx.BackgroundColor(_backgroundColor);
                    }
                    /*
                    // alternate method: convert to vector and measure it. Not nice because each text might have a 
                    // different height
                    var style = new RendererOptions(_font);
                    // Renders the glyphs to a vector instead of directly to the image
                    IPathCollection glyphs = TextBuilder.GenerateGlyphs(_text, style);
                    var bounds = glyphs.Bounds;
                    glyphs = glyphs.Translate(-bounds.Location);
                    if (HorizontalAlign == HorizontalAlignment.Center)
                    {
                        glyphs = glyphs.Translate((Width - bounds.Width) * 0.5f, 0);
                    }
                    else if (HorizontalAlign == HorizontalAlignment.Right)
                    {
                        glyphs = glyphs.Translate((Width - bounds.Width), 0);
                    }
                    //... and for vertical
                    var gOpt = new ShapeGraphicsOptions();
                    ctx.Fill(gOpt, FontColor, glyphs);
                    */
                    var textLoc = new PointF(0, 0);
                    if (_verticalAlign == VerticalAlignment.Center)
                    {
                        textLoc.Y = _image.Height * 0.5f;
                    }
                    else if (_verticalAlign == VerticalAlignment.Bottom)
                    {
                        textLoc.Y = _image.Height;
                    }
                    // If auto sizing let it grow as big as the text, do not wrap
                    var wrapTextWidth = AutoSize ? 0 : _image.Width;
                    ctx.DrawText(
                        new TextGraphicsOptions
                        {
                            GraphicsOptions = new GraphicsOptions()
                            {
                                Antialias = true,
                            },
                            TextOptions = new TextOptions()
                            {
                                HorizontalAlignment = _horizontalAlign,
                                VerticalAlignment = _verticalAlign,
                                WrapTextWidth = wrapTextWidth
                            }
                        },
                        _text, _font, FontColor, textLoc);
                });
            }
            catch (ImageProcessingException ex)
            {
                // see https://github.com/SixLabors/ImageSharp.Drawing/issues/86
                Console.WriteLine($"Could not render text '{_text}'. Perlin cannot render text with missing glyphs " +
                                  $"from the font :(\n{ex}");
            }
            //_image.SaveAsPng("eerrff.png"); // to test
            _image.TryGetSinglePixelSpan(out var span);
            PerlinApp.DefaultGraphicsDevice.UpdateTexture(Texture, 
                span.ToArray(),
                0, 0, 0, Texture.Width, Texture.Height,
                1, 0, 0);
        }

        /// <summary>
        /// Returns the dimensions of purely the text.
        /// </summary>
        public Point MeasureText()
        {
            if (_text == null)
            {
                return new Point();
            }
            RendererOptions style = new RendererOptions(_font);
            // Method1: Renders the glyphs to a vector instead of directly to the image
            //IPathCollection glyphs = TextBuilder.GenerateGlyphs(_text, style);
            //return new Point((float)Math.Ceiling(glyphs.Bounds.Width), (float)Math.Ceiling(glyphs.Bounds.Height));
            var fontRectangle = TextMeasurer.MeasureBounds(_text, style);
            return new Point((float)Math.Ceiling(fontRectangle.Right), (float)Math.Ceiling(fontRectangle.Bottom));
        }

        public override string ToString()
        {
            if (_text != null && _text.Length > 10)
            {
                return "[TextField text:" + _text.Substring(0, 10) + "..]";   
            }
            return "[TextField text:" + _text + "]";
        }
    }
}