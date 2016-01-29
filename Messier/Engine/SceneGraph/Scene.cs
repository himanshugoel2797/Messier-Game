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
        public SceneNode RootNode;

        public MaterialDictionary Materials;

        private void Draw(GraphicsContext context, SceneNode n, Matrix4 parentTransform)
        {
            Matrix4 t = parentTransform * n.Transform;
 
            for (int i = 0; i < n.Meshes.Length; i++)
            {
                EngineObject e = EngineObjects[n.Meshes[i]];
                Material m = Materials[e.MaterialIndex.ToString()];

                //Setup the engine object for rendering
            }

            for(int i = 0; i < n.Children.Length; i++)
            {
                Draw(context, n.Children[i], t);
            }
        }

        public void Draw(GraphicsContext context)
        {
            Draw(context, RootNode, Matrix4.Identity);
        }
    }
}
