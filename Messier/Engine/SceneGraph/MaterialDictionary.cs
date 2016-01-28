using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public class MaterialDictionary
    {
        Dictionary<string, Material> Materials;
        
        public MaterialDictionary()
        {
            Materials = new Dictionary<string, Material>();
        }
        
        public Material this[string name]
        {
            get
            {
                return Materials[name];
            }
            set
            {
                Materials[name] = value;
            }
        }

    }
}


