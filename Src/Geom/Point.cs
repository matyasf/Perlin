using System;

namespace Perlin.Geom
{
    /// <summary>
    /// The Point class describes a two dimensional point or vector.
    /// </summary>
    public class Point
    {

        public float X;
        public float Y;

        public Point(float x = 0, float y = 0)
        {
            X = x;
            Y = y;
        }
        
        /// <summary>
        /// The distance of this Point from the (0,0) coordinate.
        /// </summary>
        public float Length
        {
            get => (float)Math.Sqrt(X * X + Y * Y);
            set
            {
                X = X * value;
                Y = Y * value;
            }
        }
        
        /// <summary>
        /// Returns the angle of this point as a vector in degrees.
        /// </summary>
        public float Angle => (float)(Math.Atan2(Y, X) * 180 / Math.PI);
        
        public bool IsZero => X == 0.0f && Y == 0.0f;

        public void AddPoint(Point point)
        {
            X = X + point.X;
            Y = Y + point.Y;
        }

        public void SubtractPoint(Point point)
        {
            X = X - point.X;
            Y = Y - point.Y;
        }

        /// <summary>
        /// Rotates by the specified angle in degrees
        /// </summary>
        public void RotateBy(float angle)
        {
            var angleInRadians = angle * (float) Math.PI / 180f;
            var sin = MathUtil.FastSin(angleInRadians);
            var cos = MathUtil.FastCos(angleInRadians);
            X = X * cos - Y * sin;
            Y = X * sin + Y * cos;
        }
        
        /// <summary>
        /// Resizes the this point to have the length of 1. If its zero length it does nothing.
        /// </summary>
        public void Normalize()
        {
            if (IsZero)
            {
                return;
            }
            var inverseLength = 1 / Length;
            X = X * inverseLength;
            Y = Y * inverseLength;
        }

        /// <summary>
        /// Calculates the dot product of this Point and another.
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The dot product</returns>
        public float Dot(Point other)
        {
            return X * other.X + Y * other.Y;
        }

        public void CopyFromPoint(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public void SetTo(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Determines whether the specified Point's X and Y values is equal to the current Point with
        /// with a small epsilon error margin.
        /// </summary>
        public bool Equals(Point other)
        {
            if (other == this)
            {
                return true;
            }
            if (other == null)
            {
                return false;
            }
            return MathUtil.IsAlmostEqual(X, other.X) && MathUtil.IsAlmostEqual(Y, other.Y);
        }

        public float Distance(Point p2)
        {
            return (float)Math.Sqrt((X - p2.X) * (X - p2.X) + (Y - p2.Y) * (Y - p2.Y));
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }
}