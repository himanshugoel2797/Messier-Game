using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public struct Bone
    {
        public string Name;
        public List<VertexWeight> Weights;
        public Matrix4 Offset;
    }
}
