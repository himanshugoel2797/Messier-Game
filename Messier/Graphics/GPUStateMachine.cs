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
        static Stack<int> vertexArrays;
        static Dictionary<TextureTarget, Stack<int>> boundTextures;

        static GPUStateMachine()
        {
            boundBuffers = new Dictionary<BufferTarget, Stack<int>>();
            boundBuffers[BufferTarget.ArrayBuffer] = new Stack<int>();
            boundBuffers[BufferTarget.ArrayBuffer].Push(0);

            boundTextures = new Dictionary<TextureTarget, Stack<int>>();
            boundTextures[TextureTarget.Texture2D] = new Stack<int>();
            boundTextures[TextureTarget.Texture2D].Push(0);

            vertexArrays = new Stack<int>();
            vertexArrays.Push(0);
        }

        #region Buffer object state
        public static void BindBuffer(BufferTarget target, int id)
        {
            if (boundBuffers[target].Count == 0) boundBuffers[target].Push(0);

            if(boundBuffers[target].Peek() != id)GL.BindBuffer(target, id);
            boundBuffers[target].Push(id);
        }

        public static void UnbindBuffer(BufferTarget target)
        {
            boundBuffers[target].Pop();
            BindBuffer(target, boundBuffers[target].Pop());
        }
        #endregion

        #region Buffer object state
        public static void BindTexture(TextureTarget target, int id)
        {
            if (boundTextures[target].Count == 0) boundTextures[target].Push(0);

            if (boundTextures[target].Peek() != id) GL.BindTexture(target, id);
            boundTextures[target].Push(id);
        }

        public static void UnbindTexture(TextureTarget target)
        {
            boundTextures[target].Pop();
            BindTexture(target, boundTextures[target].Pop());
        }
        #endregion

        #region Vertex Array State
        public static void BindVertexArray(int id)
        {
            if (vertexArrays.Count == 0) vertexArrays.Push(0);

            if (vertexArrays.Peek() != id) GL.BindVertexArray(id);
            vertexArrays.Push(id);
        }

        public static void UnbindVertexArray()
        {
            vertexArrays.Pop();
            BindVertexArray(vertexArrays.Pop());
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
