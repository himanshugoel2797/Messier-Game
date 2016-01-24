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

namespace Messier.Testing.TG_VectField
{
    public class TerrainGenerationTest
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
            b3 = new BitmapTextureSource("heightmap.png", 0);


            Texture fbufTex = null, t3 = null;

            Matrix4 World = Matrix4.Identity;
            EngineObject eObj = null, fsq = null;
            float timer = 0;

            GraphicsDevice.Load += () =>
            {
                // setup settings, load textures, sounds
                GraphicsDevice.Wireframe = false;
                GraphicsDevice.AlphaEnabled = true;
                GraphicsDevice.DepthTestEnabled = true;
                GraphicsDevice.CullEnabled = false;
                GraphicsDevice.CullMode = CullFaceMode.Back;

                //eObj = new EngineObject();
                

                ShaderSource vert = ShaderSource.Load(ShaderType.VertexShader, "Testing/TG_VectField/vertex.glsl");
                ShaderSource frag = ShaderSource.Load(ShaderType.FragmentShader, "Testing/TG_VectField/fragment.glsl");
                ShaderSource tctrl = ShaderSource.Load(ShaderType.TessControlShader, "Testing/TG_VectField/tesscontrol.glsl");
                ShaderSource teval = ShaderSource.Load(ShaderType.TessEvaluationShader, "Testing/TG_VectField/tessdomain.glsl");

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
                eObj = HighResQuadFactory.CreateHighResQuad(512, 512);
                eObj.SetTexture(0, t2);
                eObj.SetTexture(1, t2);

                b3.Dispose();
            };
            bool eyePosStill = false;
            Vector2 pos = Vector2.Zero;

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
                
                if(GraphicsDevice.Keyboard[Key.W])
                {
                    pos += Vector2.UnitX * 10/1024;
                }

                if (GraphicsDevice.Keyboard[Key.S])
                {
                    pos -= Vector2.UnitX * 10 / 1024;
                }

                if (GraphicsDevice.Keyboard[Key.A])
                {
                    pos -= Vector2.UnitY * 10 / 1024;
                }

                if (GraphicsDevice.Keyboard[Key.D])
                {
                    pos += Vector2.UnitY * 10 / 1024;
                }

                //context.Camera.Position = Vector3.Zero;

                //timer += 0.001f;
                //World = Matrix4.RotateY(timer);
                World = Matrix4.CreateTranslation(-256, -0.5f, 256);
                prog.Set("World", World);
                prog.Set("texScale", 0.02f);
                prog.Set("texOffset", pos);
                prog.Set("timer", timer);
            };

            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.Clear();

                eObj.Bind();

                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.PatchCount = 3;
                GraphicsDevice.Draw(PrimitiveType.Patches, 0, eObj.IndexCount);

                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "The Julis Faction";
            // Run the game at 60 updates per second
            GraphicsDevice.Run(60.0, 60.0);
            if(GraphicsDevice.Cleanup != null)GraphicsDevice.Cleanup();
        }
    }
}
