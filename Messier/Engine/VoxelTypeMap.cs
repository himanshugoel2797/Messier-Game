using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public class VoxelTypeMap
    {
        public struct VoxelTypeData
        {
            public Vector4 Color;
            public bool Visible;
        }

        public Dictionary<int, VoxelTypeData> Voxels;

        public VoxelTypeMap()
        {
            Voxels = new Dictionary<int, VoxelTypeData>();
        }

        public VoxelTypeData this[int mat]
        {
            get
            {
                return Voxels[mat];
            }
            set
            {
                Voxels[mat] = value;
            }
        }
    }
}
