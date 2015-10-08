using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Math
{
    public class Vector3
    {
        #region Constants
        public static Vector3 Zero
        {
            get;
        } = new Vector3(0, 0, 0);

        public static Vector3 One
        {
            get;
        } = new Vector3(1, 1, 1);

        public static Vector3 UnitX
        {
            get;
        } = new Vector3(1, 0, 0);

        public static Vector3 UnitY
        {
            get;
        } = new Vector3(0, 1, 0);

        public static Vector3 UnitZ
        {
            get;
        } = new Vector3(0, 0, 1);
        #endregion

        #region Properties
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        #endregion

        #region Constructors
        public Vector3(float X, float Y, float Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public Vector3() : this(0, 0, 0) { }
        #endregion

        #region Operators
        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return (a.X == b.X) && (a.Y == b.Y) && (a.Z == b.Z);
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            return !(a == b);
        }

        public static bool operator <(Vector3 a, Vector3 b)
        {
            return (a.X < b.X) && (a.Y < b.Y) && (a.Z < b.Z);
        }

        public static bool operator >(Vector3 a, Vector3 b)
        {
            return (a.X > b.X) && (a.Y > b.Y) && (a.Z > b.Z);
        }

        public static Vector3 operator *(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        #endregion
    }
}
