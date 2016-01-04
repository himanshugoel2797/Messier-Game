using Messier.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public enum VoxelDataType
    {
        Chunk = 1,
        OctreeNode = 2
    }

    public interface IVoxelData
    {
        VoxelDataType datType { get; set; }
    }

    public class Chunk : IVoxelData
    {
        public const int Side = 32;
        public const int BitPerVoxel = 8;
        public const int VoxelsPerInt = 32 / BitPerVoxel;

        public VoxelDataType datType { get; set; } = VoxelDataType.Chunk;

        public uint[,,] Data;

        public int[] MaterialMap;
        public VoxelTypeMap VoxelMap;

        public void InitDataStore()
        {
            Data = new uint[Side, Side, Side / VoxelsPerInt];
            MaterialMap = new int[1 << (BitPerVoxel - 1)];
        }

        public byte this[int x, int y, int z]
        {
            get
            {
                if (x < 0 | x >= Side | y < 0 | y >= Side | z < 0 | z >= Side) return 0;
                int z0 = z % VoxelsPerInt;
                uint num = Data[x, y, z / VoxelsPerInt];
                return (byte)((num >> (z0 * BitPerVoxel)) & 0xF);
            }
            set
            {
                Data[x, y, z / VoxelsPerInt] = (uint)(Data[x, y, z / VoxelsPerInt] & ~(0xF << (z % VoxelsPerInt) * BitPerVoxel)) | ((uint)(value & 0xF) << (z % VoxelsPerInt) * BitPerVoxel);
            }
        }

        public void GenerateMesh(GPUBuffer b)
        {
            List<float> verts = new List<float>();
            List<int> Materials = new List<int>();

            int baseX = -Side / 2;
            int baseY = -Side / 2;
            int baseZ = -Side / 2;


            for (int x = 0; x < Side; x++)
            {
                for (int y = 0; y < Side; y++)
                {
                    for (int z = 0; z < Side; z++)
                    {

                        #region Naive Mesher
                        int x0 = baseX + x;
                        int y0 = baseY + y;
                        int z0 = baseZ + z;

                        if (!VoxelMap[MaterialMap[this[x, y, z]]].Visible) continue;

                        if (z == 0 | !VoxelMap[MaterialMap[this[x, y, z - 1]]].Visible)
                        {
                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0 + 1);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);
                        }

                        if (z == Side - 1 | !VoxelMap[MaterialMap[this[x, y, z + 1]]].Visible)
                        {
                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);
                        }

                        if (y == 0 | !VoxelMap[MaterialMap[this[x, y - 1, z]]].Visible)
                        {
                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);
                        }

                        if (y == Side - 1 | !VoxelMap[MaterialMap[this[x, y + 1, z]]].Visible)
                        {
                            verts.Add(x0);
                            verts.Add(y0 + 1);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0 + 1);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);
                        }

                        if (x == 0 | !VoxelMap[MaterialMap[this[x - 1, y, z]]].Visible)
                        {
                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0 + 1);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);
                        }

                        if (x == Side - 1 | !VoxelMap[MaterialMap[this[x + 1, y, z]]].Visible)
                        {
                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);
                        }
                        #endregion
                    }
                }
            }

            b.BufferData(0, verts.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
        }
    }

    public class SVO : IVoxelData
    {
        public VoxelDataType datType
        {
            get; set;
        } = VoxelDataType.OctreeNode;

        public IVoxelData[] Top;
        public IVoxelData[] Bottom;
    }
}
