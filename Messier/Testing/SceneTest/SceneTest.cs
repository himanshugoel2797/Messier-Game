using Messier.Engine;
using Messier.Engine.SceneGraph;
using Messier.Graphics;
using Messier.Graphics.Cameras;
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
            ShaderProgram prog = null;
            Scene s = null;
            GBuffer gbuf = null;
            GraphicsContext context = new GraphicsContext();
            context.Camera = new FirstPersonCamera(new OpenTK.Vector3(4, 3, 3), OpenTK.Vector3.UnitY);
            EngineObject fsq = null;


            GraphicsDevice.Load += () =>
            {

                s = AssimpLoader.LoadFile("test.fbx");

                gbuf = new GBuffer(960, 540);

                prog = new ShaderProgram(ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Shaders/vertex.glsl"),
                                             ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Shaders/fragment.glsl"));

                prog.Set("img", 0);

                fsq = Messier.Graphics.Prefabs.FullScreenQuadFactory.Create();
                fsq.SetTexture(0, gbuf.Specular);

                //Scene.SceneShader = prog;
                Scene.SceneShader = gbuf.Shader;
            };

            GraphicsDevice.Update += (e) =>
             {
                GraphicsDevice.Wireframe = !false;
                GraphicsDevice.AlphaEnabled = true;
                GraphicsDevice.DepthTestEnabled = true;
                GraphicsDevice.CullEnabled = false;
                GraphicsDevice.CullMode = CullFaceMode.Back;

                 context.Update(e);
                 //Scene.SceneShader.Set("Fcoef", (float)(2.0f / Math.Log(1000001) / Math.Log(2)));
             };

            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.Clear();
                //gbuf.Bind();
                //GraphicsDevice.Clear();
                //s.Draw(context);

                //GraphicsDevice.SetFramebuffer(Framebuffer.Default);
                //GraphicsDevice.Wireframe = false;
                //GraphicsDevice.AlphaEnabled = !true;
                //GraphicsDevice.DepthTestEnabled = !true;
                //GraphicsDevice.CullEnabled = false;
                //GraphicsDevice.CullMode = CullFaceMode.Back;
                fsq.Bind();
                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.Draw(PrimitiveType.Triangles, 0, fsq.IndexCount);

                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "Scene Test";
            GraphicsDevice.Run(60, 60);
            if (GraphicsDevice.Cleanup != null) GraphicsDevice.Cleanup();
        }
    }
}
