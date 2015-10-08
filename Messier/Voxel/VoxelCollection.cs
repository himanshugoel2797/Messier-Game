using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Voxel
{
    public struct VoxelProperties
    {
        public int Health;
        public bool visible;
        public bool collidable;
    }

    public class VoxelCollection
    {
        List<VoxelProperties> voxels;

        public VoxelCollection()
        {
            voxels = new List<VoxelProperties>();
        }

        public ushort Add(VoxelProperties props)
        {
            if (voxels.Count > ushort.MaxValue) throw new OutOfMemoryException("No more room for voxel types!");
            voxels.Add(props);
            return (ushort)(voxels.Count - 1);
        }

        public VoxelProperties this[ushort index]
        {
            get
            {
                return voxels[index];
            }
        }
    }
}
