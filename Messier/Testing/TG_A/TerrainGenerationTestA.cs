using Messier.Engine;
using Messier.Engine.SceneGraph;
using Messier.Graphics;
using Messier.Graphics.Cameras;
using Messier.Graphics.Prefabs;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Testing.TG_A
{
    public class TerrainGenerationTestA
    {
        public static void Run()
        {
            ShaderProgram prog = null;
            Texture tex = null, t2 = null;

            GraphicsContext context = new GraphicsContext()
            {
                Camera = new FirstPersonCamera(new Vector3(4, 3, 3), Vector3.UnitY)
            };

            BitmapTextureSource bmpTex, b2, b3;
            bmpTex = TextDrawer.CreateWriter("Times New Roman", FontStyle.Regular).Write("Hello!", 200, System.Drawing.Color.White);
            b2 = new BitmapTextureSource("test.jpg", 0);
            b3 = new BitmapTextureSource("heightmap.png", 0);

            CubeMapTextureSource tFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.PositiveY, b3),
                bFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.PositiveX, b3),
                lFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.PositiveZ, b3),
                rFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.NegativeX, b3),
                fFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.NegativeY, b3),
                hFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.NegativeZ, b3);

            Texture fbufTex = null, t3 = null;
            FramebufferTextureSource fbufTexSrc = new FramebufferTextureSource(1920, 1080, 0);
            Framebuffer fbuf = null;

            Matrix4 World = Matrix4.Identity;
            EngineObject eObj = null;
            float timer = 0;


            GraphicsDevice.Load += () =>
            {
                // setup settings, load textures, sounds
                //GraphicsDevice.Wireframe = true;
                GraphicsDevice.AlphaEnabled = true;
                GraphicsDevice.DepthTestEnabled = true;
                GraphicsDevice.CullEnabled = true;
                GraphicsDevice.CullMode = CullFaceMode.Back;

                eObj = new EngineObject();
                float[] data =
                {     -1, -1, 1,
                       1, -1, 1,
                       0, 1, 1
                };

                uint[] indexData = new uint[]
                {
                    0, 1, 2
                };


                ShaderSource vert = ShaderSource.Load(ShaderType.VertexShader, "Testing/TG_A/vertex.glsl");
                ShaderSource frag = ShaderSource.Load(ShaderType.FragmentShader, "Testing/TG_A/fragment.glsl");
                ShaderSource tctrl = ShaderSource.Load(ShaderType.TessControlShader, "Testing/TG_A/tesscontrol.glsl");
                ShaderSource teval = ShaderSource.Load(ShaderType.TessEvaluationShader, "Testing/TG_A/tessdomain.glsl");

                fbuf = new Framebuffer(1920, 1080);
                fbufTex = new Texture();
                fbufTex.SetData(fbufTexSrc);
                fbuf[FramebufferAttachment.ColorAttachment0] = fbufTex;

                t3 = new Texture();
                t3.SetData(tFace);
                t3.SetData(bFace);
                t3.SetData(lFace);
                t3.SetData(rFace);
                t3.SetData(fFace);
                t3.SetData(hFace);

                tex = new Texture();
                tex.SetData(bmpTex);
                tex.SetAnisotropicFilter(Texture.MaxAnisotropy);

                t2 = new Texture();
                t2.SetData(b3);
                t2.SetAnisotropicFilter(Texture.MaxAnisotropy);


                prog = new ShaderProgram(vert, tctrl, teval, frag);
                prog.Set("img", 0);
                prog.Set("heightmap", 1);
                prog.Set("World", World);

                vert.Dispose();
                frag.Dispose();
                tctrl.Dispose();
                teval.Dispose();

                //eObj = FullScreenQuadFactory.Create();
                eObj = CubeFactory.Create();
                eObj.SetTexture(0, t2);
                eObj.SetTexture(1, t3);
            };

            GraphicsDevice.Update += (e) =>
            {
                // add game logic, input handling
                if (GraphicsDevice.Keyboard[Key.Escape])
                {
                    GraphicsDevice.Exit();
                }

                context.Update(e);
                //context.Projection = Matrix4.Identity;
                //context.View = Matrix4.Identity;

                prog.Set("View", context.View);
                prog.Set("Proj", context.Projection);
                prog.Set("eyePos", context.Camera.Position);

                prog.Set("Fcoef", (float)(2.0f / Math.Log(1000001)/Math.Log(2)));

                timer += 0.01f;
                //World = Matrix4.RotateY(timer);
                World = Matrix4.CreateScale(100);
                prog.Set("World", World);

                prog.Set("timer", timer);
            };

            GraphicsDevice.Render += (e) =>
            {
                //GraphicsDevice.SetFramebuffer(fbuf);
                GraphicsDevice.Clear();

                eObj.Bind();
                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.PatchCount = 3;
                GraphicsDevice.Draw(PrimitiveType.Patches, 0, eObj.IndexCount);

                GraphicsDevice.SwapBuffers();
                //GraphicsDevice.SaveTexture(fbufTex, "test.png");
            };

            GraphicsDevice.Name = "The Julis Faction";
            // Run the game at 60 updates per second
            GraphicsDevice.Run(60.0, 60.0);
            if(GraphicsDevice.Cleanup != null)GraphicsDevice.Cleanup();
        }
    }
}
