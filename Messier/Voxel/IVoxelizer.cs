using Messier.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Voxel
{
    public interface IVoxelizer
    {
        VertexArray Voxelize(VoxelCollection collection, VoxelChunk chunk);
    }
}
