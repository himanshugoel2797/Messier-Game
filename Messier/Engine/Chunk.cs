using Messier.Graphics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        public int Side = 64;
        public int BitPerVoxel = 4;

        public float Scale
        {
            get;
        } = 1f;

        public int VoxelsPerInt
        {
            get
            {
                return 32 / BitPerVoxel;
            }
        }

        public Vector3[] Normals { get; } = new Vector3[]
        {
                new Vector3(0, 0, -1),
                new Vector3(0, -1, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0),
                new Vector3(1, 0, 0)
        };

        public int[] NormalOffsets { get; internal set; }
        public int[] NormalGroupSizes { get; internal set; }

        public VoxelDataType datType { get; set; } = VoxelDataType.Chunk;

        public uint[,,] Data;

        public int[] MaterialMap;
        public VoxelTypeMap VoxelMap;
        public bool ChunkReady
        {
            get;
            internal set;
        } = false;

        internal VertexArray vArray;
        internal GPUBuffer vBuf, vMat;

        public void InitDataStore()
        {
            Data = new uint[Side, Side, Side / VoxelsPerInt];
            MaterialMap = new int[(1 << (BitPerVoxel - 1)) + 1];
        }

        public byte this[int x, int y, int z]
        {
            get
            {
                if (x < 0 | x >= Side | y < 0 | y >= Side | z < 0 | z >= Side) return 0;
                int z0 = z % VoxelsPerInt;
                uint num = Data[x, y, z / VoxelsPerInt];
                return (byte)((num >> (z0 * BitPerVoxel)) & ((1 << BitPerVoxel) - 1));
            }
            set
            {
                Data[x, y, z / VoxelsPerInt] = (uint)(Data[x, y, z / VoxelsPerInt] & ~(((1 << BitPerVoxel) - 1) << (z % VoxelsPerInt) * BitPerVoxel)) | ((uint)(value & ((1 << BitPerVoxel) - 1)) << (z % VoxelsPerInt) * BitPerVoxel);
            }
        }

        public byte this[Vector3 a]
        {
            get
            {
                return this[(int)a.X, (int)a.Y, (int)a.Z];
            }
            set
            {
                this[(int)a.X, (int)a.Y, (int)a.Z] = value;
            }
        }

        private int curMaterial = 0;
        private bool data(int x, int y, int z)
        {
            return this[x, y, z] == curMaterial && VoxelMap[MaterialMap[this[x, y, z]]].Visible
                && (!VoxelMap[MaterialMap[this[x + 1, y, z]]].Visible | !VoxelMap[MaterialMap[this[x - 1, y, z]]].Visible
                | !VoxelMap[MaterialMap[this[x, y + 1, z]]].Visible | !VoxelMap[MaterialMap[this[x, y - 1, z]]].Visible
                | !VoxelMap[MaterialMap[this[x, y, z + 1]]].Visible | !VoxelMap[MaterialMap[this[x, y, z - 1]]].Visible);
        }

        private byte GetMat(int a0, int a1, int a2, int u, int v)
        {
            Vector3 a = Vector3.Zero;
            a[u] = a0;
            a[v] = a1;
            a[(v + 1) % 3] = a2;

            return this[a];
        }

        private int packVerts(float x, float y, float z, int uv)
        {
            byte x0 = (byte)((int)x & ((1 << 10) - 1));
            byte y0 = (byte)((int)y & ((1 << 10) - 1));
            byte z0 = (byte)((int)z & ((1 << 10) - 1));

            int packed = x0 | (y0 << 10) | (z0 << 20) | (uv & 3) << 30;

            return packed;
        }

        public void GenerateMesh()
        {
            #region Naive Mesher
            /*
            List<int>[] nverts = new List<int>[6];
            List<short>[] nMaterials = new List<short>[6];
            for (int i = 0; i < 6; i++)
            {
                nverts[i] = new List<int>();
                nMaterials[i] = new List<short>();
            }

            int baseX = 0;
            int baseY = 0;
            int baseZ = 0;

            float s = 1;

            Func<float, float, float, int> packVerts = (x, y, z) =>
            {
                byte x0 = (byte)((int)x & ((1 << 10) - 1));
                byte y0 = (byte)((int)y & ((1 << 10) - 1));
                byte z0 = (byte)((int)z & ((1 << 10) - 1));

                int packed = x0 | (y0 << 10) | (z0 << 20);

                return packed;
            };

            Parallel.For(0, Side, (x) =>
            {
                List<int>[] verts = new List<int>[6];
                List<short>[] Materials = new List<short>[6];
                for (int i = 0; i < 6; i++)
                {
                    verts[i] = new List<int>();
                    Materials[i] = new List<short>();
                }
                for (int y = 0; y < Side; y++)
                {
                    for (int z = 0; z < Side; z++)
                    {

                        float x0 = baseX + x * s;
                        float y0 = baseY + y * s;
                        float z0 = baseZ + z * s;

                        if (!VoxelMap[MaterialMap[this[(int)x, y, z]]].Visible) continue;

                        if (z == 0 | !VoxelMap[MaterialMap[this[(int)x, y, z - 1]]].Visible)
                        {
                            verts[0].Add(packVerts(x0, y0, z0));
                            Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[0].Add(packVerts(x0 + s, y0 + s, z0));
                            Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[0].Add(packVerts(x0 + s, y0, z0));
                            Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[0].Add(packVerts(x0 + s, y0 + s, z0));
                            Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[0].Add(packVerts(x0, y0, z0));
                            Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[0].Add(packVerts(x0, y0 + s, z0));
                            Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);
                        }

                        if (z == Side - 1 | !VoxelMap[MaterialMap[this[(int)x, y, z + 1]]].Visible)
                        {
                            verts[3].Add(packVerts(x0, y0, z0 + s));
                            Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[3].Add(packVerts(x0 + s, y0, z0 + s));
                            Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[3].Add(packVerts(x0 + s, y0 + s, z0 + s));
                            Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[3].Add(packVerts(x0 + s, y0 + s, z0 + s));
                            Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[3].Add(packVerts(x0, y0 + s, z0 + s));
                            Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[3].Add(packVerts(x0, y0, z0 + s));
                            Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);
                        }

                        if (y == 0 | !VoxelMap[MaterialMap[this[(int)x, y - 1, z]]].Visible)
                        {
                            verts[1].Add(packVerts(x0 + s, y0, z0 + s));
                            Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[1].Add(packVerts(x0, y0, z0 + s));
                            Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[1].Add(packVerts(x0, y0, z0));
                            Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[1].Add(packVerts(x0, y0, z0));
                            Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[1].Add(packVerts(x0 + s, y0, z0));
                            Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[1].Add(packVerts(x0 + s, y0, z0 + s));
                            Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);
                        }

                        if (y == Side - 1 | !VoxelMap[MaterialMap[this[(int)x, y + 1, z]]].Visible)
                        {
                            verts[4].Add(packVerts(x0, y0 + s, z0));
                            Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[4].Add(packVerts(x0, y0 + s, z0 + s));
                            Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[4].Add(packVerts(x0 + s, y0 + s, z0 + s));
                            Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[4].Add(packVerts(x0 + s, y0 + s, z0 + s));
                            Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[4].Add(packVerts(x0 + s, y0 + s, z0));
                            Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[4].Add(packVerts(x0, y0 + s, z0));
                            Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);
                        }

                        if (x == 0 | !VoxelMap[MaterialMap[this[x - 1, y, z]]].Visible)
                        {
                            verts[2].Add(packVerts(x0, y0 + s, z0 + s));
                            Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[2].Add(packVerts(x0, y0 + s, z0));
                            Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[2].Add(packVerts(x0, y0, z0));
                            Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[2].Add(packVerts(x0, y0, z0));
                            Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[2].Add(packVerts(x0, y0, z0 + s));
                            Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[2].Add(packVerts(x0, y0 + s, z0 + s));
                            Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);
                        }

                        if (x == Side - 1 | !VoxelMap[MaterialMap[this[x + 1, y, z]]].Visible)
                        {
                            verts[5].Add(packVerts(x0 + s, y0, z0));
                            Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[5].Add(packVerts(x0 + s, y0 + s, z0));
                            Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[5].Add(packVerts(x0 + s, y0 + s, z0 + s));
                            Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[5].Add(packVerts(x0 + s, y0 + s, z0 + s));
                            Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[5].Add(packVerts(x0 + s, y0, z0 + s));
                            Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                            verts[5].Add(packVerts(x0 + s, y0, z0));
                            Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);
                        }
                    }
                }

                lock (nverts)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        nverts[i].AddRange(verts[i]);
                        nMaterials[i].AddRange(Materials[i]);
                    }
                }
            });

            List<int> netVerts = new List<int>();
            List<short> netMats = new List<short>();

            NormalOffsets = new int[6];
            NormalGroupSizes = new int[7];

            for (int i = 0; i < 6; i++)
            {
                NormalOffsets[i] = netVerts.Count;
                NormalGroupSizes[i] = nverts[i].Count;
                NormalGroupSizes[6] += NormalGroupSizes[i];
                netVerts.AddRange(nverts[i]);
                netMats.AddRange(nMaterials[i]);
            }
            vArray = new VertexArray();
            vMat = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            vMat.BufferData(0, netMats.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            vArray.SetBufferObject(1, vMat, 1, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Short);

            vBuf = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            vBuf.BufferData(0, netVerts.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            vArray.SetBufferObject(0, vBuf, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Int2101010Rev);
            */
            #endregion


            #region Naive Mesher Asynchronous
            /*
            ChunkReady = false;

            int baseX = 0;
            int baseY = 0;
            int baseZ = 0;

            float s = 1;


            BufferStreamer.QueueTask((a) =>
            {
                Fence f = new Fence();

                vArray = new VertexArray();
                vMat = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                //vMat.BufferData(0, netMats.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
                vArray.SetBufferObject(1, vMat, 1, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Short);

                vBuf = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                //vBuf.BufferData(0, netVerts.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
                vArray.SetBufferObject(0, vBuf, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Int2101010Rev);
                f.PlaceFence();

                Action<object> runner = (q0) =>
                {
                    List<int>[] verts = new List<int>[6];
                    List<short>[] Materials = new List<short>[6];
                    for (int i = 0; i < 6; i++)
                    {
                        verts[i] = new List<int>();
                        Materials[i] = new List<short>();
                    }

                    for (int x = 0; x < Side; x++)
                    {
                        for (int y = 0; y < Side; y++)
                        {
                            for (int z = 0; z < Side; z++)
                            {

                                float x0 = baseX + x * s;
                                float y0 = baseY + y * s;
                                float z0 = baseZ + z * s;

                                if (!VoxelMap[MaterialMap[this[(int)x, y, z]]].Visible) continue;

                                if (z == 0 | !VoxelMap[MaterialMap[this[(int)x, y, z - 1]]].Visible)
                                {
                                    verts[0].Add(packVerts(x0, y0, z0));
                                    Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[0].Add(packVerts(x0 + s, y0 + s, z0));
                                    Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[0].Add(packVerts(x0 + s, y0, z0));
                                    Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[0].Add(packVerts(x0 + s, y0 + s, z0));
                                    Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[0].Add(packVerts(x0, y0, z0));
                                    Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[0].Add(packVerts(x0, y0 + s, z0));
                                    Materials[0].Add((short)MaterialMap[this[(int)x, y, z]]);
                                }

                                if (z == Side - 1 | !VoxelMap[MaterialMap[this[(int)x, y, z + 1]]].Visible)
                                {
                                    verts[3].Add(packVerts(x0, y0, z0 + s));
                                    Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[3].Add(packVerts(x0 + s, y0, z0 + s));
                                    Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[3].Add(packVerts(x0 + s, y0 + s, z0 + s));
                                    Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[3].Add(packVerts(x0 + s, y0 + s, z0 + s));
                                    Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[3].Add(packVerts(x0, y0 + s, z0 + s));
                                    Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[3].Add(packVerts(x0, y0, z0 + s));
                                    Materials[3].Add((short)MaterialMap[this[(int)x, y, z]]);
                                }

                                if (y == 0 | !VoxelMap[MaterialMap[this[(int)x, y - 1, z]]].Visible)
                                {
                                    verts[1].Add(packVerts(x0 + s, y0, z0 + s));
                                    Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[1].Add(packVerts(x0, y0, z0 + s));
                                    Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[1].Add(packVerts(x0, y0, z0));
                                    Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[1].Add(packVerts(x0, y0, z0));
                                    Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[1].Add(packVerts(x0 + s, y0, z0));
                                    Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[1].Add(packVerts(x0 + s, y0, z0 + s));
                                    Materials[1].Add((short)MaterialMap[this[(int)x, y, z]]);
                                }

                                if (y == Side - 1 | !VoxelMap[MaterialMap[this[(int)x, y + 1, z]]].Visible)
                                {
                                    verts[4].Add(packVerts(x0, y0 + s, z0));
                                    Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[4].Add(packVerts(x0, y0 + s, z0 + s));
                                    Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[4].Add(packVerts(x0 + s, y0 + s, z0 + s));
                                    Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[4].Add(packVerts(x0 + s, y0 + s, z0 + s));
                                    Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[4].Add(packVerts(x0 + s, y0 + s, z0));
                                    Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[4].Add(packVerts(x0, y0 + s, z0));
                                    Materials[4].Add((short)MaterialMap[this[(int)x, y, z]]);
                                }

                                if (x == 0 | !VoxelMap[MaterialMap[this[x - 1, y, z]]].Visible)
                                {
                                    verts[2].Add(packVerts(x0, y0 + s, z0 + s));
                                    Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[2].Add(packVerts(x0, y0 + s, z0));
                                    Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[2].Add(packVerts(x0, y0, z0));
                                    Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[2].Add(packVerts(x0, y0, z0));
                                    Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[2].Add(packVerts(x0, y0, z0 + s));
                                    Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[2].Add(packVerts(x0, y0 + s, z0 + s));
                                    Materials[2].Add((short)MaterialMap[this[(int)x, y, z]]);
                                }

                                if (x == Side - 1 | !VoxelMap[MaterialMap[this[x + 1, y, z]]].Visible)
                                {
                                    verts[5].Add(packVerts(x0 + s, y0, z0));
                                    Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[5].Add(packVerts(x0 + s, y0 + s, z0));
                                    Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[5].Add(packVerts(x0 + s, y0 + s, z0 + s));
                                    Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[5].Add(packVerts(x0 + s, y0 + s, z0 + s));
                                    Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[5].Add(packVerts(x0 + s, y0, z0 + s));
                                    Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);

                                    verts[5].Add(packVerts(x0 + s, y0, z0));
                                    Materials[5].Add((short)MaterialMap[this[(int)x, y, z]]);
                                }
                            }
                        }
                    }

                    object[] ptrs = (object[])q0;

                    List<int> sV = new List<int>();
                    List<short> sM = new List<short>();

                    NormalOffsets = new int[6];
                    NormalGroupSizes = new int[7];

                    for (int i = 0; i < 6; i++)
                    {
                        NormalOffsets[i] = sV.Count;
                        NormalGroupSizes[i] = verts[i].Count;
                        NormalGroupSizes[6] += NormalGroupSizes[i];
                        sV.AddRange(verts[i]);
                        sM.AddRange(Materials[i]);
                    }

                    BufferStreamer.QueueTask((o0) =>
                    {
                        object[] o1 = (object[])o0;
                        GPUBuffer p0 = (GPUBuffer)o1[0];
                        GPUBuffer p1 = (GPUBuffer)o1[1];

                        Fence f0 = (Fence)o1[4];

                        f0.Raised(0);
                        short[] v1 = (short[])o1[3];
                        p0.BufferData(0, v1, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);

                        int[] v0 = (int[])o1[2];
                        p1.BufferData(0, v0, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);

                        //VertexArray v2 = (VertexArray)o1[5];
                        //v2.SetBufferObject(1, p0, 1, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Short);
                        //v2.SetBufferObject(0, p1, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Int2101010Rev);

                        ChunkReady = true;

                    }, new object[] { ptrs[0], ptrs[1], sV.ToArray(), sM.ToArray(), f, ptrs[2] });

                };  //Delegate end

                ThreadPool.QueueUserWorkItem(new WaitCallback(runner), new object[] { vMat, vBuf, vArray });
            }, null);
            */
            #endregion

            #region Advanced Greedy Mesher
            ChunkReady = false;
            List<int>[] rV = new List<int>[6];
            List<short>[] rM = new List<short>[6];

            for (int i = 0; i < 6; i++)
            {
                rV[i] = new List<int>();
                rM[i] = new List<short>();
            }

            List<short> ValidMats = new List<short>();
            for (int i = 0; i < MaterialMap.Length; i++)
            {
                if (VoxelMap[MaterialMap[i]].Visible && !ValidMats.Contains((short)MaterialMap[i])) ValidMats.Add((short)MaterialMap[i]);
            }

            bool[] mask2 = new bool[Side];
            bool[,] mask = new bool[Side, Side];

            for (int m = 0; m < ValidMats.Count; m++)
            {
                for (int d = 0; d < 3; d++)
                {
                    Vector3 norm = Vector3.Zero;
                    norm[d] = 1;


                    int u = (d + 1) % 3;
                    int v = (d + 2) % 3;

                    Vector3 x = Vector3.Zero;
                    bool empty = true;

                    for (x[d] = -1; x[d] < Side;)
                    {
                        norm = Vector3.Zero;
                        norm[d] = 1;

                        for (x[u] = 0; x[u] < Side; x[u]++)
                        {
                            bool sum = false;
                            for (x[v] = 0; x[v] < Side; x[v]++)
                            {
                                //Compute a mask
                                if ((!VoxelMap[MaterialMap[this[x + norm]]].Visible | !VoxelMap[MaterialMap[this[x - norm]]].Visible))
                                {
                                    mask[(int)x[u], (int)x[v]] = (0 <= x[d] ? MaterialMap[this[x]] == ValidMats[m] : false) !=
                                        (x[d] < Side - 1 ? MaterialMap[this[x + norm]] == ValidMats[m] : false);

                                    empty = false;
                                    sum = true;
                                }
                            }
                            mask2[(int)x[u]] = sum;
                        }
                        x[d]++;
                        if (empty) continue;

                        //Now determine the edges from the mask
                        for (int i = 0; i < Side; i++)
                        {
                            if (!mask2[i])
                            {
                                continue;
                            }
                            int minW = Side;
                            for (int j = 0; j < Side;)
                            {
                                if (!mask[i, j])
                                {
                                    j++;
                                    continue;
                                }

                                int w = 1, h = 1;
                                for (; i + w < Side && mask[i + w, j]; w++) ;

                                if (w < minW) minW = w;

                                bool done = false;
                                for (; j + h < Side; h++)
                                {
                                    for (int i0 = 0; i0 < w; i0++)
                                    {
                                        if (!mask[i + i0, j + h])
                                        {
                                            done = true;
                                            break;
                                        }
                                    }
                                    if (done) break;
                                }


                                Vector3 tL = Vector3.Zero;
                                Vector3 tR = Vector3.Zero;
                                Vector3 bL = Vector3.Zero;
                                Vector3 bR = Vector3.Zero;

                                x[u] = i;
                                x[v] = j;

                                tL[d] = x[d];
                                tR[d] = x[d];
                                bL[d] = x[d];
                                bR[d] = x[d];

                                tL[u] = i;
                                tL[v] = j;

                                tR[u] = i + w;
                                tR[v] = j;

                                bL[u] = i;
                                bL[v] = j + h;

                                bR[u] = i + w;
                                bR[v] = j + h;

                                norm = Vector3.Zero;
                                norm[d] = 1;

                                Vector3 cPos = Vector3.Zero;
                                cPos[u] = i;
                                cPos[v] = j;
                                cPos[d] = x[d] - 1;

                                if (!VoxelMap[MaterialMap[this[cPos]]].Visible) norm[d] = -1;
                                else norm[d] = 1;

                                int normIndex = 0;
                                for (; normIndex < Normals.Length && Normals[normIndex] != norm; normIndex++) ;

                                if (normIndex <= 2)
                                {
                                    rV[normIndex].Add(packVerts(tR.X, tR.Y, tR.Z, 0));
                                    rV[normIndex].Add(packVerts(tL.X, tL.Y, tL.Z, 1));
                                    rV[normIndex].Add(packVerts(bL.X, bL.Y, bL.Z, 2));
                                    rV[normIndex].Add(packVerts(bL.X, bL.Y, bL.Z, 2));
                                    rV[normIndex].Add(packVerts(bR.X, bR.Y, bR.Z, 3));
                                    rV[normIndex].Add(packVerts(tR.X, tR.Y, tR.Z, 0));
                                }
                                else
                                {
                                    rV[normIndex].Add(packVerts(tL.X, tL.Y, tL.Z, 0));
                                    rV[normIndex].Add(packVerts(tR.X, tR.Y, tR.Z, 1));
                                    rV[normIndex].Add(packVerts(bL.X, bL.Y, bL.Z, 2));
                                    rV[normIndex].Add(packVerts(bL.X, bL.Y, bL.Z, 2));
                                    rV[normIndex].Add(packVerts(tR.X, tR.Y, tR.Z, 3));
                                    rV[normIndex].Add(packVerts(bR.X, bR.Y, bR.Z, 0));
                                }

                                rM[normIndex].Add(ValidMats[m]);
                                rM[normIndex].Add(ValidMats[m]);
                                rM[normIndex].Add(ValidMats[m]);
                                rM[normIndex].Add(ValidMats[m]);
                                rM[normIndex].Add(ValidMats[m]);
                                rM[normIndex].Add(ValidMats[m]);

                                for (int w0 = i; w0 < i + w; w0++)
                                    for (int j0 = j; j0 < j + h; j0++)
                                    {
                                        mask[w0, j0] = false;
                                    }

                                j += h;
                            }
                        }


                    }

                }
            }   //d loop


            NormalOffsets = new int[6];
            NormalGroupSizes = new int[7];

            List<int> vertex = new List<int>();
            List<short> mat = new List<short>();
            for (int i = 0; i < 6; i++)
            {
                NormalOffsets[i] = vertex.Count;
                NormalGroupSizes[i] = rV[i].Count;
                NormalGroupSizes[6] += NormalGroupSizes[i];
                vertex.AddRange(rV[i]);
                mat.AddRange(rM[i]);
            }

            BufferStreamer.QueueTask((a) =>
            {
                if (vArray == null) vArray = new VertexArray();
                if (vBuf == null)
                {
                    vBuf = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                    vArray.SetBufferObject(0, vBuf, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Int2101010Rev);
                }

                if (vMat == null)
                {
                    vMat = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                    vArray.SetBufferObject(1, vMat, 1, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Short);
                }

                vBuf.OrphanData(vertex.Count * 4, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
                vMat.OrphanData(mat.Count * 2, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);

                BufferStreamer.QueueTask((b) =>
                {
                    vBuf.BufferSubData(0, vertex.ToArray(), vertex.Count);
                    vMat.BufferSubData(0, mat.ToArray(), mat.Count);
                }, null);

                BufferStreamer.QueueTask((c) => ChunkReady = true, null);
            }, null);
            #endregion

            #region Greedy Mesher
            /*
            ChunkReady = false;

            List<int>[] rVerts = new List<int>[6];
            List<short>[] rMats = new List<short>[6];
            for (int i = 0; i < 6; i++)
            {
                rVerts[i] = new List<int>();
                rMats[i] = new List<short>();
            }
            for (int d = 0; d < 3; d++)
            {
                List<int>[] normalVerts = new List<int>[6];
                List<short>[] mats = new List<short>[6];

                for (int i0 = 0; i0 < 6; i0++)
                {
                    mats[i0] = new List<short>();
                    normalVerts[i0] = new List<int>();
                }

                for (int q9 = 0; q9 < MaterialMap.Length; q9++)
                {
                    curMaterial = q9;

                    int i, j, k, l, w, h, u = (d + 1) % 3, v = (d + 2) % 3;
                    int[] x = new int[3];
                    int[] q = new int[3];
                    bool[] mask = new bool[Side * Side];

                    q[d] = 1;

                    for (x[d] = -1; x[d] < Side;)
                    {
                        // Compute the mask
                        int n = 0;
                        for (x[v] = 0; x[v] < Side; ++x[v])
                        {
                            for (x[u] = 0; x[u] < Side; ++x[u])
                            {
                                mask[n++] = (0 <= x[d] ? data(x[0], x[1], x[2]) : false) !=
                                    (x[d] < Side - 1 ? data(x[0] + q[0], x[1] + q[1], x[2] + q[2]) : false);
                            }
                        }

                        // Increment x[d]
                        ++x[d];

                        // Generate mesh for mask using lexicographic ordering
                        n = 0;
                        for (j = 0; j < Side; ++j)
                        {
                            for (i = 0; i < Side;)
                            {
                                if (mask[n])
                                {
                                    // Compute width
                                    for (w = 1; i + w < Side && mask[n + w]; ++w) ;

                                    // Compute height (this is slightly awkward
                                    var done = false;
                                    for (h = 1; j + h < Side; ++h)
                                    {
                                        for (k = 0; k < w; ++k)
                                        {
                                            if (!mask[n + k + h * Side])
                                            {
                                                done = true;
                                                break;
                                            }
                                        }
                                        if (done) break;
                                    }

                                    // Add quad
                                    x[u] = i; x[v] = j;
                                    int[] du = new int[3];
                                    int[] dv = new int[3];
                                    du[u] = w;
                                    dv[v] = h;

                                    Vector3[] v0 = new Vector3[] {
                                        new Vector3(x[0], x[1], x[2]),
                                        new Vector3(x[0] + du[0], x[1] + du[1], x[2] + du[2]),
                                        new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]),
                                        new Vector3(x[0] + du[0] + dv[0], x[1] + du[1] + dv[1], x[2] + du[2] + dv[2]),
                                        new Vector3(x[0] + dv[0], x[1] + dv[1], x[2] + dv[2]),
                                        new Vector3(x[0], x[1], x[2])
                                };

                                    Vector3 curVoxel = Vector3.Zero;
                                    curVoxel[u] = i;
                                    curVoxel[v] = j;
                                    curVoxel[d] = x[d] - 1;

                                    Vector3 nNorm = Vector3.Zero;

                                    if (data((int)curVoxel.X, (int)curVoxel.Y, (int)curVoxel.Z))
                                    {
                                        nNorm[d] = -1;
                                    }
                                    else
                                    {
                                        nNorm[d] = 1;
                                    }

                                    nNorm.X = (float)Math.Round(nNorm.X);
                                    nNorm.Y = (float)Math.Round(nNorm.Y);
                                    nNorm.Z = (float)Math.Round(nNorm.Z);

                                    curVoxel = new Vector3(x[0], x[1], x[2]) + nNorm;

                                    if (nNorm.X > 0 | nNorm.Y > 0 | nNorm.Z > 0)
                                    {
                                        Vector3 tmp0 = v0[1];
                                        Vector3 tmp1 = v0[2];

                                        v0[1] = tmp1;
                                        v0[2] = tmp0;

                                        tmp0 = v0[3];
                                        tmp1 = v0[4];

                                        v0[3] = tmp1;
                                        v0[4] = tmp0;

                                    }

                                    int[] v1 = new int[v0.Length];
                                    for (int q0 = 0; q0 < v1.Length; q0++)
                                    {
                                        v1[q0] = packVerts(v0[q0].X, v0[q0].Y, v0[q0].Z);

                                    }

                                    if (nNorm == Vector3.UnitX)
                                    {
                                        normalVerts[0].AddRange(v1);
                                        for (int i0 = 0; i0 < v1.Length; i0++) mats[0].Add((short)MaterialMap[this[curVoxel]]);
                                    }
                                    else if (nNorm == Vector3.UnitY)
                                    {
                                        normalVerts[1].AddRange(v1);
                                        for (int i0 = 0; i0 < v1.Length; i0++) mats[1].Add((short)MaterialMap[this[curVoxel]]);
                                    }
                                    else if (nNorm == Vector3.UnitZ)
                                    {
                                        normalVerts[5].AddRange(v1);
                                        for (int i0 = 0; i0 < v1.Length; i0++) mats[5].Add((short)MaterialMap[this[curVoxel]]);
                                    }
                                    else if (nNorm == -Vector3.UnitX)
                                    {
                                        normalVerts[3].AddRange(v1);
                                        for (int i0 = 0; i0 < v1.Length; i0++) mats[3].Add((short)MaterialMap[this[curVoxel]]);
                                    }
                                    else if (nNorm == -Vector3.UnitY)
                                    {
                                        normalVerts[4].AddRange(v1);
                                        for (int i0 = 0; i0 < v1.Length; i0++) mats[4].Add((short)MaterialMap[this[curVoxel]]);
                                    }
                                    else if (nNorm == -Vector3.UnitZ)
                                    {
                                        normalVerts[2].AddRange(v1);    //Switched around to make the normal array cleaner
                                        for (int i0 = 0; i0 < v1.Length; i0++) mats[2].Add((short)MaterialMap[this[curVoxel]]);
                                    }
                                    else throw new Exception("Invalid Face!");

                                    // Zero-out mask
                                    for (l = 0; l < h; ++l)
                                    {
                                        for (k = 0; k < w; ++k)
                                        {
                                            mask[n + k + l * Side] = false;
                                        }
                                    }

                                    // Increment counters and continue
                                    i += w; n += w;
                                }
                                else
                                {
                                    ++i; ++n;
                                }
                            }
                        }
                    }

                    lock (rVerts)
                    {
                        for (int i0 = 0; i0 < 6; i0++)
                        {
                            rMats[i0].AddRange(mats[i0]);
                            rVerts[i0].AddRange(normalVerts[i0]);
                        }
                    }
                }
            }

            NormalOffsets = new int[6];
            NormalGroupSizes = new int[7];

            List<int> vertex = new List<int>();
            List<short> mat = new List<short>();
            for (int i = 0; i < 6; i++)
            {
                NormalOffsets[i] = vertex.Count;
                NormalGroupSizes[i] = rVerts[i].Count;
                NormalGroupSizes[6] += NormalGroupSizes[i];
                vertex.AddRange(rVerts[i]);
                mat.AddRange(rMats[i]);
            }

            BufferStreamer.QueueTask((a) =>
            {
                vArray = new VertexArray();
                vBuf = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                vBuf.BufferData(0, vertex.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
                vArray.SetBufferObject(0, vBuf, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Int2101010Rev);

                vMat = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                vMat.BufferData(0, mat.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
                vArray.SetBufferObject(1, vMat, 1, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Short);
                ChunkReady = true;
            }, null);
            */

            #endregion
        }


        public void Bind()
        {
            GraphicsDevice.SetVertexArray(vArray);
        }
    }
}
