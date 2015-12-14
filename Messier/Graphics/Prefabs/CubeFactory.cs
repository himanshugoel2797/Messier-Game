using Messier.Engine.SceneGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Graphics.Prefabs
{
    class CubeFactory
    {
        static EngineObject eObj;

        static CubeFactory()
        {
            Init();
        }

        private static void Init()
        {
            eObj = new EngineObject();

            eObj.SetIndices(0, new uint[] {
                0, 1, 2, 3,
                0, 4, 5, 0,
                6, 3, 6, 0,
                0, 2, 4, 5,
                1, 0, 2, 1,
                5, 7, 6, 3,
                6, 7, 5, 7,
                3, 4, 7, 4,
                2, 7, 2, 5 }, false);

            eObj.SetUVs(0, new float[] {
                0,1,
                1,1,
                1,0,
                0,0,
                0,1,
                1,1,
                1,0,
                0,0,
            }, false);

            float width = 0.5f, height = 0.5f, depth = 0.5f;

            eObj.SetVertices(0, new float[]{
                -width, -height, -depth,    //0
                -width, -height, depth,     //1
                -width, height, depth,      //2
                width, height, -depth,      //3
                -width, height, -depth,     //4
                width, -height, depth,      //5
                width, -height, -depth,     //6
                width, height, depth        //7
            }, false);
        }

        public static EngineObject Create()
        {
            return new EngineObject(eObj, true);    //Lock the buffers from changes
        }
    }
}
