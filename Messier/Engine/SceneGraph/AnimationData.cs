using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public struct AnimationData
    {
        public Quaternion Rotation;
        public Vector3 Translation;
        public Vector3 Scale;

        public static AnimationData Interpolate(AnimationData a, AnimationData b, float c)
        {
            AnimationData n = new AnimationData();

            n.Rotation = a.Rotation * (1 - c) + b.Rotation * c;
            n.Scale = a.Scale * (1 - c) + b.Scale * c;
            n.Translation = a.Translation * (1 - c) + b.Translation * c;

            return n;
        }
    }
}
