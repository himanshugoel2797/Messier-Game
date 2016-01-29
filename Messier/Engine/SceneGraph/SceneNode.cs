using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public class SceneNode
    {
        public SceneNode[] Children;
        public int[] Meshes;

        public Matrix4 Transform;
    }
}
