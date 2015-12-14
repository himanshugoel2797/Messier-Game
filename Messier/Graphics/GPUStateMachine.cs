using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics
{
    public static class GPUStateMachine
    {
        static Dictionary<BufferTarget, Stack<int>> boundBuffers;
        static Stack<int> vertexArrays, framebuffers;
        static List<Dictionary<TextureTarget, Stack<int>>> boundTextures;

        static GPUStateMachine()
        {
            boundBuffers = new Dictionary<BufferTarget, Stack<int>>();
            boundBuffers[BufferTarget.ArrayBuffer] = new Stack<int>();
            boundBuffers[BufferTarget.ArrayBuffer].Push(0);

            boundBuffers[BufferTarget.ElementArrayBuffer] = new Stack<int>();
            boundBuffers[BufferTarget.ElementArrayBuffer].Push(0);

            boundTextures = new List<Dictionary<TextureTarget, Stack<int>>>();
            for (int i = 0; i < 8; i++)
            {
                boundTextures.Add(new Dictionary<TextureTarget, Stack<int>>());
                boundTextures[i][TextureTarget.Texture2D] = new Stack<int>();
                boundTextures[i][TextureTarget.Texture2D].Push(0);
            }

            vertexArrays = new Stack<int>();
            vertexArrays.Push(0);

            framebuffers = new Stack<int>();
            framebuffers.Push(0);
        }

        #region Buffer object state
        public static void BindBuffer(BufferTarget target, int id)
        {
            if (boundBuffers[target].Count == 0) boundBuffers[target].Push(0);

            if (boundBuffers[target].Peek() != id || id == 0) GL.BindBuffer(target, id);
            boundBuffers[target].Push(id);
        }

        public static void UnbindBuffer(BufferTarget target)
        {
            boundBuffers[target].Pop();
            BindBuffer(target, boundBuffers[target].Pop());
        }
        #endregion

        #region Texture state
        public static void BindTexture(int index, TextureTarget target, int id)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + index);
            if (boundTextures[index][target].Count == 0) boundTextures[index][target].Push(0);

            if (boundTextures[index][target].Peek() != id || id == 0) GL.BindTexture(target, id);
            boundTextures[index][target].Push(id);
        }

        public static void UnbindTexture(int index, TextureTarget target)
        {
            boundTextures[index][target].Pop();
            BindTexture(index, target, boundTextures[index][target].Pop());
        }
        #endregion

        #region Vertex Array State
        public static void BindVertexArray(int id)
        {
            if (vertexArrays.Count == 0) vertexArrays.Push(0);

            if (vertexArrays.Peek() != id || id == 0) GL.BindVertexArray(id);
            vertexArrays.Push(id);
        }

        public static void UnbindVertexArray()
        {
            vertexArrays.Pop();
            BindVertexArray(vertexArrays.Pop());
        }
        #endregion

        #region Framebuffer State
        public static void BindFramebuffer(int id)
        {
            if (framebuffers.Count == 0) framebuffers.Push(0);

            if (framebuffers.Peek() != id || id == 0) GL.BindFramebuffer(FramebufferTarget.Framebuffer, id);
            framebuffers.Push(id);
        }

        public static void UnbindFramebuffer()
        {
            framebuffers.Pop();
            BindFramebuffer(framebuffers.Pop());
        }
        #endregion

        #region Viewport State
        static Vector4 viewport;
        public static void SetViewport(int x, int y, int width, int height)
        {
            viewport.X = x;
            viewport.Y = y;
            viewport.Z = width;
            viewport.W = height;
            GL.Viewport(x, y, width, height);
        }
        #endregion
    }
}
