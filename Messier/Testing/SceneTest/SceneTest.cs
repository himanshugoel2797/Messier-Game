using Messier.Engine;
using Messier.Engine.SceneGraph;
using Messier.Graphics;
using Messier.Graphics.Cameras;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Testing.SceneTest
{
    public class SceneTest
    {
        public static void Run()
        {
            ShaderProgram prog = null, prog2 = null;
            Scene s = null;
            GBuffer gbuf = null;
            BitmapTextureSource bmpSrc = new BitmapTextureSource("grassTex.jpg", 0);
            GraphicsContext context = new GraphicsContext();
            context.Camera = new FirstPersonCamera(new OpenTK.Vector3(4, 3, 3), OpenTK.Vector3.UnitY);
            EngineObject fsq = null;
            Texture t = null;


            GraphicsDevice.Load += () =>
            {
                s = AssimpLoader.LoadFile("test.fbx");

                gbuf = new GBuffer(960, 540);

                prog = new ShaderProgram(ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Shaders/vertex.glsl"),
                                             ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Shaders/fragment.glsl"));

                prog2 = new ShaderProgram(ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Shaders/vertex.glsl"),
                                             ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Shaders/fragment.glsl"));


                prog.Set("World", Matrix4.Identity);
                prog.Set("View", Matrix4.Identity);
                prog.Set("Proj", Matrix4.Identity);

                t = new Texture();
                t.SetData(bmpSrc);

                fsq = Messier.Graphics.Prefabs.FullScreenQuadFactory.Create();
                fsq.SetTexture(0, gbuf.Diffuse);

                PC2Parser.Load("test.pc2", 0, s.EngineObjects[0]);

                //Scene.SceneShader = prog;
                Scene.SceneShader = gbuf.Shader;
                Scene.SceneShader.Set("World", Matrix4.Identity);
                Scene.SceneShader.Set("View", Matrix4.Identity);
                Scene.SceneShader.Set("Proj", Matrix4.Identity);
            };

            GraphicsDevice.Update += (e) =>
             {
                 GraphicsDevice.Wireframe = !false;
                 GraphicsDevice.AlphaEnabled = true;
                 GraphicsDevice.DepthTestEnabled = true;
                 GraphicsDevice.CullEnabled = false;
                 GraphicsDevice.CullMode = CullFaceMode.Back;

                 context.Update(e);

                 prog2.Set("World", Matrix4.Identity);
                 prog2.Set("View", context.View);
                 prog2.Set("Proj", context.Projection);

             };

            GraphicsDevice.Render += (e) =>
            {
                gbuf.Bind();
                GraphicsDevice.SetViewport(0, 0, 960, 540);
                GraphicsDevice.Clear();
                s.Draw(context);

                GraphicsDevice.SetFramebuffer(Framebuffer.Default);
                GraphicsDevice.SetViewport(0, 0, GraphicsDevice.WindowSize.Width, GraphicsDevice.WindowSize.Height);
                GraphicsDevice.Wireframe = false;
                GraphicsDevice.AlphaEnabled = true;
                GraphicsDevice.DepthTestEnabled = !true;
                GraphicsDevice.CullEnabled = false;
                GraphicsDevice.CullMode = CullFaceMode.Back;
                fsq.Bind();
                GraphicsDevice.SetShaderProgram(prog);
                prog.Set("img", 0);
                GraphicsDevice.Draw(PrimitiveType.Triangles, 0, fsq.IndexCount);

                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "Scene Test";
            GraphicsDevice.Run(60, 60);
            if (GraphicsDevice.Cleanup != null) GraphicsDevice.Cleanup();
        }
    }
}
