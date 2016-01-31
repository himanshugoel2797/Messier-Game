using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics
{
    public class GBuffer
    {
        Framebuffer fbuf;
        FramebufferTextureSource norm, spec, diffuse, world;
        Texture diffT, normT, specT, worldT;

        ShaderProgram gbufferShader;

        public ShaderProgram Shader { get { return gbufferShader; } }
        public Texture Diffuse { get { return diffT; } }
        public Texture Normal { get { return normT; } }
        public Texture Specular { get { return specT; } }
        public Texture World { get { return worldT; } }

        public GBuffer(int width, int height)
        {
            diffuse = new FramebufferTextureSource(width, height, 0);
            world = new FramebufferTextureSource(width, height, 0);
            spec = new FramebufferTextureSource(width, height, 0);
            norm = new FramebufferTextureSource(width, height, 0);

            fbuf = new Framebuffer(width, height);

            diffT = new Texture();
            normT = new Texture();
            specT = new Texture();
            worldT = new Texture();

            diffT.SetData(diffuse);
            normT.SetData(norm);
            specT.SetData(spec);
            worldT.SetData(world);

            Texture depth = new Texture();
            DepthTextureSource dTex = new DepthTextureSource(width, height);
            dTex.InternalFormat = OpenTK.Graphics.OpenGL4.PixelInternalFormat.DepthComponent24;

            depth.SetData(dTex);

            fbuf[OpenTK.Graphics.OpenGL4.FramebufferAttachment.ColorAttachment0] = diffT;
            fbuf[OpenTK.Graphics.OpenGL4.FramebufferAttachment.ColorAttachment1] = normT;
            fbuf[OpenTK.Graphics.OpenGL4.FramebufferAttachment.ColorAttachment2] = specT;
            fbuf[OpenTK.Graphics.OpenGL4.FramebufferAttachment.ColorAttachment3] = worldT;
            fbuf[OpenTK.Graphics.OpenGL4.FramebufferAttachment.DepthAttachment] = depth;

            ShaderSource v = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Shaders/GBuffer/vertex.glsl");
            ShaderSource f = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Shaders/GBuffer/fragment.glsl");
            gbufferShader = new ShaderProgram(v, f);
        }

        public void Bind()
        {
            GraphicsDevice.SetFramebuffer(fbuf);
        }
    }
}
