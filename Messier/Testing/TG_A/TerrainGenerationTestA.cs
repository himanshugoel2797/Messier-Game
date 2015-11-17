using Messier.Engine;
using Messier.Graphics;
using Messier.Graphics.Cameras;
using Messier.Graphics.Prefabs;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Testing.TG_A
{
    public class TerrainGenerationTestA
    {
        public static void Run()
        {
            ShaderProgram prog = null;
            VertexArray varray = null;
            GPUBuffer buf = null, indexBuf = null;
            Texture tex = null;

            GraphicsContext context = new GraphicsContext()
            {
                Camera = new FirstPersonCamera(new Vector3(4, 3, 3), Vector3.UnitY)
            };

            BitmapTextureSource bmpTex;
            bmpTex = new BitmapTextureSource("test.jpg", 0);
            //bmpTex = TextDrawer.CreateWriter("Times New Roman", FontStyle.Regular).Write("Hello ABCDEFGHI!", 200, System.Drawing.Color.White);

            Matrix4 World = Matrix4.Identity;
            float timer = 0;


            GraphicsDevice.Load += () =>
            {
                // setup settings, load textures, sounds
                // GraphicsDevice.Wireframe = true;

                varray = new VertexArray();
                float[] data =
                {     -1, -1, 1,
                       1, -1, 1,
                       0, 1, 1
                };

                uint[] indexData = new uint[]
                {
                    0, 1, 2
                };


                ShaderSource vert = ShaderSource.Load(ShaderType.VertexShader, "Testing/TG_A/vertex.glsl");
                ShaderSource frag = ShaderSource.Load(ShaderType.FragmentShader, "Shaders/fragment.glsl");
                ShaderSource tctrl = ShaderSource.Load(ShaderType.TessControlShader, "Testing/TG_A/tesscontrol.glsl");
                ShaderSource teval = ShaderSource.Load(ShaderType.TessEvaluationShader, "Testing/TG_A/tessdomain.glsl");

                tex = new Texture();
                tex.SetData(bmpTex);
                tex.SetAnisotropicFilter(Texture.MaxAnisotropy);

                prog = new ShaderProgram(vert, tctrl, teval, frag);
                prog.Set("img", 0, tex);
                prog.Set("World", World);

                vert.Dispose();
                frag.Dispose();
                tctrl.Dispose();
                teval.Dispose();

                //buf = new GPUBuffer(BufferTarget.ArrayBuffer);
                //buf.BufferData(0, data, BufferUsageHint.StaticDraw);

                //indexBuf = new GPUBuffer(BufferTarget.ElementArrayBuffer);
                //indexBuf.BufferData(0, indexData, BufferUsageHint.StaticDraw);
                buf = FullScreenQuadFactory.CreateVertices();
                indexBuf = FullScreenQuadFactory.CreateIndices();

                varray.SetBufferObject(0, buf, 3, VertexAttribPointerType.Float);
            };

            GraphicsDevice.Update += (e) =>
            {
                // add game logic, input handling
                if (GraphicsDevice.Keyboard[Key.Escape])
                {
                    GraphicsDevice.Exit();
                }

                context.Update(e);
                context.Projection = Matrix4.Identity;
                context.View = Matrix4.Identity;

                prog.Set("View", context.View);
                prog.Set("Proj", context.Projection);
                prog.Set("eyePos", context.Camera.Position);

                timer += 0.01f;
                prog.Set("timer", timer);
            };

            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.Clear();

                GraphicsDevice.SetIndexBuffer(indexBuf);
                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.SetVertexArray(varray);
                GraphicsDevice.PatchCount = 3;
                GraphicsDevice.Draw(PrimitiveType.Patches, 0, 6);

                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "The Julis Faction";
            // Run the game at 60 updates per second
            GraphicsDevice.Run(60.0, 60.0);
            varray.Dispose();
            buf.Dispose();
            indexBuf.Dispose();
            prog.Dispose();
            tex.Dispose();
            bmpTex.Dispose();
            if(GraphicsDevice.Cleanup != null)GraphicsDevice.Cleanup();
        }
    }
}
