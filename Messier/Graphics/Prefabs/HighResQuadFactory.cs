using Messier.Engine.SceneGraph;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics.Prefabs
{
    public class HighResQuadFactory
    {
        public static EngineObject CreateHighResQuad(int terrainWidth, int terrainHeight)
        {
            Vector4[] vertices = new Vector4[terrainWidth * terrainHeight];
            //Vector2[] uvs = new Vector2[terrainWidth * terrainHeight];
            for (int x = 0; x < terrainWidth; x++)
            {
                for (int y = 0; y < terrainHeight; y++)
                {
                    //vertices.AddRange(new float[] { x, 0, -y });
                    vertices[x + y * terrainWidth] = new Vector4(x, -y, (float)x / (float)terrainWidth, (float)y / (float)terrainHeight);
                }
            }


            uint[] indices = new uint[(terrainWidth - 1) * (terrainHeight - 1) * 6];
            int counter = 0;
            for (int y = 0; y < terrainHeight - 1; y++)
            {
                for (int x = 0; x < terrainWidth - 1; x++)
                {
                    int lowerLeft = x + y * terrainWidth;
                    int lowerRight = (x + 1) + y * terrainWidth;
                    int topLeft = x + (y + 1) * terrainWidth;
                    int topRight = (x + 1) + (y + 1) * terrainWidth;

                    indices[counter++] = (uint)topLeft;
                    indices[counter++] = (uint)lowerRight;
                    indices[counter++] = (uint)lowerLeft;

                    indices[counter++] = (uint)topLeft;
                    indices[counter++] = (uint)topRight;
                    indices[counter++] = (uint)lowerRight;
                }
            }

            List<float> verts = new List<float>();
            List<float> uv = new List<float>();
            List<float> norms = new List<float>();
            for (int i = 0; i < vertices.Length; i++)
            {
                verts.AddRange(new float[] { vertices[i].X, vertices[i].Y, vertices[i].Z, vertices[i].W });
            }

            EngineObject o = new EngineObject();
            o.SetVertices(0, verts.ToArray(), false, 4);
            o.SetIndices(0, indices, false);
            return o;
        }
    }
}
