using Messier.Graphics;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public class VoxelTypeMap
    {
        public struct VoxelTypeData
        {
            public Vector4 Color;
            public bool Visible;
        }

        public List<VoxelTypeData> Voxels;

        GPUBuffer mappingData = null;
        public BufferTexture ColorData
        {
            get; internal set;
        } = null;

        public VoxelTypeMap()
        {
            Voxels = new List<VoxelTypeData>();
        }

        public VoxelTypeData this[int mat]
        {
            get
            {
                return Voxels[mat];
            }
            set
            {
                while (mat >= Voxels.Count) Voxels.Add(new VoxelTypeData());
                Voxels[mat] = value;
            }
        }

        public void UpdateBuffers()
        {
            if (mappingData == null | ColorData == null)
            {
                mappingData = new GPUBuffer(OpenTK.Graphics.OpenGL4.BufferTarget.TextureBuffer);
                ColorData = new BufferTexture();
            }

            Vector4[] cols = new Vector4[Voxels.Count];
            for (int i = 0; i < cols.Length; i++) cols[i] = Voxels[i].Color;

            mappingData.BufferData(0, cols, OpenTK.Graphics.OpenGL4.BufferUsageHint.StaticDraw);
            ColorData.SetStorage(mappingData, OpenTK.Graphics.OpenGL4.SizedInternalFormat.Rgba32f);
        }
    }
}
