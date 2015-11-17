using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics.Prefabs
{
    public class FullScreenQuadFactory
    {
        static GPUBuffer vbuffer, ibuffer, uvbuffer;

        static FullScreenQuadFactory()
        {
            vbuffer = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            ibuffer = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ElementArrayBuffer);
            uvbuffer = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);

            ibuffer.BufferData(0, new uint[] { 3, 2, 0, 0, 2, 1 }, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            uvbuffer.BufferData(0, new float[] {
                0,1,
                1,1,
                1,0,
                0,0
            }, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);

            vbuffer.BufferData(0, new float[]{
                -1, 1, 0.5f,
                1, 1, 0.5f,
                1, -1,0.5f,
                -1, -1,0.5f
            }, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
        }

        public static GPUBuffer CreateVertices()
        {
            return vbuffer;
        }

        public static GPUBuffer CreateIndices()
        {
            return ibuffer;
        }

        public static GPUBuffer CreateUVs()
        {
            return uvbuffer;
        }
    }
}
