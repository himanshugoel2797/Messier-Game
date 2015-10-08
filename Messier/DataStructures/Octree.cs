using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messier.Math;

namespace Messier.DataStructures
{
    public enum OctreeNode      //YZX
    {                           //TFR
        TopFrontLeft = 6,       //110
        TopFrontRight = 7,      //111

        TopBackLeft = 4,        //100
        TopBackRight = 5,       //101

        BottomFrontLeft = 2,    //010
        BottomFrontRight = 3,   //011

        BottomBackLeft = 0,     //000
        BottomBackRight = 1     //001
    }

    public class Octree<T>
    {
        public Octree<T> Parent { get; set; }
        public Octree<T>[] Children { get; set; }
        public T Value { get; set; }

        int maxLevels;

        const int ChildCount = 8;
        #region Private Helpers
        private static OctreeNode CalcPos(Vector3 vec)
        {
            int x = (vec.X > 0) ? 1 : 0;
            int y = (vec.Y > 0) ? 1 : 0;
            int z = (vec.Z > 0) ? 1 : 0;

            return (OctreeNode)((y << 2) | (z << 1) | x);    //Calculate the node position
        }
        #endregion


        public Octree(int maxLevels)
        {
            Parent = null;
            Children = new Octree<T>[ChildCount];
            this.maxLevels = maxLevels;
        }

        public Octree(T Value, int maxLevels) : this(maxLevels)
        {
            this.Value = Value;
        }

        public void Add(Vector3 vec, T Value)
        {
            //Assuming we are centered at (0,0,0) determine to the maximum level, where value should be placed
            if (vec != Vector3.Zero)
            {

            }
        }
    }
}
