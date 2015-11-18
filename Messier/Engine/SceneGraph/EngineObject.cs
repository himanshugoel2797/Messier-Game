using Messier.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public class EngineObject
    {
        public BoundingBox Bounds { get; set; }

        VertexArray mesh;
        GPUBuffer verts, indices, uvs, norms;
        List<Texture> textures;

        public EngineObject()
        {
            mesh = new VertexArray();
            verts = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            indices = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ElementArrayBuffer);
            uvs = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            norms = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            textures = new List<Texture>();
        }

        public void SetVertices(int offset, float[] vertices, bool Dynamic)
        {
            verts.BufferData(offset, vertices, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(0, verts, 3, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
        }

        public void SetIndices(int offset, uint[] i, bool Dynamic)
        {
            indices.BufferData(offset, i, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
        }

        public void SetUVs(int offset, float[] uv, bool Dynamic)
        {
            uvs.BufferData(offset, uv, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(1, uvs, 2, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
        }

        public void SetNormals(int offset, float[] n, bool Dynamic)
        {
            norms.BufferData(offset, n, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(2, norms, 3, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
        }

        public void SetTexture(int slot, Texture tex)
        {
            while (textures.Count <= slot)
            {
                textures.Add(null);
            }
            textures[slot] = tex;
        }

        public void Bind()
        {
            for (int i = 0; i < textures.Count; i++)
                GraphicsDevice.SetTexture(i, textures[i]);

            GraphicsDevice.SetIndexBuffer(indices);
            GraphicsDevice.SetVertexArray(mesh);
        }
    }
}
