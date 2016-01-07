using Messier.Engine;
using Messier.Graphics;
using Messier.Graphics.Cameras;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Testing.VoxTest
{
    public class VoxelTest
    {
        public static void Run()
        {
            GraphicsContext context = new GraphicsContext()
            {
                Camera = new FirstPersonCamera(new Vector3(4, 3, 3), Vector3.UnitY)
            };
            Matrix4 World = Matrix4.Identity;


            VoxelTypeMap v = new VoxelTypeMap();
            v[0] = new VoxelTypeMap.VoxelTypeData()
            {
                Color = Vector4.One,
                Visible = false
            };

            v[1] = new VoxelTypeMap.VoxelTypeData()
            {
                Color = Vector4.One,
                Visible = true
            };

            v[2] = new VoxelTypeMap.VoxelTypeData()
            {
                Color = Vector4.UnitX,
                Visible = true
            };

            Chunk c = new Chunk();
            c.InitDataStore();
            Chunk c0 = new Chunk();
            c0.InitDataStore();

            c.VoxelMap = v;
            c.MaterialMap[0] = 0;
            c.MaterialMap[1] = 1;
            c.MaterialMap[2] = 2;

            c0.VoxelMap = v;
            c0.MaterialMap[0] = 0;
            c0.MaterialMap[1] = 1;
            c0.MaterialMap[2] = 2;

            OpenSimplexNoise s = new OpenSimplexNoise(0);

            //Rendering stuff
            ShaderProgram prog = null;

            GraphicsDevice.Load += () =>
            {
                //GraphicsDevice.Winding = FaceWinding.Clockwise;
                GraphicsDevice.CullMode = OpenTK.Graphics.OpenGL4.CullFaceMode.Back;
                GraphicsDevice.DepthTestEnabled = true;
                GraphicsDevice.Window.KeyUp += (k, e) =>
                {
                    if (e.Key == OpenTK.Input.Key.Z) GraphicsDevice.Wireframe = !GraphicsDevice.Wireframe;
                    else if (e.Key == OpenTK.Input.Key.C) GraphicsDevice.CullEnabled = !GraphicsDevice.CullEnabled;
                };

                Random rng = new Random(0);
                double n = 64;

                v.UpdateBuffers();
                GraphicsDevice.SetBufferTexture(0, v.ColorData);


                for (int x = 0; x < c.Side; x++)
                    for (int y = 0; y < c.Side; y++)
                        for (int z = 0; z < c.Side; z++)
                        {
                            //c[x, y, z] = 1;
                            //if (n++ == 1) c[x, y, z] = 1;
                            //c[x, y, z] = (byte)((x + y + z) % 2);
                            c[x, y, z] = (byte)(s.Evaluate(x / n, y / n, z / n) >= 0.5 ? 0 : rng.Next(1, 3));
                            c0[x, y, z] = (byte)(s.Evaluate((x + c0.Side) / n, y / n, z / n) >= 0.5 ? 0 : rng.Next(1, 3));
                        }

                c.GenerateMesh();
                c0.GenerateMesh();



                ShaderSource vShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Testing/VoxTest/vertex.glsl");
                ShaderSource fShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Testing/VoxTest/fragment.glsl");

                prog = new ShaderProgram(vShader, fShader);
                prog.Set("materialColors", 0);

            };

            GraphicsDevice.Update += (e) =>
            {
                context.Update(e);

                //World *= Matrix4.CreateRotationY(0.01f);

                prog.Set("World", World);
                prog.Set("View", context.View);
                prog.Set("Proj", context.Projection);
                prog.Set("Fcoef", (float)(2.0f / Math.Log(1000001) / Math.Log(2)));
                prog.Set("lightDir", new Vector3(5, 10, 5).Normalized());

            };


            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.Clear();
                c.Bind();
                prog.Set("World", World);

                prog.Set("range1", c.NormalOffsets[1]);
                prog.Set("range2", c.NormalOffsets[2]);
                prog.Set("range3", c.NormalOffsets[3]);
                prog.Set("range4", c.NormalOffsets[4]);
                prog.Set("range5", c.NormalOffsets[5]);
                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.Draw(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, c.NormalGroupSizes[6]);

                c0.Bind();
                prog.Set("World", Matrix4.CreateTranslation(c0.Side * c0.Scale, 0, 0));

                prog.Set("range1", c0.NormalOffsets[1]);
                prog.Set("range2", c0.NormalOffsets[2]);
                prog.Set("range3", c0.NormalOffsets[3]);
                prog.Set("range4", c0.NormalOffsets[4]);
                prog.Set("range5", c0.NormalOffsets[5]);
                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.Draw(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, c0.NormalGroupSizes[6]);

                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "Voxel Test";
            GraphicsDevice.Run(60, 0);
            if (GraphicsDevice.Cleanup != null) GraphicsDevice.Cleanup();
        }
    }
}
