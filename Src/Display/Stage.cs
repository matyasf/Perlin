using System;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Point = Perlin.Geom.Point;

namespace Perlin.Display
{
    /// <summary>
    /// Stage is the root of the display tree. If you want to show something you have to add it to the Stage
    /// (via <code>AddChild()</code> or to one of its descendants.)
    /// </summary>
    public class Stage : DisplayObject
    {
        /// <summary>
        /// Stage cannot be rotated, this will throw an exception
        /// </summary>
        public override float Rotation { set => throw new ArgumentException("The Stage cannot be rotated."); }
        
        /// <summary>
        /// Background color of the Stage.
        /// </summary>
        public Rgb24 BackgroundColor = new Rgb24(255,255,255);
        
        /// <summary>
        /// Sets the X coordinate of the window. Does not do anything on Android/iOS.
        /// </summary>
        public override float X
        {
            set => PerlinApp.Window.X = (int)value;
            get => PerlinApp.Window.X;
        }

        /// <summary>
        /// Sets the Y coordinate of the window. Does not do anything on Android/iOS.
        /// </summary>
        public override float Y
        {
            set => PerlinApp.Window.Y = (int)value;
            get => PerlinApp.Window.Y;
        }

        /// <summary>
        /// Stage has no parent. Trying to set it will throw an exception.
        /// </summary>
        public override DisplayObject Parent { internal set => throw new ArgumentException(); }
        
        
        internal Stage(int width, int height)
        {
            _isOnStage = true;
            OriginalWidth = width;
            OriginalHeight = height;
            Name = "Stage";
        }

        public override DisplayObject HitTest(Point p)
        {
            // if nothing else is hit, the stage returns itself as target
            DisplayObject target = base.HitTest(p);
            if (target == null)
            {
                target = this;
            }
            return target;
        }
        
        private DisplayObject _mouseDownTarget;
        private DisplayObject _mouseHoverTarget;

        internal void OnMouseMoveInternal(float x, float y)
        {
            var p = new Point(x, y);
            DisplayObject mouseDownTarget;
            DisplayObject currentObjectUnderMouse = HitTest(p);
            if (_mouseHoverTarget != currentObjectUnderMouse) // mouse hovered over a new object
            {
                _mouseHoverTarget?.DispatchMouseExit(p);
                currentObjectUnderMouse.DispatchMouseEnter(p);
                // Console.WriteLine("enter: " + currentObjectUnderMouse.Name + " exit: " + _mouseHoverTarget?.Name);
                _mouseHoverTarget = currentObjectUnderMouse;
            }
            currentObjectUnderMouse.DispatchMouseHover(p);
            if (_mouseDownTarget != null)
            {
                mouseDownTarget = _mouseDownTarget;
            }
            else
            {
                mouseDownTarget = currentObjectUnderMouse;
            }
            // Console.WriteLine("move x:" + x + " y:" + y + " " + currentObjectUnderMouse);
            mouseDownTarget.DispatchMouseMoved(p); // + send local coordinates too, but calculate on-demand
        }
        
        internal DisplayObject DispatchMouseDownInternal(MouseButton button, Vector2 mousePosition)
        {
            var p = new Point(mousePosition.X, mousePosition.Y);
            _mouseDownTarget = HitTest(p);
            _mouseDownTarget.DispatchMouseDown(button, p); // + send local coordinates too
            //Console.WriteLine("DOWN " + p + " " + target);
            return _mouseDownTarget;
        }
        
        internal DisplayObject DispatchMouseUpInternal(MouseButton button, Vector2 mousePosition)
        {
            var p = new Point(mousePosition.X, mousePosition.Y);
            var target = HitTest(p);
            target.DispatchMouseUp(button, p); // + send local coordinates too
            //Console.WriteLine("UP " + p + " " + target);
            _mouseDownTarget = null;
            return target;
        }
        
        /// <summary>
        /// Renders all its children recursively.
        /// </summary>>
        public override void Render(float elapsedTimeSecs)
        {
            InvokeEnterFrameEvent(elapsedTimeSecs);
            foreach (var child in Children)
            {
                child.Render(elapsedTimeSecs);
            }
        }

        public override string ToString()
        {
            return "Stage";
        }
    }
}