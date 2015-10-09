using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messier.Graphics;

namespace Messier.Voxel.Voxelizers
{
    public class GreedyVoxelizer : IVoxelizer
    {
        public VertexArray Voxelize(VoxelCollection collection, VoxelChunk chunk)
        {
            List<float> verts = new List<float>();

            for(int x = 0; x < VoxelChunk.Side; x++)
            {
                for(int y = 0; y < VoxelChunk.Side; y++)
                {
                    for(int z = 0; z < VoxelChunk.Side; z++)
                    {
                        

                    }
                }
            }


        }
    }
}
