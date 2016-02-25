using Messier.Graphics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public class EngineObject : IDisposable
    {
        public BoundingBox Bounds { get; set; }

        internal VertexArray mesh;
        internal GPUBuffer verts, indices, uvs, norms;
        internal Dictionary<int, GPUBuffer>[] animFrames;
        internal List<Texture> textures;

        public int IndexCount { get; set; }
        public int MaterialIndex { get; set; }
        public int[] CurrentFrame { get; set; }
        public int[] FrameCount { get; set; }

        public Bone[] Bones { get; set; }
        public float[] Vertices { get; set; }
        public Dictionary<string, Animation> SkeletalAnimations { get; set; }
        public string CurrentSkeletalAnimationName { get; set; }
        public int CurrentSkeletalAnimationFrame { get; set; }

        public const int MaximumAnimationChannels = 4;

        private bool lock_changes = false;

        public EngineObject()
        {
            animFrames = new Dictionary<int, GPUBuffer>[MaximumAnimationChannels];
            CurrentFrame = new int[MaximumAnimationChannels];
            FrameCount = new int[MaximumAnimationChannels];

            for (int i = 0; i < MaximumAnimationChannels; i++)
                animFrames[i] = new Dictionary<int, GPUBuffer>();

            mesh = new VertexArray();
            verts = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            indices = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ElementArrayBuffer);
            uvs = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            norms = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer);
            textures = new List<Texture>();
        }

        public EngineObject(EngineObject src, bool lockChanges)
        {
            mesh = src.mesh;
            verts = src.verts;
            indices = src.indices;
            uvs = src.uvs;
            norms = src.norms;
            IndexCount = src.IndexCount;
            animFrames = src.animFrames;
            Bones = src.Bones;
            CurrentFrame = src.CurrentFrame;
            FrameCount = src.FrameCount;
            MaterialIndex = src.MaterialIndex;
            SkeletalAnimations = src.SkeletalAnimations;
            CurrentSkeletalAnimationName = src.CurrentSkeletalAnimationName;
            CurrentSkeletalAnimationFrame = src.CurrentSkeletalAnimationFrame;

            Vertices = new float[src.Vertices.Length];
            Array.Copy(src.Vertices, Vertices, Vertices.Length);

            textures = new List<Texture>();
            lock_changes = lockChanges;
        }

        public Matrix4 UpdateVertices(string nodeName, Matrix4 transform)
        {
            Bone b = Bones.Single(c => c.Name == nodeName);
            var anim = SkeletalAnimations[CurrentSkeletalAnimationName].GetFrame(CurrentSkeletalAnimationFrame);

            Matrix4 boneTrans = Matrix4.CreateFromQuaternion(anim.Rotation) * Matrix4.CreateTranslation(anim.Translation) * Matrix4.CreateScale(anim.Scale);

            var t = transform * b.Offset * boneTrans;

            for (int i = 0; i < b.Weights.Count; i++)
            {
                var tmp = new Vector3(Vertices[b.Weights[i].Index * 3], Vertices[b.Weights[i].Index * 3 + 1], Vertices[b.Weights[i].Index * 3 + 2]);
                tmp = Vector3.Transform(tmp, t * b.Weights[i].Weight);
                Vertices[b.Weights[i].Index * 3] = tmp.X;
                Vertices[b.Weights[i].Index * 3 + 1] = tmp.Y;
                Vertices[b.Weights[i].Index * 3 + 2] = tmp.Z;
            }

            SetVertices(0, Vertices, true, 3);

            return t;
        }

        public void SetVertices(int offset, float[] vertices, bool Dynamic, int elementCount)
        {
            if (lock_changes) return;
            Vertices = new float[vertices.Length];
            Array.Copy(vertices, Vertices, Vertices.Length);
            verts.BufferData(offset, vertices, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(0, verts, elementCount, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
        }

        public void SetAnimationFrame(int animChannel, int frame, GPUBuffer frameVerts)
        {
            if (lock_changes) return;
            if (animChannel >= MaximumAnimationChannels) throw new ArgumentOutOfRangeException(nameof(animChannel));
            animFrames[animChannel][frame] = frameVerts;
        }

        public void SetIndices(int offset, uint[] i, bool Dynamic)
        {
            if (lock_changes) return;
            IndexCount = i.Length;
            indices.BufferData(offset, i, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
        }

        public void SetIndices(EngineObject src)
        {
            if (src == null) throw new ArgumentNullException();
            this.indices = src.indices;
            this.IndexCount = this.indices.dataLen;
        }

        public void SetUVs(int offset, float[] uv, bool Dynamic, int elementCount)
        {
            if (lock_changes) return;
            uvs.BufferData(offset, uv, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(1, uvs, elementCount, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
        }

        public void SetNormals(int offset, float[] n, bool Dynamic, int elementCount)
        {
            if (lock_changes) return;
            norms.BufferData(offset, n, Dynamic ? OpenTK.Graphics.OpenGL4.BufferUsageHint.DynamicDraw : OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            mesh.SetBufferObject(2, norms, elementCount, OpenTK.Graphics.OpenGL4.VertexAttribPointerType.Float);
        }

        public void SetTexture(int slot, Texture tex)
        {
            while (textures.Count <= slot)
            {
                textures.Add(null);
            }
            textures[slot] = tex;
        }

        //Where interval is the time elapsed since the previous frame in milliseconds
        public void CalculateNextFrame(double interval)
        {
            //determine the number of frames that should have elapsed in the given time

            //Increment the frame counters appropriately
        }

        public void Bind()
        {
            for (int i = 0; i < textures.Count; i++)
                GraphicsDevice.SetTexture(i, textures[i]);

            //Determine which two buffers should be bound for each channel based off of the current frame, then specify the interpolation weight

            if (indices.dataLen != 0) GraphicsDevice.SetIndexBuffer(indices);
            GraphicsDevice.SetVertexArray(mesh);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EngineObject() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
