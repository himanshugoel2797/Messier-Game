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
        public int Side = 128;
        public int BitPerVoxel = 2;

        public int VoxelsPerInt
        {
            get
            {
                return 32 / BitPerVoxel;
            }
        }

        public Vector3[] Normals { get; } = new Vector3[]
        {
                new Vector3(-1, 0, 0),
                new Vector3(0, -1, 0),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1)
        };

        public VoxelDataType datType { get; set; } = VoxelDataType.Chunk;

        public uint[,,] Data;

        public int[] MaterialMap;
        public VoxelTypeMap VoxelMap;

        internal VertexArray[] vArray;
        internal GPUBuffer[] vBuf;

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


        private Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            return (Vector3.Cross(b - a, c - a));
        }

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
        }

        public void GenerateMesh()
        {
            #region Naive Mesher
            /*
            List<float>[] nVerts = new List<float>[6];
            for (int i = 0; i < 6; i++) nVerts[i] = new List<float>();

            int baseX = -Side / 2;
            int baseY = -Side / 2;
            int baseZ = -Side / 2;
            for (int x = 0; x < Side; x++)
            {
                for (int y = 0; y < Side; y++)
                {
                    for (int z = 0; z < Side; z++)
                    {
                        int x0 = baseX + x;
                        int y0 = baseY + y;
                        int z0 = baseZ + z;
                        if (!VoxelMap[MaterialMap[this[x, y, z]]].Visible) continue;
                        if ((z == 0) | !VoxelMap[MaterialMap[this[x, y, z - 1]]].Visible)
                        {
                            nVerts[2].Add(x0 + 1);
                            nVerts[2].Add(y0);
                            nVerts[2].Add(z0);
                            nVerts[2].Add(MaterialMap[this[x, y, z]]);
                            nVerts[2].Add(x0);
                            nVerts[2].Add(y0);
                            nVerts[2].Add(z0);
                            nVerts[2].Add(MaterialMap[this[x, y, z]]);
                            nVerts[2].Add(x0 + 1);
                            nVerts[2].Add(y0 + 1);
                            nVerts[2].Add(z0);
                            nVerts[2].Add(MaterialMap[this[x, y, z]]);
                            nVerts[2].Add(x0 + 1);
                            nVerts[2].Add(y0 + 1);
                            nVerts[2].Add(z0);
                            nVerts[2].Add(MaterialMap[this[x, y, z]]);
                            nVerts[2].Add(x0);
                            nVerts[2].Add(y0);
                            nVerts[2].Add(z0);
                            nVerts[2].Add(MaterialMap[this[x, y, z]]);
                            nVerts[2].Add(x0);
                            nVerts[2].Add(y0 + 1);
                            nVerts[2].Add(z0);
                            nVerts[2].Add(MaterialMap[this[x, y, z]]);
                        }
                        if ((z == (Side - 1)) | !VoxelMap[MaterialMap[this[x, y, z + 1]]].Visible)
                        {
                            nVerts[5].Add(x0);
                            nVerts[5].Add(y0);
                            nVerts[5].Add(z0 + 1);
                            nVerts[5].Add(MaterialMap[this[x, y, z]]);
                            nVerts[5].Add(x0 + 1);
                            nVerts[5].Add(y0);
                            nVerts[5].Add(z0 + 1);
                            nVerts[5].Add(MaterialMap[this[x, y, z]]);
                            nVerts[5].Add(x0 + 1);
                            nVerts[5].Add(y0 + 1);
                            nVerts[5].Add(z0 + 1);
                            nVerts[5].Add(MaterialMap[this[x, y, z]]);
                            nVerts[5].Add(x0 + 1);
                            nVerts[5].Add(y0 + 1);
                            nVerts[5].Add(z0 + 1);
                            nVerts[5].Add(MaterialMap[this[x, y, z]]);
                            nVerts[5].Add(x0);
                            nVerts[5].Add(y0 + 1);
                            nVerts[5].Add(z0 + 1);
                            nVerts[5].Add(MaterialMap[this[x, y, z]]);
                            nVerts[5].Add(x0);
                            nVerts[5].Add(y0);
                            nVerts[5].Add(z0 + 1);
                            nVerts[5].Add(MaterialMap[this[x, y, z]]);
                        }
                        if ((y == 0) | !VoxelMap[MaterialMap[this[x, y - 1, z]]].Visible)
                        {
                            nVerts[1].Add(x0);
                            nVerts[1].Add(y0);
                            nVerts[1].Add(z0);
                            nVerts[1].Add(MaterialMap[this[x, y, z]]);
                            nVerts[1].Add(x0 + 1);
                            nVerts[1].Add(y0);
                            nVerts[1].Add(z0 + 1);
                            nVerts[1].Add(MaterialMap[this[x, y, z]]);
                            nVerts[1].Add(x0);
                            nVerts[1].Add(y0);
                            nVerts[1].Add(z0 + 1);
                            nVerts[1].Add(MaterialMap[this[x, y, z]]);
                            nVerts[1].Add(x0);
                            nVerts[1].Add(y0);
                            nVerts[1].Add(z0);
                            nVerts[1].Add(MaterialMap[this[x, y, z]]);
                            nVerts[1].Add(x0 + 1);
                            nVerts[1].Add(y0);
                            nVerts[1].Add(z0);
                            nVerts[1].Add(MaterialMap[this[x, y, z]]);
                            nVerts[1].Add(x0 + 1);
                            nVerts[1].Add(y0);
                            nVerts[1].Add(z0 + 1);
                            nVerts[1].Add(MaterialMap[this[x, y, z]]);
                        }
                        if ((y == Side - 1) | !VoxelMap[MaterialMap[this[x, y + 1, z]]].Visible)
                        {
                            nVerts[4].Add(x0);
                            nVerts[4].Add(y0 + 1);
                            nVerts[4].Add(z0);
                            nVerts[4].Add(MaterialMap[this[x, y, z]]);
                            nVerts[4].Add(x0);
                            nVerts[4].Add(y0 + 1);
                            nVerts[4].Add(z0 + 1);
                            nVerts[4].Add(MaterialMap[this[x, y, z]]);
                            nVerts[4].Add(x0 + 1);
                            nVerts[4].Add(y0 + 1);
                            nVerts[4].Add(z0 + 1);
                            nVerts[4].Add(MaterialMap[this[x, y, z]]);
                            nVerts[4].Add(x0 + 1);
                            nVerts[4].Add(y0 + 1);
                            nVerts[4].Add(z0 + 1);
                            nVerts[4].Add(MaterialMap[this[x, y, z]]);
                            nVerts[4].Add(x0 + 1);
                            nVerts[4].Add(y0 + 1);
                            nVerts[4].Add(z0);
                            nVerts[4].Add(MaterialMap[this[x, y, z]]);
                            nVerts[4].Add(x0);
                            nVerts[4].Add(y0 + 1);
                            nVerts[4].Add(z0);
                            nVerts[4].Add(MaterialMap[this[x, y, z]]);
                        }
                        if ((x == 0) | !VoxelMap[MaterialMap[this[x - 1, y, z]]].Visible)
                        {
                            nVerts[0].Add(x0);
                            nVerts[0].Add(y0);
                            nVerts[0].Add(z0);
                            nVerts[0].Add(MaterialMap[this[x, y, z]]);

                            nVerts[0].Add(x0);
                            nVerts[0].Add(y0 + 1);
                            nVerts[0].Add(z0);
                            nVerts[0].Add(MaterialMap[this[x, y, z]]);

                            nVerts[0].Add(x0);
                            nVerts[0].Add(y0 + 1);
                            nVerts[0].Add(z0 + 1);
                            nVerts[0].Add(MaterialMap[this[x, y, z]]);

                            nVerts[0].Add(x0);
                            nVerts[0].Add(y0 + 1);
                            nVerts[0].Add(z0 + 1);
                            nVerts[0].Add(MaterialMap[this[x, y, z]]);

                            nVerts[0].Add(x0);
                            nVerts[0].Add(y0);
                            nVerts[0].Add(z0 + 1);
                            nVerts[0].Add(MaterialMap[this[x, y, z]]);

                            nVerts[0].Add(x0);
                            nVerts[0].Add(y0);
                            nVerts[0].Add(z0);
                            nVerts[0].Add(MaterialMap[this[x, y, z]]);
                        }
                        if ((x == Side - 1) | !VoxelMap[MaterialMap[this[x + 1, y, z]]].Visible)
                        {
                            nVerts[3].Add(x0);
                            nVerts[3].Add(y0);
                            nVerts[3].Add(z0);
                            nVerts[3].Add(MaterialMap[this[x, y, z]]);

                            nVerts[3].Add(x0);
                            nVerts[3].Add(y0 + 1);
                            nVerts[3].Add(z0);
                            nVerts[3].Add(MaterialMap[this[x, y, z]]);

                            nVerts[3].Add(x0);
                            nVerts[3].Add(y0 + 1);
                            nVerts[3].Add(z0 + 1);
                            nVerts[3].Add(MaterialMap[this[x, y, z]]);

                            nVerts[3].Add(x0);
                            nVerts[3].Add(y0 + 1);
                            nVerts[3].Add(z0 + 1);
                            nVerts[3].Add(MaterialMap[this[x, y, z]]);

                            nVerts[3].Add(x0);
                            nVerts[3].Add(y0);
                            nVerts[3].Add(z0 + 1);
                            nVerts[3].Add(MaterialMap[this[x, y, z]]);

                            nVerts[3].Add(x0);
                            nVerts[3].Add(y0);
                            nVerts[3].Add(z0);
                            nVerts[3].Add(MaterialMap[this[x, y, z]]);
                        }
                    }
                }
            }

            vArray = new VertexArray[6];
            vBuf = new GPUBuffer[6];

            for (int i = 0; i < 6; i++)
            {
                vBuf[i] = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                vArray[i] = new VertexArray();

                vBuf[i].BufferData(0, nVerts[i].ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
                vArray[i].SetBufferObject(0, vBuf[i], 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
            }
            */
            #endregion

            #region Naive Surface Nets

            List<float>[] nVerts = new List<float>[6];
            for (int i = 0; i < 6; i++) nVerts[i] = new List<float>();

            int baseX = -Side / 2;
            int baseY = -Side / 2;
            int baseZ = -Side / 2;

            for (int d = 0; d < 3; d++)
            {
                Vector3 u0 = new Vector3();
                Vector3 v0 = new Vector3();
                Vector3 w0 = new Vector3();

                Vector3 a = new Vector3();
                int u = (d + 1) % 3;
                int v = (d + 2) % 3;

                u0[u] = 1;
                v0[v] = 1;
                w0[d] = 1;

                int ctrl = 0;

                for (int x = 0; x < Side; x++)
                    for (int y = 0; y < Side;)
                    {
                        bool done = false;

                        a[u] = x;
                        a[v] = y;

                        for (a[d] = 0; a[d] < Side; a[d]++)
                        {
                            if (VoxelMap[MaterialMap[this[a]]].Visible)
                            {
                                done = true;
                                break;
                            }
                        }

                        if (done)
                        {
                            nVerts[0].Add(baseX + a[0]);
                            nVerts[0].Add(baseY + a[1]);
                            nVerts[0].Add(baseZ + a[2]);

                            nVerts[0].Add(MaterialMap[this[a]]);
                        }
                        if (ctrl == 0)
                        {
                            x++;
                        }
                        else if (ctrl == 1)
                        {
                            x--;
                            y++;
                        }
                        else if (ctrl == 2)
                        {
                            x++;
                            y--;
                        }
                        else if (ctrl == 3)
                        {
                            y++;
                        }
                        else if (ctrl == 4)
                        {
                            x--;
                            ctrl = -1;
                        }
                        ctrl++;

                    }
            }

            vArray = new VertexArray[6];
            vBuf = new GPUBuffer[6];

            for (int i = 0; i < 1; i++)
            {
                vBuf[i] = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                vArray[i] = new VertexArray();

                vBuf[i].BufferData(0, nVerts[i].ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
                vArray[i].SetBufferObject(0, vBuf[i], 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
            }
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
                                for (w = 1; i + w < Side && mask[n + w] && GetMat(i + w, j, x[(v + 1) % 3], u, v) == GetMat(i, j, x[(v + 1) % 3], u, v); ++w) ;

                                // Compute height (this is slightly awkward
                                var done = false;
                                for (h = 1; j + h < Side; ++h)
                                {
                                    for (k = 0; k < w; ++k)
                                    {
                                        if (!mask[n + k + h * Side] | GetMat(i + k, j + h, x[(v + 1) % 3], u, v) != GetMat(i, j, x[(v + 1) % 3], u, v))
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
                                curVoxel[(v + 1) % 3] = x[(v + 1) % 3] - 1;

                                Vector3 nNorm = Vector3.Zero;

                                if (data((int)curVoxel.X, (int)curVoxel.Y, (int)curVoxel.Z))
                                {
                                    nNorm[(v + 1) % 3] = -1;
                                }
                                else
                                {
                                    nNorm[(v + 1) % 3] = 1;
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


        public void Bind(int i)
        {
            GraphicsDevice.SetVertexArray(vArray[i]);
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
