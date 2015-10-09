using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Voxel
{
    public interface ICamera
    {
        void Update();
        Matrix4 View { get; set; }
        Vector3 Position { get; set; }
        Vector3 Direction { get; set; }
        
    }
}
