using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public enum LightType
    {
        Undefined = 0,
        Directional = 1,
        Point = 2,
        Spot = 3
    }

    public class Light
    {
        public string Name;
        public Vector3 Position;
        public LightType Type;
        public Vector3 Direction;

        public Vector3 DiffuseColor;
        public Vector3 AmbientColor;
        public Vector3 SpecularColor;

        public float QuadraticAttenuation;
        public float ConstantAttenuation;
        public float LinearAttenuation;
    }
}
