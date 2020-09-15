using SixLabors.Fonts;
using SixLabors.ImageSharp;

namespace Perlin.Display
{
    /// <summary>
    /// A simple button
    /// </summary>
    public class Button : Sprite
    {
        private Sprite _upGraphic;
        private Sprite _hoverGraphic;
        private Sprite _downGraphic;
        private readonly TextField _label;

        /// <summary>
        /// Constructs a new button.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="autoSize">Whether to size the button based on the text displayed.</param>
        public Button(string text, bool autoSize = true)
        {
            Name = "buttonBase";
            var font = PerlinApp.FontRobotoMono.CreateFont(14);
            _label = new TextField(font, text, autoSize);
            _label.MouseOrTouchEnabled = false;
            
            UpGraphic = new Sprite(_label.Width + 10, _label.Height + 7, Color.Beige);
            HoverGraphic = new Sprite(_label.Width + 10, _label.Height + 7, Color.LightGray);
            DownGraphic = new Sprite(_label.Width + 10, _label.Height + 7, Color.DarkGray);

            _label.X = 5;
            _label.Y = 0;
            AddChild(_label);

            OriginalWidth = _label.Width + 10;
            OriginalHeight = _label.Height + 6;

            MouseEnter += (target, coords) =>
            {
                //Console.WriteLine("ENTER");
                _hoverGraphic.Visible = true;
            };
            MouseExit += (target, coords) =>
            {
                //Console.WriteLine("EXIT");
                _hoverGraphic.Visible = false;
                _downGraphic.Visible = false;
            };
            MouseDown += (target, coords, button) =>
            {
                //Console.WriteLine("DOWN");
                _downGraphic.Visible = true;
            };
            MouseUp += (target, coords, button) =>
            {
                //Console.WriteLine("UP");
                _downGraphic.Visible = false;
            };
        }

        /// <summary>
        /// The text displayed on the button. If this is autosized then the background graphics will be resized too.
        /// </summary>
        public string Text
        {
            set
            {
                _label.Text = value;
                ResizeGraphicsIfNeeded();
            }
            get => _label.Text;
        }

        /// <summary>
        /// The Font and its properties of the displayed text.
        /// </summary>
        public Font Font
        {
            set
            {
                _label.Font = value;
                ResizeGraphicsIfNeeded();
            }
            get => _label.Font;
        }

        private void ResizeGraphicsIfNeeded()
        {
            if (_label.AutoSize)
            {
                _upGraphic.WidthScaled = _label.Width;
                _upGraphic.HeightScaled = _label.Height;
                
                _hoverGraphic.WidthScaled = _label.Width;
                _hoverGraphic.HeightScaled = _label.Height;
                
                _downGraphic.WidthScaled = _label.Width;
                _downGraphic.HeightScaled = _label.Height;   
            }
        }

        /// <summary>
        /// The Sprite displayed if the button is not interacted in any way.
        /// </summary>
        public Sprite UpGraphic
        {
            get => _upGraphic;
            set
            {
                if (_upGraphic != null)
                {
                    RemoveChild(_upGraphic);
                }
                _upGraphic = value;
                _upGraphic.MouseOrTouchEnabled = false;
                AddChildAt(_upGraphic, 0);
            }
        }

        /// <summary>
        /// The Sprite displayed if mouse is hovered over this Button.
        /// </summary>
        public Sprite HoverGraphic
        {
            get => _hoverGraphic;
            set
            {
                if (_hoverGraphic != null)
                {
                    RemoveChild(_hoverGraphic);
                }
                _hoverGraphic = value;
                _hoverGraphic.Visible = false;
                _hoverGraphic.MouseOrTouchEnabled = false;
                AddChildAt(_hoverGraphic, 1);
            }
        }

        /// <summary>
        /// The Sprite displayed the mouse is held down over this button.
        /// </summary>
        public Sprite DownGraphic
        {
            get => _downGraphic;
            set
            {
                if (_downGraphic != null)
                {
                    RemoveChild(_downGraphic);
                } 
                _downGraphic = value;
                _downGraphic.MouseOrTouchEnabled = false;
                _downGraphic.Visible = false;
                AddChildAt(_downGraphic, 2);
            }
        }
    }
}