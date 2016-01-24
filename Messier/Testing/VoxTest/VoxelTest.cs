using Messier.Engine;
using Messier.Graphics;
using Messier.Graphics.Cameras;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Testing.VoxTest
{
    public class VoxelTest
    {
        public static void Run()
        {
            GraphicsContext context = new GraphicsContext()
            {
                Camera = new FirstPersonCamera(new Vector3(4, 3, 3), Vector3.UnitY)
            };
            Matrix4 World = Matrix4.Identity;


            VoxelTypeMap v = new VoxelTypeMap();
            v[0] = new VoxelTypeMap.VoxelTypeData()
            {
                Color = Vector4.One,
                Visible = false
            };

            v[1] = new VoxelTypeMap.VoxelTypeData()
            {
                Color = Vector4.UnitY * 0.7f + Vector4.UnitW,
                Visible = true
            };

            v[2] = new VoxelTypeMap.VoxelTypeData()
            {
                Color = Vector4.UnitX + Vector4.UnitW,
                Visible = true
            };

            DualVoxelChunk c = new DualVoxelChunk();
            c.Side = 128;
            OpenSimplexNoise snoise = new OpenSimplexNoise(5);
            BitmapTextureSource bTexSrc = new BitmapTextureSource("grassTex.jpg", 0);
            BitmapTextureSource nTexSrc = new BitmapTextureSource("normalmap1.jpg", 0);
            BitmapTextureSource snowNormSrc = new BitmapTextureSource("snowNorm.jpg", 0);

            Func<float, float, float, float> f = (x, y, z) =>
            {
                //if (x == 0 | y == 0 | z == 0 | x == c.Side - 1 | y == c.Side - 1 | z == c.Side - 1) return 0;
                //else return 1;
                float n = 64;


                float no = 0;

                for (int i = 1; i < 4; i++)
                {
                    no += (float)(1.0d - Math.Abs(snoise.Evaluate(x / (n * i), z / (n * i)))) * 2 - 1;
                }
                //no = no / 3 + (float)snoise.Evaluate(x / n + 50, z / n + 50) * 2;
                n = 32;
                //no = no * (float)Math.Round(snoise.Evaluate(x / n + 50, z / n + 50));

                float w = Math.Max(Math.Min(y - 2, 1), 0);
                return (1 - w) * (y - 3f) + (w) * (y - no * 8);

                x -= c.Side / (2);
                y -= c.Side / (2);
                z -= c.Side / (2);

                x /= n;
                y /= n;
                z /= n;

                return (float)(x * x + y * y + z * z - 4.0f);
                //return (float)(Math.Sin(x) + Math.Sin(y) + Math.Sin(z));
            };

            c.InitDataStore(f);

            //Rendering stuff
            ShaderProgram prog = null;
            VertexArray vArray = null;
            GPUBuffer verts = null, indices = null, normsB = null;
            Texture t = null, nt = null, st = null;

            GraphicsDevice.Load += () =>
            {
                //GraphicsDevice.Winding = FaceWinding.Clockwise;
                GraphicsDevice.CullMode = OpenTK.Graphics.OpenGL4.CullFaceMode.Back;
                GraphicsDevice.DepthTestEnabled = true;
                GraphicsDevice.Window.KeyUp += (k, e) =>
                {
                    if (e.Key == OpenTK.Input.Key.Z) GraphicsDevice.Wireframe = !GraphicsDevice.Wireframe;
                    else if (e.Key == OpenTK.Input.Key.C) GraphicsDevice.CullEnabled = !GraphicsDevice.CullEnabled;
                };

                Random rng = new Random(0);
                double n = 64;

                v.UpdateBuffers();
                GraphicsDevice.SetBufferTexture(0, v.ColorData);

                ShaderSource vShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader, "Shaders/TerrainLoD/vertex.glsl");
                ShaderSource tCShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.TessControlShader, "Shaders/TerrainLoD/tesscontrol.glsl");
                ShaderSource tEShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.TessEvaluationShader, "Shaders/TerrainLoD/tessdomain.glsl");
                ShaderSource fShader = ShaderSource.Load(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader, "Shaders/TerrainLoD/fragment.glsl");

                prog = new ShaderProgram(vShader, tCShader, tEShader, fShader);
                prog.Set("materialColors", 0);


                float[] tmpV, norm;
                int[] inds;
                c.GenerateMesh(f, out tmpV, out inds, out norm);

                verts = new GPUBuffer(BufferTarget.ArrayBuffer);
                verts.BufferData(0, tmpV, BufferUsageHint.StaticDraw);

                indices = new GPUBuffer(BufferTarget.ElementArrayBuffer);
                indices.BufferData(0, inds, BufferUsageHint.StaticDraw);

                normsB = new GPUBuffer(BufferTarget.ArrayBuffer);
                normsB.BufferData(0, norm, BufferUsageHint.StaticDraw);

                vArray = new VertexArray();
                vArray.SetBufferObject(0, verts, 4, VertexAttribPointerType.Float);
                vArray.SetBufferObject(1, normsB, 4, VertexAttribPointerType.Float);

                t = new Texture();
                t.SetData(bTexSrc);
                prog.Set("tex", 1);

                nt = new Texture();
                nt.SetData(nTexSrc);
                prog.Set("normMap", 2);

                st = new Texture();
                st.SetData(snowNormSrc);
                prog.Set("snowMap", 3);
            };

            GraphicsDevice.Update += (e) =>
            {
                context.Update(e);

                //World *= Matrix4.CreateRotationY(0.01f);

                prog.Set("World", Matrix4.CreateScale(20));
                prog.Set("View", context.View);
                prog.Set("Proj", context.Projection);
                prog.Set("eyePos", context.Camera.Position);
                prog.Set("Fcoef", (float)(2.0f / Math.Log(10000001) / Math.Log(2)));
                prog.Set("lightDir", new Vector3(5, 10, 5).Normalized());

            };


            GraphicsDevice.Render += (e) =>
            {
                GraphicsDevice.Clear();
                GraphicsDevice.SetTexture(1, t);
                GraphicsDevice.SetTexture(2, nt);
                GraphicsDevice.SetTexture(3, st);
                GraphicsDevice.SetShaderProgram(prog);
                GraphicsDevice.SetVertexArray(vArray);
                GraphicsDevice.SetIndexBuffer(indices);
                GraphicsDevice.PatchCount = 3;
                GraphicsDevice.Draw(PrimitiveType.Patches, 0, indices.dataLen);
                GraphicsDevice.SwapBuffers();
            };

            GraphicsDevice.Name = "Voxel Test";
            GraphicsDevice.Run(60, 0);
            if (GraphicsDevice.Cleanup != null) GraphicsDevice.Cleanup();
        }
    }
}
