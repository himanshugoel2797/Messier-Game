using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace Messier.Graphics
{
    public class CubeMapTextureSource : ITextureSource
    {
        public enum CubeMapFace{
            PositiveX, PositiveY, PositiveZ,
            NegativeX, NegativeY, NegativeZ
        };

        private ITextureSource texSrc;
        private CubeMapFace curFace;

        public CubeMapTextureSource(CubeMapFace face, ITextureSource tex)
        {
            curFace = face;
            texSrc = tex;
        }

        public int GetDepth()
        {
            return 0;
        }

        public int GetDimensions()
        {
            return 2;
        }

        public PixelFormat GetFormat()
        {
            return texSrc.GetFormat();
        }

        public int GetHeight()
        {
            return texSrc.GetHeight();
        }

        public PixelInternalFormat GetInternalFormat()
        {
            return texSrc.GetInternalFormat();
        }

        public int GetLevels()
        {
            return texSrc.GetLevels();
        }

        public IntPtr GetPixelData()
        {
            return texSrc.GetPixelData();
        }


        private int targetCallNum = 0;
        public TextureTarget GetTextureTarget()
        {
            if (targetCallNum++ % 2 == 0) return TextureTarget.TextureCubeMap;
            switch(curFace)
            {
                case CubeMapFace.NegativeX:
                    return TextureTarget.TextureCubeMapNegativeX;
                case CubeMapFace.NegativeY:
                    return TextureTarget.TextureCubeMapNegativeY;
                case CubeMapFace.NegativeZ:
                    return TextureTarget.TextureCubeMapNegativeZ;
                case CubeMapFace.PositiveX:
                    return TextureTarget.TextureCubeMapPositiveX;
                case CubeMapFace.PositiveY:
                    return TextureTarget.TextureCubeMapPositiveY;
                case CubeMapFace.PositiveZ:
                    return TextureTarget.TextureCubeMapPositiveZ;
            }
            return TextureTarget.TextureCubeMap;
        }

        public int GetWidth()
        {
            return texSrc.GetWidth();
        }

        PixelType ITextureSource.GetType()
        {
            return texSrc.GetType();
        }
    }
}
