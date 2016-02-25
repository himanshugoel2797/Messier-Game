using Messier.Graphics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public class Scene
    {
        public EngineObject[] EngineObjects;
        public Light[] Lights;
        public Dictionary<string, Animation> Animations;
        public SceneNode RootNode;

        public MaterialDictionary Materials;

        public static ShaderProgram SceneShader;

        private void Draw(GraphicsContext context, SceneNode n, Matrix4 parentTransform)
        {
            //Matrix4 t = parentTransform;
            Matrix4 t = n.Transform * parentTransform;

            for (int i = 0; i < n.Meshes.Length; i++)
            {
                EngineObject e = EngineObjects[n.Meshes[i]];
                Material m = Materials[e.MaterialIndex.ToString()];

                //Setup the engine object for rendering
                e.Bind();
                GraphicsDevice.SetShaderProgram(SceneShader);
                SceneShader.Set("World", t);
                SceneShader.Set("SpecExp", m.SpecularExponent);
                SceneShader.Set("SpecColor", m.SpecularColor);
                SceneShader.Set("DiffuseColor", m.DiffuseColor);
                SceneShader.Set("AmbientColor", m.AmbientColor);
                SceneShader.Set("DiffuseTex", 1);
                SceneShader.Set("NormalTex", 2);

                GraphicsDevice.SetTexture(0, m.AmbientTexture);
                GraphicsDevice.SetTexture(1, m.DiffuseTexture);
                GraphicsDevice.SetTexture(2, m.NormalMap);
                GraphicsDevice.Draw(OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles, 0, e.IndexCount);

                if(EngineObjects[i].Bones.Any((a)=>a.Name == n.Name))
                {
                    //This node represents a bone for this mesh, retrieve the relevant animation data and use it to adjust the transform
                    t = EngineObjects[i].UpdateVertices(n.Name, t);
                }
            }

            for (int i = 0; i < n.Children.Length; i++)
            {
                Draw(context, n.Children[i], t);
            }
        }

        public void Draw(GraphicsContext context)
        {
            SceneShader.Set("View", context.View);
            SceneShader.Set("Proj", context.Projection);
            Draw(context, RootNode, RootNode.Transform);
        }
    }
}
