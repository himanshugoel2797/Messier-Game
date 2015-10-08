using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Voxel
{
    public class VoxelChunk : IDisposable
    {
        const int Side = 512;   //Decide on exactly how large this is, what does it represent?

        public int ID { get; set; }

        ushort[,,] voxels;

        public VoxelChunk(int ID)
        {
            this.ID = ID;
            voxels = new ushort[Side, Side, Side];
        }

        public ushort this[int x, int y, int z]
        {
            get
            {
                if (voxels == null) Load();
                return voxels[x, y, z];
            }
            set
            {
                voxels[x, y, z] = value;
            }
        }

        public void Save()
        {
            if (!Directory.Exists("VoxelData")) Directory.CreateDirectory("VoxelData");
            File.WriteAllBytes("VoxelData/" + ID.ToString() + ".vdat", voxels.Cast<byte>().ToArray());
        }

        public void Load()
        {
            byte[] tmp = File.ReadAllBytes("VoxelData/" + ID.ToString() + ".vdat");
            voxels = new ushort[Side, Side, Side];
            Buffer.BlockCopy(tmp, 0, voxels, 0, tmp.Length);
        }

        public void FreeData()
        {
            voxels = null;
        }

        public void SaveAndFree()
        {
            Save();
            FreeData();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    voxels = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~VoxelChunk() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
