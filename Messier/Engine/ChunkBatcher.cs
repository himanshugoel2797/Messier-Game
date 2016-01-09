using Messier.Graphics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public class ChunkBatcher
    {
        public int Side = 8;
        public int ChunkSide = 32;

        private Chunk[,] Chunks;
        private GPUBuffer chunkBuffer;
        private GPUBuffer chunkMatBuffer;
        private VertexArray vArray;


        public ChunkBatcher(int side, VoxelTypeMap VoxelTypes, Func<int, int, int, short> dataFiller)
        {
            this.Side = side;
            Chunks = new Chunk[Side, Side];

            chunkMatBuffer = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer, 4 * 1024 * 1024, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw);
            chunkBuffer = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer, 8 * 1024 * 1024, OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw);

            vArray = new VertexArray();
            vArray.SetBufferObject(0, chunkBuffer, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Int2101010Rev);
            vArray.SetBufferObject(1, chunkMatBuffer, 1, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Short);

            for (int i = 0; i < Side; i++)
                for (int j = 0; j < Side; j++)
                {
                    Chunks[i, j] = new Chunk();
                }

            ThreadPool.QueueUserWorkItem(new WaitCallback((a) =>
            {
                for (int i = 0; i < Side; i++)
                    for (int j = 0; j < Side; j++)
                    {
                        //Generate the mesh
                        Chunks[i, j].Side = Side;
                        Chunks[i, j].InitDataStore();
                        Chunks[i, j].VoxelMap = VoxelTypes;

                        for (int i0 = 0; i0 < Math.Min(Chunks[i, j].MaterialMap.Length, VoxelTypes.Voxels.Count); i0++)
                        {
                            Chunks[i, j].MaterialMap[i0] = i0;
                        }

                        for (int x = 0; x < Side; x++)
                            for (int y = 0; y < Side; y++)
                                for (int z = 0; z < Side; z++)
                                {

     
                                }
                        Chunks[i, j].GenerateMesh();
                    }
            }
                ));

        }

        public void SetupDrawCall(out Matrix4 World)
        {

        }
    }
}
