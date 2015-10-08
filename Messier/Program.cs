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
            using (var game = new GameWindow())
            {
                game.Load += (sender, e) =>
                {
                    // setup settings, load textures, sounds
                    game.VSync = VSyncMode.On;

                    varray = new VertexArray();
                    float[] data =
                    { -1, -1, 0,
                       1, -1, 0,
                       0, 1, 0
                    };

                    ShaderSource vert = ShaderSource.Load(ShaderType.VertexShader, "Shaders/vertex.glsl");
                    ShaderSource frag = ShaderSource.Load(ShaderType.FragmentShader, "Shaders/fragment.glsl");

                    prog = new ShaderProgram(vert, frag);

                    vert.Dispose();
                    frag.Dispose();

                    buf = new GPUBuffer(BufferTarget.ArrayBuffer);
                    buf.BufferData(0, data, BufferUsageHint.StaticDraw);

                    varray.SetBufferObject(0, buf, 3, VertexAttribPointerType.Float);
                };

                game.Resize += (sender, e) =>
                {
                    Graphics.GPUStateMachine.SetViewport(0, 0, game.Width, game.Height);
                };

                game.UpdateFrame += (sender, e) =>
                {
                    // add game logic, input handling
                    if (game.Keyboard[Key.Escape])
                    {
                        game.Exit();
                    }
                };

                game.RenderFrame += (sender, e) =>
                {
                    // render graphics
                    GL.ClearColor(0, 0.5f, 1.0f, 0.0f);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GraphicsDevice.SetShaderProgram(prog);
                    GraphicsDevice.SetVertexArray(varray);
                    GraphicsDevice.Draw(PrimitiveType.Triangles, 0, 3);

                    game.SwapBuffers();
                };

                game.Title = "Messier";
                // Run the game at 60 updates per second
                game.Run(60.0);
                varray.Dispose();
                buf.Dispose();
                prog.Dispose();
            }
        }
    }
}
