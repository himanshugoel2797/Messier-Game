﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Messier.Graphics
{
    public class GPUBuffer : IDisposable
    {
        internal int id;
        internal BufferTarget target;
        
        public GPUBuffer(BufferTarget target)
        {
            id = GL.GenBuffer();
            this.target = target;
        }

        public void BufferData(int offset, float[] data, BufferUsageHint hint)
        {
            GPUStateMachine.BindBuffer(target, id);

            GL.BufferData(target, (IntPtr)(sizeof(float) * data.Length), data, hint);

            GPUStateMachine.UnbindBuffer(target);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                GL.DeleteBuffer(id);
                id = 0;
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        ~GPUBuffer()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}