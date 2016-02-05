using Assimp;
using Messier.Engine.SceneGraph;
using Messier.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public class PC2Parser
    {
        private struct PC2File
        {
            public char[] Sig;
            public int version;
            public int numPoints;
            public float startFrame;
            public float sampleRate;
            public int numSamples;
            public float[][] verts;
        }

        public static void Load(string file, int channel, EngineObject dst)
        {
            PC2File f = new PC2File();
            List<uint> indices = new List<uint>();
            StreamReader fr = new StreamReader(System.IO.Path.ChangeExtension(file, "obj"));

            string ln = "";
            while (!ln.StartsWith("f")) ln = fr.ReadLine();

            string[] ind;

            do
            {
                ind = ln.Split(' ');
                indices.Add(uint.Parse(ind[1].Split('/')[0]) - 1);
                indices.Add(uint.Parse(ind[2].Split('/')[0]) - 1);
                indices.Add(uint.Parse(ind[3].Split('/')[0]) - 1);

                ln = fr.ReadLine();
            }
            while (!fr.EndOfStream && ln.StartsWith("f"));

            ind = ln.Split(' ');
            indices.Add(uint.Parse(ind[1].Split('/')[0]) - 1);
            indices.Add(uint.Parse(ind[2].Split('/')[0]) - 1);
            indices.Add(uint.Parse(ind[3].Split('/')[0]) - 1);


            using (FileStream s = File.OpenRead(file))
            {
                BinaryReader r = new BinaryReader(s);

                f.Sig = r.ReadChars(12);
                f.version = r.ReadInt32();
                f.numPoints = r.ReadInt32();
                f.startFrame = r.ReadSingle();
                f.sampleRate = r.ReadSingle();
                f.numSamples = r.ReadInt32();

                f.verts = new float[f.numSamples][];

                for (int i = 0; i < f.numSamples; i++)
                {
                    f.verts[i] = new float[f.numPoints * 3];

                    for (int j = 0; j < f.numPoints; j++)
                    {
                        f.verts[i][j * 3] = r.ReadSingle();
                        f.verts[i][(j * 3) + 1] = r.ReadSingle();
                        f.verts[i][(j * 3) + 2] = r.ReadSingle();
                    }
                }
            }

            float[][] verts = new float[f.numSamples][];
            for (int i = 0; i < f.numSamples; i++)
            {
                verts[i] = new float[indices.Count * 3];
                for (int j = 0; j < indices.Count; j++)
                {
                    verts[i][(j * 3)] = f.verts[i][(indices[j] * 3)];
                    verts[i][(j * 3) + 1] = f.verts[i][(indices[j] * 3) + 1];
                    verts[i][(j * 3) + 2] = f.verts[i][(indices[j] * 3) + 2];
                }
            }

            for (int i = 0; i < f.numSamples; i++)
            {
                GPUBuffer buf = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                buf.BufferData(0, verts[i], OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
                dst.SetAnimationFrame(channel, (int)(f.sampleRate * i), buf);
            }
        }
    }
}
