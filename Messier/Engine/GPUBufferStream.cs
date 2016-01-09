using Messier.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public class GPUBufferStream : IDisposable
    {

        public int Size;
        private int page = 0, frames = 3;

        GPUBuffer buffer;
        IntPtr ptr;

        public GPUBufferStream(int size, int frames)
        {
            this.frames = frames;
            Size = size;
            buffer = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.ArrayBuffer, Size * frames, OpenTK.Graphics.OpenGL4.BufferUsageHint.StreamDraw);
            buffer.MapBuffer(false);
            ptr = buffer.GetPtr();
        }

        public IntPtr GetBufferPtr()
        {
            IntPtr p;
            p = ptr + (Size * page);
            page = (page + 1) % frames;
            return p;
        }

        public int GetOffset()
        {
            return Size * page;
        }

        public GPUBuffer GetBuffer()
        {
            return buffer;
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

                buffer.Dispose();
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~GPUBufferStream() {
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
