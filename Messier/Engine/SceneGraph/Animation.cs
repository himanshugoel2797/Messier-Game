using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine.SceneGraph
{
    public struct Animation
    {
        //Contain an array of AnimationData structures which contains the matrix to be applied at the frame, the scene renderer can take this information and use it to
        //determine the transform to be applied to a set of bones at the frame, the transform is then passed on to the EngineObject which is responsible for updating its vertices
        //based on their weight to the bone

        private Dictionary<int, AnimationData> srcFrames;
        public Animation(string name, Dictionary<int, AnimationData> data, float framesPerSecond)
        {
            srcFrames = data;
            Name = name;
            FramesPerSecond = framesPerSecond;
        }

        public string Name { get; set; }

        public AnimationData GetFrame(int frame)
        {
            int minKey = 0;
            int maxKey = 0;

            for (int i = 0; i < srcFrames.Keys.Count - 1; i++)
            {
                if (srcFrames.Keys.ElementAt(i) <= frame && srcFrames.Keys.ElementAt(i + 1) > frame)
                {
                    minKey = srcFrames.Keys.ElementAt(i);
                    maxKey = srcFrames.Keys.ElementAt(i + 1);
                }
            }

            //Interpolate between minKey and maxKey based off of the frame number
            var btm = srcFrames[minKey];
            var top = srcFrames[maxKey];

            return AnimationData.Interpolate(btm, top, (float)(frame - minKey) / (float)(maxKey - minKey));
        }

        public float FramesPerSecond { get; set; }
    }
}
