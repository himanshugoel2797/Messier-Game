using Messier.Graphics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public struct Material
    {
        public Vector3 AmbientColor;
        public Vector3 DiffuseColor;
        public Vector3 SpecularColor;
        public float SpecularExponent;
        public float Transparency;
        public Texture AmbientTexture;
        public Texture DiffuseTexture;
        public Texture PBRTexture;  //R = Metalness, G = Roughness, B = AO, A = Alpha
        public Texture NormalMap;
        public Texture DisplacementMap;
    }
}
