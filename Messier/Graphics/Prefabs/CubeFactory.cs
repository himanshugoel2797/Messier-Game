using Messier.Engine.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics.Prefabs
{
    class CubeFactory
    {
        static EngineObject eObj;

        static CubeFactory()
        {
            Init();
        }

        private static void Init()
        {
            eObj = new EngineObject();

            uint[] indices = new uint[] {
                0, 1, 2, 2, 1, 3,
                2, 3, 4, 4, 3, 5,
                4, 5, 6, 6, 5, 7,
                6, 7, 0, 0, 7, 1,
                1, 7, 3, 3, 7, 5,
                6, 0, 4, 4, 0, 2
            };

            float width = 0.5f, height = 0.5f, depth = 0.5f;
            float[] vers = new float[]{
                -width, -height, depth,    //0
                width, -height, depth,     //1
                -width, height, depth,      //2
                width, height, depth,      //3
                -width, height, -depth,     //4
                width, height, -depth,      //5
                -width, -height, -depth,     //6
                width, -height, -depth        //7
            };

            float[] uvs = new float[]
            {
                0, 0,
                1, 0,
                0, 1,
                1, 1,
            };

            float[] norm_p = new float[]
            {
                0, 0, 1,
                0, 1, 0,
                0, 0, -1,
                0, -1, 0,
                1, 0, 0,
                -1, 0, 0
            };

            List<float> norms = new List<float>();
            List<float> uvs_l = new List<float>();
            List<float> verts = new List<float>();
            List<uint> inds = new List<uint>();

            for(int i = 0; i < indices.Length; i++)
            {
                verts.Add(vers[indices[i] * 3]);
                verts.Add(vers[indices[i] * 3 + 1]);
                verts.Add(vers[indices[i] * 3 + 2]);

                int uv_index = i % 6;
                
                switch(uv_index)
                {
                    case 0:
                        uv_index = 0;
                        if (i / 6 == 2) uv_index = 3;
                        break;
                    case 1:
                        uv_index = 1;
                        if (i / 6 == 2) uv_index = 2;
                        break;
                    case 2:
                        uv_index = 2;
                        if (i / 6 == 2) uv_index = 1;
                        break;
                    case 3:
                        uv_index = 2;
                        if (i / 6 == 2) uv_index = 1;
                        break;
                    case 4:
                        uv_index = 1;
                        if (i / 6 == 2) uv_index = 2;
                        break;
                    case 5:
                        uv_index = 3;
                        if (i / 6 == 2) uv_index = 0;
                        break;
                }

                uvs_l.Add(uvs[uv_index * 2]);
                uvs_l.Add(uvs[uv_index * 2 + 1]);

                norms.Add(norm_p[i / 6]);
                norms.Add(norm_p[i / 6 + 1]);
                norms.Add(norm_p[i / 6 + 2]);

                inds.Add((uint)i);
            }

            eObj.SetIndices(0, inds.ToArray(), false);
            eObj.SetUVs(0, uvs_l.ToArray(), false);
            eObj.SetNormals(0, norms.ToArray(), false);
            eObj.SetVertices(0, verts.ToArray(), false);
        }

        public static EngineObject Create()
        {
            return new EngineObject(eObj, true);    //Lock the buffers from changes
        }
    }
}
