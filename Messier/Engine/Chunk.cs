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

        /*
        private bool data(int x, int y, int z)
        {
            return VoxelMap[MaterialMap[this[x, y, z]]].Visible;
        }

        private byte GetMat(int a0, int a1, int a2, int u, int v)
        {
            Vector3 a = Vector3.Zero;
            a[u] = a0;
            a[v] = a1;
            a[(v + 1) % 3] = a2;

            return this[a];
        }*/

        public void GenerateMesh()
        {
            #region Naive Mesher
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

            vBuf = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            vBuf.BufferData(0, netVerts.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            vArray.SetBufferObject(0, vBuf, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Int2101010Rev);
            vArray.SetBufferObject(1, vMat, 1, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Short);
            #endregion

            #region Greedy Mesher
            /*
            List<Vector4>[] rVerts = new List<Vector4>[6];
            for (int i = 0; i < 6; i++) rVerts[i] = new List<Vector4>();

            Parallel.For(0, 3, (d) =>
            {
                List<Vector4>[] normalVerts = new List<Vector4>[6];
                for (int i0 = 0; i0 < 6; i0++) normalVerts[i0] = new List<Vector4>();

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
                                for (w = 1; i + w < Side; ++w)
                                {
                                    if (GetMat(i + w, j, x[d], u, v) == GetMat(i, j, x[d], u, v)) break;
                                    if (mask[n + w]) break;
                                }

                                // Compute height (this is slightly awkward
                                var done = false;
                                for (h = 1; j + h < Side; ++h)
                                {
                                    for (k = 0; k < w; ++k)
                                    {
                                        if (!mask[n + k + h * Side] | GetMat(i + k, j + h, x[d], u, v) != GetMat(i, j, x[d], u, v))
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

                                curVoxel = new Vector3(x[0], x[1], x[2]);

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

                                Vector4[] v1 = new Vector4[v0.Length];
                                for (int q0 = 0; q0 < v1.Length; q0++)
                                {
                                    v1[q0] = new Vector4(v0[q0], MaterialMap[this[curVoxel]]);
                                }

                                if (nNorm == Vector3.UnitX)
                                {
                                    normalVerts[0].AddRange(v1);
                                }
                                else if (nNorm == Vector3.UnitY)
                                {
                                    normalVerts[1].AddRange(v1);
                                }
                                else if (nNorm == Vector3.UnitZ)
                                {
                                    normalVerts[5].AddRange(v1);
                                }
                                else if (nNorm == -Vector3.UnitX)
                                {
                                    normalVerts[3].AddRange(v1);
                                }
                                else if (nNorm == -Vector3.UnitY)
                                {
                                    normalVerts[4].AddRange(v1);
                                }
                                else if (nNorm == -Vector3.UnitZ)
                                {
                                    normalVerts[2].AddRange(v1);    //Switched around to make the normal array cleaner
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
                    for (int i0 = 0; i0 < 6; i0++) rVerts[i0].AddRange(normalVerts[i0]);
                }
            });

            Vector4[][] vertex = new Vector4[6][];
            for (int i = 0; i < 6; i++) vertex[i] = rVerts[i].ToArray();

            vArray = new VertexArray[6];
            vBuf = new GPUBuffer[6];

            for (int i = 0; i < 6; i++)
            {
                vArray[i] = new VertexArray();
                vBuf[i] = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                vBuf[i].BufferData(0, vertex[i], OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
                vArray[i].SetBufferObject(0, vBuf[i], 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
            }
            */
            #endregion
        }


        public void Bind()
        {
            GraphicsDevice.SetVertexArray(vArray);
        }
    }
}
