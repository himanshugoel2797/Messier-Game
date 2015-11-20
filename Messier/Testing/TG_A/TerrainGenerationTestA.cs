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

            BitmapTextureSource bmpTex, b2;
            bmpTex = TextDrawer.CreateWriter("Times New Roman", FontStyle.Regular).Write("Hello ABCDEFGHI!", 200, System.Drawing.Color.White);
            b2 = new BitmapTextureSource("test.jpg", 0);

            Matrix4 World = Matrix4.Identity;
            EngineObject eObj = null;
            float timer = 0;


            GraphicsDevice.Load += () =>
            {
                // setup settings, load textures, sounds
                // GraphicsDevice.Wireframe = true;
                GraphicsDevice.AlphaEnabled = true;

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
                ShaderSource frag = ShaderSource.Load(ShaderType.FragmentShader, "Shaders/fragment.glsl");
                ShaderSource tctrl = ShaderSource.Load(ShaderType.TessControlShader, "Testing/TG_A/tesscontrol.glsl");
                ShaderSource teval = ShaderSource.Load(ShaderType.TessEvaluationShader, "Testing/TG_A/tessdomain.glsl");

                tex = new Texture();
                tex.SetData(bmpTex);
                tex.SetAnisotropicFilter(Texture.MaxAnisotropy);

                t2 = new Texture();
                t2.SetData(b2);
                t2.SetAnisotropicFilter(Texture.MaxAnisotropy);

                prog = new ShaderProgram(vert, tctrl, teval, frag);
                prog.Set("img", 0);
                prog.Set("World", World);

                vert.Dispose();
                frag.Dispose();
                tctrl.Dispose();
                teval.Dispose();

                //eObj.SetVertices(0, data, false);
                //eObj.SetIndices(0, indexData, false);
                eObj = FullScreenQuadFactory.Create();
                eObj.SetTexture(0, tex);
            };

            GraphicsDevice.Update += (e) =>
            {
                // add game logic, input handling
                if (GraphicsDevice.Keyboard[Key.Escape])
                {
                    GraphicsDevice.Exit();
                }

                context.Update(e);
                context.Projection = Matrix4.Identity;
                context.View = Matrix4.Identity;

                prog.Set("View", context.View);
                prog.Set("Proj", context.Projection);
                prog.Set("eyePos", context.Camera.Position);

                timer += 0.01f;
                prog.Set("timer", timer);
            };

            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.Clear();

                eObj.Bind();
                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.PatchCount = 3;
                GraphicsDevice.Draw(PrimitiveType.Patches, 0, 6);

                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "The Julis Faction";
            // Run the game at 60 updates per second
            GraphicsDevice.Run(60.0, 60.0);
            if(GraphicsDevice.Cleanup != null)GraphicsDevice.Cleanup();
        }
    }
}
