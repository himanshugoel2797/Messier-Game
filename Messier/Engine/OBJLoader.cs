using Messier.Engine.SceneGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public class OBJLoader
    {
        public static EngineObject[] Load(string file)
        {
            StreamReader r = new StreamReader(file);

            List<float> verts = new List<float>();
            List<float> uvs = new List<float>();
            List<float> norms = new List<float>();


            List<List<float>> vs = new List<List<float>>();
            List<List<float>> ns = new List<List<float>>();
            List<List<float>> us = new List<List<float>>();

            int index = -1;
            string ln;
            while (!r.EndOfStream)
            {
                ln = r.ReadLine().Trim();
                if (ln.StartsWith("vt"))
                {
                    string[] v = ln.Split(' ');
                    for (int i = 1; i < v.Length; i++)
                    {
                        uvs.Add(float.Parse(v[i]));
                    }
                }
                else if (ln.StartsWith("vn"))
                {
                    string[] v = ln.Split(' ');
                    for (int i = 1; i < v.Length; i++)
                    {
                        norms.Add(float.Parse(v[i]));
                    }
                }
                else if (ln.StartsWith("v") && !ln.StartsWith("vp"))
                {
                    string[] v = ln.Split(' ');
                    for (int i = 1; i < v.Length; i++)
                    {
                        verts.Add(float.Parse(v[i]));
                    }
                }
                else if (ln.StartsWith("f"))
                {
                    string[] v = ln.Split(' ');
                    for (int i = 1; i < v.Length; i++)
                    {
                        string[] p = v[i].Split('/');

                        vs[index].Add(verts[(int.Parse(p[0]) - 1) * 3]);
                        vs[index].Add(verts[(int.Parse(p[0]) - 1) * 3 + 1]);
                        vs[index].Add(verts[(int.Parse(p[0]) - 1) * 3 + 2]);

                        if (p.Length > 1 && p[1].Length > 0)
                        {
                            us[index].Add(uvs[(int.Parse(p[1]) - 1) * 2]);
                            us[index].Add(uvs[(int.Parse(p[1]) - 1) * 2 + 1]);
                        }

                        if (p.Length > 2 && p[2].Length > 0)
                        {
                            ns[index].Add(norms[(int.Parse(p[2]) - 1) * 3]);
                            ns[index].Add(norms[(int.Parse(p[2]) - 1) * 3 + 1]);
                            ns[index].Add(norms[(int.Parse(p[2]) - 1) * 3 + 2]);
                        }
                    }
                }
                else if (ln.StartsWith("o"))
                {
                    index++;
                    vs.Add(new List<float>());
                    ns.Add(new List<float>());
                    us.Add(new List<float>());
                }
                else if (ln.StartsWith("mtllib"))
                {

                }
                else if (ln.StartsWith("usemtl"))
                {

                }
            }


            EngineObject[] eObjs = new EngineObject[us.Count];
            for (int i = 0; i < us.Count; i++)
            {
                eObjs[i].SetNormals(0, ns[i].ToArray(), false, 3);
                eObjs[i].SetVertices(0, vs[i].ToArray(), false, 3);
                eObjs[i].SetUVs(0, us[i].ToArray(), false, 2);
                eObjs[i].IndexCount = vs[i].Count / 3;
            }

            return eObjs;
        }

    }
}
