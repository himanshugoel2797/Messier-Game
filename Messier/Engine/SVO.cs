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
        public const int Side = 128;
        public const int BitPerVoxel = 1;
        public const int VoxelsPerInt = 32 / BitPerVoxel;

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

        public void GenerateMesh()
        {
            #region Naive Mesher
            /*
            List<float> verts = new List<float>();
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

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0 + 1);
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

                        if ((z == (Side - 1)) | !VoxelMap[MaterialMap[this[x, y, z + 1]]].Visible)
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

                        if ((y == 0) | !VoxelMap[MaterialMap[this[x, y - 1, z]]].Visible)
                        {
                            verts.Add(x0);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0);
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

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0);
                            verts.Add(MaterialMap[this[x, y, z]]);

                            verts.Add(x0 + 1);
                            verts.Add(y0);
                            verts.Add(z0 + 1);
                            verts.Add(MaterialMap[this[x, y, z]]);
                        }

                        if ((y == Side - 1) | !VoxelMap[MaterialMap[this[x, y + 1, z]]].Visible)
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

                        if ((x == 0) | !VoxelMap[MaterialMap[this[x - 1, y, z]]].Visible)
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

                        if ((x == Side - 1) | !VoxelMap[MaterialMap[this[x + 1, y, z]]].Visible)
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
                    }
                }
            }
            
            vArray = new VertexArray();
            vBuf.BufferData(0, verts.ToArray(), OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            vArray.SetBufferObject(0, vBuf, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
            */
            #endregion

            #region Greedy Mesher
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

                                Vector3 t0 = (v0[1] - v0[0]);
                                Vector3 t1 = (v0[2] - v0[0]);

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

                                Matrix4 objTrans = new Matrix4(new Vector4(t0, 0), new Vector4(t1, 0), new Vector4(nNorm, 0), Vector4.Zero);
                                Vector3 vect = Vector3.Transform(nNorm, objTrans);

                                Vector4[] v1 = new Vector4[v0.Length];
                                for (int q0 = 0; q0 < v1.Length; q0++)
                                {
                                    v1[q0] = new Vector4(v0[q0], MaterialMap[this[x[0], x[1], x[2]]]);
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
                                    normalVerts[2].AddRange(v1);
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
                                    normalVerts[5].AddRange(v1);
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
