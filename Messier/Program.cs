using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using Messier.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Messier
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ShaderProgram prog = null;
            VertexArray varray = null;
            GPUBuffer buf = null;
            Texture tex = null;
            BitmapTextureSource bmpTex = new BitmapTextureSource("test.jpg", 0);
            Matrix4 Proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45), 16f/9f, 0.01f, 100f);
            Matrix4 View = Matrix4.LookAt(new Vector3(4,3,3), new Vector3(0,0,0), new Vector3(0, 1, 0));
            Matrix4 World = Matrix4.Identity;


            GraphicsDevice.Load += () =>
            {
                // setup settings, load textures, sounds

                varray = new VertexArray();
                float[] data =
                {     -1, -1, 0,
                       1, -1, 0,
                       0, 1, 0
                };

                ShaderSource vert = ShaderSource.Load(ShaderType.VertexShader, "Shaders/vertex.glsl");
                ShaderSource frag = ShaderSource.Load(ShaderType.FragmentShader, "Shaders/fragment.glsl");
                
                tex = new Texture();
                tex.SetData(bmpTex);

                prog = new ShaderProgram(vert, frag);
                prog.Set("img", 0, tex);
                prog.Set("World", World);
                prog.Set("View", View);
                prog.Set("Proj", Proj);

                vert.Dispose();
                frag.Dispose();

                buf = new GPUBuffer(BufferTarget.ArrayBuffer);
                buf.BufferData(0, data, BufferUsageHint.StaticDraw);

                varray.SetBufferObject(0, buf, 3, VertexAttribPointerType.Float);
            };

            GraphicsDevice.Update += (e) =>
            {
                // add game logic, input handling
                if (GraphicsDevice.Keyboard[Key.Escape])
                {
                    GraphicsDevice.Exit();
                }
            };

            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.Clear();


                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.SetVertexArray(varray);
                GraphicsDevice.Draw(PrimitiveType.Triangles, 0, 3);

                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "The Julis Faction";
            // Run the game at 60 updates per second
            GraphicsDevice.Run(60.0, 60.0);
            varray.Dispose();
            buf.Dispose();
            prog.Dispose();
            tex.Dispose();
            bmpTex.Dispose();
        }
    }
}
