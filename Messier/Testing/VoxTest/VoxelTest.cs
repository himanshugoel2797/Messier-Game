using Messier.Engine;
using Messier.Graphics;
using Messier.Graphics.Cameras;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
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

            Chunk c = new Chunk();
            c.InitDataStore();
            c.VoxelMap = v;
            c.MaterialMap[0] = 0;
            c.MaterialMap[1] = 1;


            //Rendering stuff
            VertexArray vArray = null;
            GPUBuffer verts = null;
            ShaderProgram prog = null;

            GraphicsDevice.Load += () =>
            {
                GraphicsDevice.CullEnabled = !false;
                GraphicsDevice.CullMode = OpenTK.Graphics.OpenGL4.CullFaceMode.Back;
                GraphicsDevice.Wireframe = true;
                Random rng = new Random(0);
                int n = 0;

                for(int x = 0; x < Chunk.Side; x++)
                    for (int y = 0; y < Chunk.Side; y++)
                        for (int z = 0; z < Chunk.Side; z++)
                        {
                            c[x, y, z] = 1;
                           // c[x, y, z] = (byte)((x + y + z) % 2);
                        }


                verts = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
                c.GenerateMesh(verts);

                vArray = new VertexArray();
                vArray.SetBufferObject(0, verts, 4, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);

                ShaderSource vShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Testing/VoxTest/vertex.glsl");
                ShaderSource fShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Testing/VoxTest/fragment.glsl");

                prog = new ShaderProgram(vShader, fShader);
            };

            GraphicsDevice.Update += (e) =>
            {
                context.Update(e);

                prog.Set("World", World);
                prog.Set("View", context.View);
                prog.Set("Proj", context.Projection);
                prog.Set("Fcoef", (float)(2.0f / Math.Log(1000001) / Math.Log(2)));
            };

            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.Clear();

                for (int i = 0; i < 1; i++)
                {
                    GraphicsDevice.SetVertexArray(vArray);
                    GraphicsDevice.SetShaderProgram(prog);
                    GraphicsDevice.Draw(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, 600000);
                }
                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "Voxel Test";
            GraphicsDevice.Run(60, 60);
            if (GraphicsDevice.Cleanup != null) GraphicsDevice.Cleanup();
        }
    }
}
