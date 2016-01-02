using Messier.Graphics.Input.LowLevel;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        static GPUBuffer curIndices;
        static GameWindow game;
        static List<Texture> textures;
        static List<Tuple<GPUBuffer, int, int>> feedbackBufs;
        static PrimitiveType feedbackPrimitive;

        public static Size WindowSize
        {
            get
            {
                return new Size(game.Width, game.Height);
            }
            set
            {
                game.Width = value.Width;
                game.Height = value.Height;
            }
        }
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
        public static Action Cleanup { get; set; }
        public static OpenTK.Input.KeyboardDevice Keyboard { get { return game.Keyboard; } }
        public static OpenTK.Input.MouseDevice Mouse { get { return game.Mouse; } }
        public static int PatchCount
        {
            set
            {
                GL.PatchParameter(PatchParameterInt.PatchVertices, value);
            }
        }

        static bool wframe = false;
        public static bool Wireframe
        {
            set
            {
                wframe = value;
                GL.PolygonMode(MaterialFace.FrontAndBack, value?PolygonMode.Line:PolygonMode.Fill);
            }
            get
            {
                return wframe;
            }
        }

        static bool aEnabled = false;
        public static bool AlphaEnabled
        {
            get
            {
                return aEnabled;
            }
            set
            {
                aEnabled = value;
                if(aEnabled)
                {
                    GL.Enable(EnableCap.Blend);
                    GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                }
                else
                {
                    GL.Disable(EnableCap.Blend);
                }
            }
        }

        static CullFaceMode cullMode = CullFaceMode.Back;
        public static CullFaceMode CullMode
        {
            get
            {
                return cullMode;
            }
            set
            {
                cullMode = value;
                GL.CullFace(cullMode);
            }
        }

        static bool cullEnabled = false;
        public static bool CullEnabled
        {
            get
            {
                return cullEnabled;
            }
            set
            {
                cullEnabled = value;
                if (cullEnabled)
                    GL.Enable(EnableCap.CullFace);
                else
                    GL.Disable(EnableCap.CullFace);
            }
        }

        static bool depthTestEnabled = false;
        public static bool DepthTestEnabled
        {
            get
            {
                return depthTestEnabled;
            }
            set
            {
                depthTestEnabled = value;
                if(depthTestEnabled)
                {
                    GL.Enable(EnableCap.DepthTest);
                    GL.DepthFunc(DepthFunction.Lequal);
                }
                else
                {
                    GL.Disable(EnableCap.DepthTest);
                }
            }
        }

        static GraphicsDevice()
        {
            game = new GameWindow((int)(16f/9f * 540), 540);
            game.Resize += Window_Resize;
            game.Load += Game_Load;
            game.RenderFrame += Game_RenderFrame;
            game.UpdateFrame += Game_UpdateFrame;

            curVarray = null;
            curProg = null;
            curFramebuffer = Framebuffer.Default;
            textures = new List<Texture>();
            feedbackBufs = new List<Tuple<GPUBuffer, int, int>>();
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
            GL.Enable(EnableCap.DepthClamp);
            GL.Enable(EnableCap.TextureCubeMapSeamless);
            Load?.Invoke();
        }

        private static void Window_Resize(object sender, EventArgs e)
        {
            GPUStateMachine.SetViewport(0, 0, game.Width, game.Height);
            InputLL.SetWinXY(game.Location.X, game.Location.Y, game.ClientSize.Width, game.ClientSize.Height);
        }

        public static void SetViewport(int x, int y, int width, int height)
        {
            GL.Viewport(x, y, width, height);
        }

        public static void SetShaderProgram(ShaderProgram prog)
        {
            curProg = prog;
        }

        public static void SetVertexArray(VertexArray varray)
        {
            curVarray = varray;
        }

        public static void SetIndexBuffer(GPUBuffer indices)
        {
            if (indices.target != BufferTarget.ElementArrayBuffer) throw new ArgumentException("Argument must be an index buffer!");
            curIndices = indices;
        }

        public static void SetFramebuffer(Framebuffer framebuf)
        {
            curFramebuffer = framebuf;
        }

        public static void SetTexture(int slot, Texture tex)
        {
            while (textures.Count <= slot)
                textures.Add(null);

            textures[slot] = tex;
        }

        public static void SetFeedbackBuffer(int slot, GPUBuffer buf)
        {
            SetFeedbackBuffer(slot, buf, 0, buf.size);
        }

        public static void SetFeedbackBuffer(int slot, GPUBuffer buf, int offset, int size)
        {
            while (feedbackBufs.Count <= slot)
                feedbackBufs.Add(null);

            feedbackBufs[slot] = new Tuple<GPUBuffer, int, int>(buf, offset, size);
        }


        public static void SetFeedbackPrimitive(PrimitiveType type)
        {
            if (feedbackPrimitive == PrimitiveType.Points || feedbackPrimitive == PrimitiveType.Lines || feedbackPrimitive == PrimitiveType.Triangles) feedbackPrimitive = type;
            else throw new Exception();
        }

        public static void Draw(PrimitiveType type, int first, int count)
        {
            if (curVarray == null) return;
            if (curProg == null) return;
            if (curFramebuffer == null) return;

            for (int i = 0; i < textures.Count; i++) GPUStateMachine.BindTexture(i, textures[i].texTarget, textures[i].id);
            for (int i = 0; i < feedbackBufs.Count; i++) GPUStateMachine.BindFeedbackBuffer(BufferTarget.TransformFeedbackBuffer, feedbackBufs[i].Item1.id, i, (IntPtr)feedbackBufs[i].Item2, (IntPtr)feedbackBufs[i].Item3);

            

            GPUStateMachine.BindFramebuffer(curFramebuffer.id);
            if(feedbackBufs.Count > 0)GL.BeginTransformFeedback((TransformFeedbackPrimitiveType)feedbackPrimitive);

            GL.UseProgram(curProg.id);
            GPUStateMachine.BindVertexArray(curVarray.id);
            if(curIndices != null)GPUStateMachine.BindBuffer(BufferTarget.ElementArrayBuffer, curIndices.id);

            if (curIndices != null) GL.DrawElements(type, count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            else GL.DrawArrays(type, first, count);

            if(curIndices != null)GPUStateMachine.UnbindBuffer(BufferTarget.ElementArrayBuffer);
            GPUStateMachine.UnbindVertexArray();
            GL.UseProgram(0);

            if (feedbackBufs.Count > 0) GL.EndTransformFeedback();
            GPUStateMachine.UnbindFramebuffer();

            for (int i = 0; i < feedbackBufs.Count; i++) GPUStateMachine.UnbindFeedbackBuffer(BufferTarget.TransformFeedbackBuffer, i);
            for (int i = 0; i < textures.Count; i++) GPUStateMachine.UnbindTexture(i, textures[i].texTarget);

            curVarray = null;
            curProg = null;
            curIndices = null;
            textures.Clear();
            feedbackBufs.Clear();
            curFramebuffer = Framebuffer.Default;
        }

        public static void Clear()
        {
            // render graphics
            GL.ClearColor(0, 0.5f, 1.0f, 0.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public static void SaveTexture(Texture t, string file)
        {
#if DEBUG
            Bitmap bmp = new Bitmap(t.Width, t.Height);
            System.Drawing.Imaging.BitmapData bmpData;

            bmpData = bmp.LockBits(new Rectangle(0, 0, t.Width, t.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GPUStateMachine.BindTexture(0, t.texTarget, t.id);
            GL.GetTexImage(t.texTarget, 0, PixelFormat.Bgra, PixelType.UnsignedInt8888Reversed, bmpData.Scan0);
            GPUStateMachine.UnbindTexture(0, t.texTarget);
            bmp.UnlockBits(bmpData);
        
            bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
            bmp.Save(file);
            bmp.Dispose();
#endif
        }

    }
}
