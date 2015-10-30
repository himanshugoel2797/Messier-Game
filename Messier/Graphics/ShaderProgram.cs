﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK;

namespace Messier.Graphics
{
    public class ShaderProgram : IDisposable
    {
        internal int id;
        Texture[] textures;

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

            textures = new Texture[8];
        }

        public void Set(string name, Vector3 vec)
        {
            int loc = GL.GetProgramResourceLocation(id, ProgramInterface.Uniform, name);
            if (loc >= 0) GL.ProgramUniform3(id, loc, vec.X, vec.Y, vec.Z);
        }

        public void Set(string name, Vector4 vec)
        {
            int loc = GL.GetProgramResourceLocation(id, ProgramInterface.Uniform, name);
            if (loc >= 0) GL.ProgramUniform4(id, loc, vec.X, vec.Y, vec.Z, vec.W);
        }

        public void Set(string name, Vector2 vec)
        {
            int loc = GL.GetProgramResourceLocation(id, ProgramInterface.Uniform, name);
            if (loc >= 0) GL.ProgramUniform2(id, loc, vec.X, vec.Y);
        }

        public void Set(string name, Matrix4 vec)
        {
            int loc = GL.GetProgramResourceLocation(id, ProgramInterface.Uniform, name);
            if (loc >= 0) GL.ProgramUniformMatrix4(id, loc, 1, false, new float[] { vec.M11, vec.M12, vec.M13, vec.M14,
                                                                                    vec.M21, vec.M22, vec.M23, vec.M24,
                                                                                    vec.M31, vec.M32, vec.M33, vec.M34,
                                                                                    vec.M41, vec.M42, vec.M43, vec.M44 });
        }

        public void Set(string name, float val)
        {
            int loc = GL.GetProgramResourceLocation(id, ProgramInterface.Uniform, name);
            if (loc >= 0) GL.ProgramUniform1(id, loc, val);
        }

        public void Set(string name, int index, Texture t)
        {
            //TODO this needs changing, this isn't the way to bind textures
            textures[index] = t;

            int loc = GL.GetProgramResourceLocation(id, ProgramInterface.Uniform, name);
            if (loc >= 0) GL.ProgramUniform1(id, loc, index);
        }

        internal void BindTextures()
        {
            for(int i = 0; i < textures.Length; i++)
            {
                if (textures[i] != null) GPUStateMachine.BindTexture(i, textures[i].texTarget, textures[i].id);
            }
        }

        internal void UnbindTextures()
        {
            for (int i = 0; i < textures.Length; i++)
            {
                if (textures[i] != null) GPUStateMachine.UnbindTexture(i, textures[i].texTarget);
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
                if(id != 0)GL.DeleteProgram(id);
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