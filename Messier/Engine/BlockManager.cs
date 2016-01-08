using Messier.Graphics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public class BlockManager
    {
        //Determines which blocks to draw, queus new blocks for generation etc
        Dictionary<Vector3, Chunk> chunks;
        OpenSimplexNoise s;

        public int Side { get; set; } = 32;
        public VoxelTypeMap VoxelTypes { get; set; }

        public BlockManager()
        {
            chunks = new Dictionary<Vector3, Chunk>();
            s = new OpenSimplexNoise(0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector3 CorrectVector(Vector3 a)
        {
            Vector3 p = new Vector3(a.X - (a.X % Side), a.Y - (a.Y % Side), a.Z - (a.Z % Side));
            p += Vector3.One * Side / 2;

            return p;
        }

        private Chunk GetChunk(Vector3 pos)
        {
            //if chunk does not exist, generate it from the noise function
            pos = CorrectVector(pos);

            if (!chunks.ContainsKey(pos))
            {
                //Generate the mesh
                chunks.Add(pos, new Chunk());
                chunks[pos].InitDataStore();
                chunks[pos].Side = Side;
                chunks[pos].VoxelMap = VoxelTypes;

                for (int i = 0; i < Math.Min(chunks[pos].MaterialMap.Length, VoxelTypes.Voxels.Count); i++)
                {
                    chunks[pos].MaterialMap[i] = i;
                }

                ThreadPool.QueueUserWorkItem(new WaitCallback((a) =>
                {
                    Vector3 p0 = pos - Vector3.One * Side / 2;

                    float n = Side * 4f;

                    for (int x = 0; x < Side; x++)
                        for (int y = 0; y < Side; y++)
                        {
                            int height = (int)(s.Evaluate((x + p0.X) / n, (y + p0.Z) / n) * Side);
                            height -= (int)p0.Y;

                            if (height >= Side) height = Side;
                            else if (height < 0) height = 0;

                            for (int z = 0; z < height; z++)
                            {
                                chunks[pos][x, z, y] = 1;
                                chunks[pos][x, z, y] = (byte)(s.Evaluate((x + p0.X) / n, (z + p0.Z) / n, (y + p0.Y) / n) <= 0.5 ? chunks[pos][x, z, y] : 0);
                            }
                        }
                    chunks[pos].GenerateMesh();
                }));
            }
            return chunks[pos];
        }

        public Chunk Draw(Vector3 EyePos, out Matrix4 pos)
        {
            //Attempt to access the chunk right underneath
            pos = Matrix4.CreateTranslation(CorrectVector(EyePos) - Vector3.One * Side / 2);
            return GetChunk(EyePos);


        }
    }
}
