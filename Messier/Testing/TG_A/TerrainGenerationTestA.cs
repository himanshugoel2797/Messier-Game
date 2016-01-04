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
            ShaderProgram prog = null, terrProg = null;
            Texture tex = null, t2 = null;

            GraphicsContext context = new GraphicsContext()
            {
                Camera = new FirstPersonCamera(new Vector3(4, 3, 3), Vector3.UnitY)
            };

            BitmapTextureSource bmpTex, b2, b3;
            bmpTex = TextDrawer.CreateWriter("Times New Roman", FontStyle.Regular).Write("Hello!", 200, System.Drawing.Color.White);
            b2 = new BitmapTextureSource("test.jpg", 0);
            b3 = new BitmapTextureSource("test.png", 0);


            Texture fbufTex = null, t3 = null;
            FramebufferTextureSource fbufTexSrc = new FramebufferTextureSource(256, 256, 0);
            Framebuffer fbuf = null;

            Matrix4 World = Matrix4.Identity;
            EngineObject eObj = null, fsq = null;
            float timer = 0;

            CubeMapTextureSource tFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.PositiveY, b3),
                bFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.PositiveX, b3),
                lFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.PositiveZ, b3),
                rFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.NegativeX, b3),
                fFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.NegativeY, b3),
                hFace = new CubeMapTextureSource(CubeMapTextureSource.CubeMapFace.NegativeZ, b3);

            GraphicsDevice.Load += () =>
            {
                // setup settings, load textures, sounds
                GraphicsDevice.Wireframe = false;
                GraphicsDevice.AlphaEnabled = true;
                GraphicsDevice.DepthTestEnabled = true;
                GraphicsDevice.CullEnabled = false;
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

                ShaderSource vA = ShaderSource.Load(ShaderType.VertexShader, "Shaders/TerrainGen/vertex.glsl");
                ShaderSource fA = ShaderSource.Load(ShaderType.FragmentShader, "Shaders/TerrainGen/fragment.glsl");
                terrProg = new ShaderProgram(vA, fA);
                terrProg.Set("World", Matrix4.Identity);

                fbuf = new Framebuffer(256, 256);
                fbufTex = new Texture();
                fbufTex.SetData(fbufTexSrc);
                fbuf[FramebufferAttachment.ColorAttachment0] = fbufTex;

                fsq = FullScreenQuadFactory.Create();

                t3 = new Texture();
                t3.SetData(tFace);
                t3.SetData(bFace);
                t3.SetData(lFace);
                t3.SetData(rFace);
                t3.SetData(fFace);
                t3.SetData(hFace);
                t3.SetEnableLinearFilter(true);
                t3.SetAnisotropicFilter(Texture.MaxAnisotropy);

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

                b3.Dispose();
            };
            bool eyePosStill = false;
            GraphicsDevice.Update += (e) =>
            {
                // add game logic, input handling
                if (GraphicsDevice.Keyboard[Key.Escape])
                {
                    GraphicsDevice.Exit();
                }

                if(GraphicsDevice.Keyboard[Key.Z])
                {
                    GraphicsDevice.Wireframe = !GraphicsDevice.Wireframe;
                    //GraphicsDevice.CullEnabled = !GraphicsDevice.Wireframe;
                }

                if(GraphicsDevice.Keyboard[Key.F])
                {
                    eyePosStill = !eyePosStill;
                    Console.WriteLine("EyePosStill = " + eyePosStill);
                }

                context.Update(e);
                //context.Projection = Matrix4.Identity;
                //context.View = Matrix4.Identity;

                prog.Set("View", context.View);
                prog.Set("Proj", context.Projection);
                if(!eyePosStill)prog.Set("eyePos", context.Camera.Position);

                prog.Set("Fcoef", (float)(2.0f / Math.Log(1000001)/Math.Log(2)));

                //timer += 0.001f;
                //World = Matrix4.RotateY(timer);
                World = Matrix4.CreateScale(10);
                prog.Set("World", World);

                prog.Set("timer", timer);
                terrProg.Set("timer", timer);
            };

            bool te1 = false;

            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.SetFramebuffer(fbuf);
                GraphicsDevice.Clear();
                
                fsq.Bind();
                GraphicsDevice.SetShaderProgram(terrProg);
                GraphicsDevice.SetViewport(0, 0, 256, 256);
                GraphicsDevice.Draw(PrimitiveType.Triangles, 0, fsq.IndexCount);

                eObj.Bind();
                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.SetViewport(0, 0, GraphicsDevice.WindowSize.Width, GraphicsDevice.WindowSize.Height);
                GraphicsDevice.PatchCount = 3;
                GraphicsDevice.Draw(PrimitiveType.Patches, 0, eObj.IndexCount);

                GraphicsDevice.SwapBuffers();
                if (!te1)
                {
                    te1 = true;
                    GraphicsDevice.SaveTexture(fbufTex, "test1.png");
                }
            };

            GraphicsDevice.Name = "The Julis Faction";
            // Run the game at 60 updates per second
            GraphicsDevice.Run(60.0, 60.0);
            if(GraphicsDevice.Cleanup != null)GraphicsDevice.Cleanup();
        }
    }
}
