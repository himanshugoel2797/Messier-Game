using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public class BoundingBox
    {
        public Vector3 Min { get; set; }
        public Vector3 Max { get; set; }

        public bool Intersects(BoundingBox b)
        {
            if (b.Min.X < Max.X && b.Min.Y < Min.Y && b.Min.Z < Min.Z && b.Min.X > Min.X && b.Min.Y > Min.Y && b.Min.Z > Min.Z) return true;
            else if (b.Max.X > Min.X && b.Max.Y > Min.Y && b.Max.Z > Min.Z && b.Max.X < Max.X && b.Max.Y < Max.Y && b.Max.Z < Max.Z) return true;

            return false;
        }
    }
}
