using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Voxel
{
    public class VoxelSuperChunk : IDisposable
    {
        Dictionary<Vector3, VoxelChunk> chunks;

        public bool Visible { get; set; } = false;
        public long ID { get; }

        public VoxelSuperChunk(long ID)
        {
            chunks = new Dictionary<Vector3, VoxelChunk>();
        }

        public void SetupData()
        {
            for (int i = 0; i < chunks.Values.Count; i++)
            {
                chunks.Values.ElementAt(i).SetupData();
            }
        }

        public void Load()
        {
            for (int i = 0; i < chunks.Values.Count; i++)
            {
                chunks.Values.ElementAt(i).Load();
            }
        }

        public void Save()
        {
            for(int i = 0; i < chunks.Values.Count; i++)
            {
                chunks.Values.ElementAt(i).Save();
            }
        }

        public void SaveAndFree()
        {
            for (int i = 0; i < chunks.Values.Count; i++)
            {
                chunks.Values.ElementAt(i).SaveAndFree();
            }
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
                    SaveAndFree();
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~VoxelSuperChunk() {
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
