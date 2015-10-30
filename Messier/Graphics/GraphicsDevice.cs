using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics
{
    public class GraphicsDevice
    {
        static VertexArray curVarray;
        static ShaderProgram curProg;
        static Framebuffer curFramebuffer;
        static GameWindow game;

        public static string Name {
            get
            {
                return game.Title;
            }
            set
            {
                game.Title = value;
            }
        }
        public static Action Load { get; set; }
        public static Action<double> Render { get; set; }
        public static Action<double> Update { get; set; }
        public static OpenTK.Input.KeyboardDevice Keyboard { get { return game.Keyboard; } }

        static GraphicsDevice()
        {
            game = new GameWindow();
            game.Resize += Window_Resize;
            game.Load += Game_Load;
            game.RenderFrame += Game_RenderFrame;
            game.UpdateFrame += Game_UpdateFrame;

            curVarray = null;
            curProg = null;
            curFramebuffer = Framebuffer.Default;
        }

        public static void Run(double ups, double fps)
        {
            game.Run(ups, fps);
        }

        public static void SwapBuffers()
        {
            game.SwapBuffers();
        }

        public static void Exit()
        {
            game.Exit();
        }

        private static void Game_UpdateFrame(object sender, FrameEventArgs e)
        {
            Update?.Invoke(e.Time);
        }

        private static void Game_RenderFrame(object sender, FrameEventArgs e)
        {
            Render?.Invoke(e.Time);
        }

        private static void Game_Load(object sender, EventArgs e)
        {
            Load?.Invoke();
        }

        private static void Window_Resize(object sender, EventArgs e)
        {
            GPUStateMachine.SetViewport(0, 0, game.Width, game.Height);
        }

        public static void SetShaderProgram(ShaderProgram prog)
        {
            curProg = prog;
        }

        public static void SetVertexArray(VertexArray varray)
        {
            curVarray = varray;
        }

        public static void Draw(PrimitiveType type, int first, int count)
        {
            if (curVarray == null) return;
            if (curProg == null) return;
            if (curFramebuffer == null) return;

            GPUStateMachine.BindFramebuffer(curFramebuffer.id);
            GL.UseProgram(curProg.id);
            curProg.BindTextures();
            GPUStateMachine.BindVertexArray(curVarray.id);

            GL.DrawArrays(type, first, count);

            GPUStateMachine.UnbindVertexArray();
            curProg.UnbindTextures();
            GL.UseProgram(0);
            GPUStateMachine.UnbindFramebuffer();

            curVarray = null;
            curProg = null;
            curFramebuffer = Framebuffer.Default;
        }

        public static void Clear()
        {
            // render graphics
            GL.ClearColor(0, 0.5f, 1.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
