using System;

namespace AlemdarLabs.ColorPalette.MachineLearning.Helpers
{
    public struct Point
    {
        /// <summary>
        /// Creates a new vector with given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="z">The Z coordinate.</param>
        public Point(float x, float y, float z = 0f)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        /// <summary>
        /// Gets the length of the vector.
        /// </summary>
        public float Magnitude
        {
            get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        /// <summary>
        /// Gets the squared length of the vector.
        /// </summary>
        public float SqrMagnitude
        {
            get { return X * X + Y * Y + Z * Z; }
        }

        /// <summary>
        /// Gets the vector with a magnitude of 1.
        /// </summary>
        public Point Normalized
        {
            get
            {
                Point copy = this;
                copy.Normalize();
                return copy;
            }
        }

        /// <summary>
        /// Normalizes the vector with a magnitude of 1.
        /// </summary>
        public void Normalize()
        {
            float num = Magnitude;
            if (num > 1E-05f)
            {
                this /= num;
            }
            else
            {
                this = Point.Zero;
            }
        }

        /// <summary>
        /// Shorthand for writing Point(0, 0, 0).
        /// </summary>
        public static Point Zero = new Point(0f, 0f, 0f);

        /// <summary>
        /// Returns the distance between two points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The distance.</returns>
        public static float Distance(Point a, Point b)
        {
            Point vector = new Point(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            return (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
        }

        /// <summary>
        /// Determines whether the specified object as a <see cref="Point" /> is exactly equal to this instance.
        /// </summary>
        /// <remarks>
        /// Due to floating point inaccuracies, this might return false for vectors which are essentially (but not exactly) equal. Use the <see cref="op_Equality"/> to test two points for approximate equality.
        /// </remarks>
        /// <param name="other">The <see cref="Point" /> object to compare with this instance.</param>
        /// <returns><c>true</c> if the specified point is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object other)
        {
            bool result;
            if (!(other is Point))
            {
                result = false;
            }
            else
            {
                Point vector = (Point)other;
                result = (X.Equals(vector.X) && Y.Equals(vector.Y) && Z.Equals(vector.Z));
            }
            return result;
        }

        #region Operators

        /// <summary>
        /// Adds two vectors.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Subtracts one vector from another.
        /// </summary>
        /// <param name="a">The first vector.</param>
        /// <param name="b">The second vector.</param>
        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// Negates a vector.
        /// </summary>
        /// <param name="a">The vector.</param>
        public static Point operator -(Point a)
        {
            return new Point(-a.X, -a.Y, -a.Z);
        }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The number.</param>
        public static Point operator *(Point a, float d)
        {
            return new Point(a.X * d, a.Y * d, a.Z * d);
        }

        /// <summary>
        /// Multiplies a vector by a number.
        /// </summary>
        /// <param name="d">The number.</param>
        /// <param name="a">The vector.</param>
        public static Point operator *(float d, Point a)
        {
            return new Point(a.X * d, a.Y * d, a.Z * d);
        }

        /// <summary>
        /// Divides a vector by a number.
        /// </summary>
        /// <param name="a">The vector.</param>
        /// <param name="d">The number.</param>
        public static Point operator /(Point a, float d)
        {
            return new Point(a.X / d, a.Y / d, a.Z / d);
        }

        /// <summary>
        /// Determines whether two points are approximately equal.
        /// </summary>
        /// <remarks>
        /// To allow for floating point inaccuracies, the two vectors are considered equal if the magnitude of their difference is less than 1e-5..
        /// </remarks>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        public static bool operator ==(Point a, Point b)
        {
            return (a - b).SqrMagnitude < 9.99999944E-11f;
        }

        /// <summary>
        /// Determines whether two points are different.
        /// </summary>
        /// <remarks>
        /// To allow for floating point inaccuracies, the two vectors are considered equal if the magnitude of their difference is less than 1e-5.
        /// </remarks>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        public static bool operator !=(Point a, Point b)
        {
            return !(a == b);
        }

        #endregion Operators
    }
}
