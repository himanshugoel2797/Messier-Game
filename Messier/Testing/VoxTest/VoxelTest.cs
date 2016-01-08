using Messier.Engine;
using Messier.Graphics;
using Messier.Graphics.Cameras;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
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
                Color = Vector4.UnitY * 0.7f + Vector4.UnitW,
                Visible = true
            };

            v[2] = new VoxelTypeMap.VoxelTypeData()
            {
                Color = Vector4.UnitX + Vector4.UnitW,
                Visible = true
            };

            BlockManager man = new BlockManager();
            man.Side = 32;
            man.VoxelTypes = v;

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

                ShaderSource vShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Testing/VoxTest/vertex.glsl");
                ShaderSource fShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Testing/VoxTest/fragment.glsl");

                prog = new ShaderProgram(vShader, fShader);
                prog.Set("materialColors", 0);

            };

            GraphicsDevice.Update += (e) =>
            {
                context.Update(e);

                //World *= Matrix4.CreateRotationY(0.01f);

                prog.Set("World", Matrix4.Identity);
                prog.Set("View", context.View);
                prog.Set("Proj", context.Projection);
                prog.Set("Fcoef", (float)(2.0f / Math.Log(1000001) / Math.Log(2)));
                prog.Set("lightDir", new Vector3(5, 10, 5).Normalized());

            };


            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.Clear();

                for (int k = -2; k < 0; k++)
                {
                    Vector3 a = new Vector3();
                    a.Y = k * man.Side;
                    for (int i = -5; i < 5; i++)
                    {
                        a.Z = i * man.Side;
                        for (int j = -5; j < 5; j++)
                        {
                            a.X = j * man.Side;
                            Vector3 dir = (context.Camera as FirstPersonCamera).Direction;
                            if (Vector3.Dot(dir.Normalized(), a.Normalized()) >= -0.3)
                            {
                                //Chunk c = man.Draw(-Vector3.UnitY * 123, out World);
                                Chunk c = man.Draw(context.Camera.Position + a, out World);
                                if (c.ChunkReady)
                                {
                                    c.Bind();
                                    prog.Set("World", World);

                                    prog.Set("range1", c.NormalOffsets[1]);
                                    prog.Set("range2", c.NormalOffsets[2]);
                                    prog.Set("range3", c.NormalOffsets[3]);
                                    prog.Set("range4", c.NormalOffsets[4]);
                                    prog.Set("range5", c.NormalOffsets[5]);
                                    GraphicsDevice.SetShaderProgram(prog);


                                    GraphicsDevice.Draw(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, c.NormalGroupSizes[6]);
                                }
                            }
                        }
                    }
                }

                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "Voxel Test";
            GraphicsDevice.Run(60, 0);
            if (GraphicsDevice.Cleanup != null) GraphicsDevice.Cleanup();
        }
    }
}
