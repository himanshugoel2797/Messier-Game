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

        static GraphicsDevice()
        {
            curVarray = null;
            curProg = null;
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

            GL.UseProgram(curProg.id);
            GPUStateMachine.BindVertexArray(curVarray.id);
            GL.DrawArrays(type, first, count);
            GPUStateMachine.UnbindVertexArray();
            GL.UseProgram(0);

            curVarray = null;
            curProg = null;
        }
    }
}
