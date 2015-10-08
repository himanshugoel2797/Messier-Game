﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Messier.Graphics
{
    public class ShaderProgram : IDisposable
    {
        internal int id;

        public ShaderProgram(params ShaderSource[] shaders)
        {
            id = GL.CreateProgram();

            for (int i = 0; i < shaders.Length; i++)
            {
                GL.AttachShader(id, shaders[i].id);
            }
            GL.LinkProgram(id);

            int status = 0;
            GL.GetProgram(id, GetProgramParameterName.LinkStatus, out status);
            if (status == 0)
            {
                //Retrieve the errors
                string error = "";
                GL.GetProgramInfoLog(id, out error);

                GL.DeleteProgram(id);

                Console.WriteLine(error);
                throw new Exception("Shader linking error: " + error);
            }

            for (int i = 0; i < shaders.Length; i++)
            {
                GL.DetachShader(id, shaders[i].id);
            }
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
                GL.DeleteProgram(id);
                id = 0;

                disposedValue = true;
            }
        }

        ~ShaderProgram()
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
