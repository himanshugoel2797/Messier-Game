﻿using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics
{
    public class Texture : IDisposable
    {
        internal int id;
        internal PixelInternalFormat internalformat;
        internal TextureTarget texTarget;
        internal PixelFormat format;

        public int Width { get; set; }
        public int Height { get; set; }
        public int Depth { get; set; }
        public int LevelCount { get; set; }


        public Texture()
        {
            id = GL.GenTexture();
        }

        public void SetData(ITextureSource src)
        {
            GPUStateMachine.BindTexture(src.GetTextureTarget(), id);
            switch (src.GetDimensions())
            {
                case 1:
                    GL.TexImage1D(src.GetTextureTarget(), src.GetLevels(), src.GetInternalFormat(), src.GetWidth(), 0, src.GetFormat(), src.GetType(), src.GetPixelData());
                    break;
                case 2:
                    GL.TexImage2D(src.GetTextureTarget(), src.GetLevels(), src.GetInternalFormat(), src.GetWidth(), src.GetHeight(), 0, src.GetFormat(), src.GetType(), src.GetPixelData());
                    break;
                case 3:
                    GL.TexImage3D(src.GetTextureTarget(), src.GetLevels(), src.GetInternalFormat(), src.GetWidth(), src.GetHeight(), src.GetDepth(), 0, src.GetFormat(), src.GetType(), src.GetPixelData());
                    break;
            }

            GL.GenerateMipmap((GenerateMipmapTarget)src.GetTextureTarget());
            GPUStateMachine.UnbindTexture(src.GetTextureTarget());

            this.Width = src.GetWidth();
            this.Height = src.GetHeight();
            this.Depth = src.GetDepth();
            this.LevelCount = src.GetLevels();

            this.format = src.GetFormat();
            this.internalformat = src.GetInternalFormat();
            this.texTarget = src.GetTextureTarget();
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

                GL.DeleteTexture(id);
                id = 0;

                disposedValue = true;
            }
        }

        ~Texture()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion


    }
}
