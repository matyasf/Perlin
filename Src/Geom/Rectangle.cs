using System;

namespace Perlin.Geom
{
    /// <summary>
    /// This class describes a non-rotated 2D rectangle. 
    /// </summary>
    public class Rectangle
    {
        /// <summary>
        /// The top left X coordinate
        /// </summary>
        public float X;
        /// <summary>
        /// The top left Y coordinate
        /// </summary>
        public float Y;
        public float Width;
        public float Height;
        
        public Rectangle (float x = 0.0f, float y = 0.0f, float width = 0.0f, float height = 0.0f)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        
        /// <summary>
        /// The top left Y coordinate, same as Y 
        /// </summary>
        public float Top
        {
            get => Y;
            set => Y = value;
        }
        
        /// <summary>
        /// The Y coordinate of the bottom of this rectangle
        /// </summary>
        public float Bottom
        {
            get => Y + Height;
            set => Height = value - Y;
        }
        
        /// <summary>
        /// The left X coordinate
        /// </summary>
        public float Left
        {
            get => X;
            set => X = value;
        }
        
        /// <summary>
        /// The right X coordinate.
        /// </summary>
        public float Right
        {
            get => X + Width;
            set => Width = value - X;
        }

        public Point TopLeft
        {
            get => new Point(X, Y);
            set
            { 
                X = value.X;
                Y = value.Y;
            }
        }

        public Point BottomRight
        {
            get => new Point(X + Width, Y + Height);
            set
            { 
                Right = value.X;
                Bottom = value.Y;
            }
        }
        
        /// <summary>
        /// The Width and Height of this rectangle.
        /// </summary>
        public Point Size
        {
            get => new Point(Width, Height);
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }
        
        /// <summary>
        /// Returns true if the given point is inside the rectangle.
        /// </summary>
        public bool Contains(float x, float y)
        {
            return x >= X && y >= Y && x <= X + Width && y <= Y + Height;
        }

        /// <summary>
        /// Returns true if the given point is inside the rectangle.
        /// </summary>
        public bool Contains(Point point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>
        /// Returns true if the given Rectangle is fully contained within this Rectangle.
        /// </summary>
        public bool Contains(Rectangle rectangle)
        {
            if (rectangle == null)
            {
                return false;
            }
            return rectangle.X >= X && rectangle.Right <= Right &&
                   rectangle.Y >= Y && rectangle.Bottom <= Bottom;
        }
        
        /// <summary>
        /// Returns true if the this and the other rectangle overlap.
        /// </summary>
        public bool Intersects(Rectangle other)
        {
            if (other == null || other.IsEmpty() || IsEmpty())
            {
                return false;
            }
            var left = Math.Max(X, other.X);
            var right = Math.Min(X + Width, other.X + other.Width);
            var top = Math.Max(Y, other.Y);
            var bottom = Math.Min(Y + Height, other.Y + other.Height);
            if (left > right || top > bottom)
            {
                return false;
            }
            return true;
        }

        public void SetTo(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
        /// <summary>
        /// Returns the intersecting rectangle. Returns a 0-sized rectangle if there is no intersection.
        /// </summary>
        public Rectangle Intersection(Rectangle rectangle)
        {
            if (rectangle == null)
            {
                return null;
            }
            var left = Math.Max(X, rectangle.X);
            var right = Math.Min(X + Width, rectangle.X + rectangle.Width);
            var top = Math.Max(Y, rectangle.Y);
            var bottom = Math.Min(Y + Height, rectangle.Y + rectangle.Height);
            if (left > right || top > bottom)
            {
                return new Rectangle();
            }
            return new Rectangle(left, top, right - left, bottom - top);
        }

        /// <summary>
        /// Returns a rectangle that encompasses both rectangles
        /// </summary>
        public Rectangle Union(Rectangle rectangle)
        {
            if (rectangle == null)
            {
                return null;
            }
            float left = Math.Max(X, rectangle.X);
            float right = Math.Min(X + Width, rectangle.X + rectangle.Width);
            float top = Math.Max(Y, rectangle.Y);
            float bottom = Math.Min(Y + Height, rectangle.Y + rectangle.Height);
            return new Rectangle(left, top, right - left, bottom - top);
        }
        
        /// <summary>
        /// Extends the bounds of the rectangle in all four directions.
        /// </summary>
        public void Inflate(float dx, float dy)
        {
            X -= dx;
            Y -= dy;
            Width += 2.0f * dx;
            Height += 2.0f * dy;
        }
        
        /// <summary>
        /// Extends the bounds of the rectangle in all four directions.
        /// </summary>
        public void Inflate(float left, float right, float top, float bottom)
        {
            X -= left;
            Y -= top;
            Width += left + right;
            Height += top + bottom;
        }

        public void Empty()
        {
            X = Y = Width = Height = 0.0f;
        }

        public void CopyFrom(Rectangle rectangle)
        {
            X = rectangle.X;
            Y = rectangle.Y;
            Width = rectangle.Width;
            Height = rectangle.Height;
        }

        /// <summary>
        /// Inverts X and Y if they are negative
        /// </summary>
        public void Normalize()
        {
            if (Width < 0.0f)
            {
                Width = -Width;
                X -= Width;
            }
            if (Height < 0.0f)
            {
                Height = -Height;
                Y -= Height;
            }
        }

        /// <summary>
        /// Returns true if Width or Height are 0
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return Width == 0.0f || Height == 0.0f;
        }

        /// <summary>
        /// Determines whether this instance is equal the specified other with a small Epsilon error margin.
        /// </summary>
        public bool IsEqual(Rectangle other)
        {
            if (other == this)
            {
                return true;
            } 
            if (other == null)
            {
                return false;
            }
            return MathUtil.IsAlmostEqual(X, other.X) &&
                   MathUtil.IsAlmostEqual(Y, other.Y) &&
                   MathUtil.IsAlmostEqual(Width, other.Width) &&
                   MathUtil.IsAlmostEqual(Height, other.Height);
        }

        /// <summary>
        /// Calculates the bounds of a rectangle after transforming it by a matrix.
        /// </summary>
        public Rectangle GetBounds(Matrix2D matrix)
        {
            Rectangle outRect = new Rectangle();

            float minX = float.MaxValue, maxX = -float.MaxValue;
            float minY = float.MaxValue, maxY = -float.MaxValue;
            Point[] positions = GetPositions();

            for (int i = 0; i < 4; ++i)
            {
                var sPoint = matrix.TransformPoint(positions[i]);

                if (minX > sPoint.X) minX = sPoint.X;
                if (maxX < sPoint.X) maxX = sPoint.X;
                if (minY > sPoint.Y) minY = sPoint.Y;
                if (maxY < sPoint.Y) maxY = sPoint.Y;
            }

            outRect.SetTo(minX, minY, maxX - minX, maxY - minY);
            return outRect;
        }

        /// <summary>
        /// Returns a vector containing the positions of the four edges of the given rectangle. 
        /// </summary>
        public Point[] GetPositions()
        {
            Point[] outP = new Point[4];

            for (int i = 0; i < 4; ++i)
            {
                outP[i] = new Point();
            }
            outP[0].X = Left; outP[0].Y = Top;
            outP[1].X = Right; outP[1].Y = Top;
            outP[2].X = Left;  outP[2].Y = Bottom;
            outP[3].X = Right; outP[3].Y = Bottom;
            return outP;
        }

        public Rectangle Clone()
        {
            return new Rectangle(X, Y, Width, Height);
        }

        public override string ToString()
        {
            return "[x=" + X + " y=" + Y + " width=" + Width + " height=" + Height + "]";
        }
        
    }
}