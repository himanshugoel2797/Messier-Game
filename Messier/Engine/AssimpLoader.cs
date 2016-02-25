using Assimp;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messier.Engine
{
    public class AssimpLoader
    {
        private static Matrix4 ToCustom(Matrix4x4 r)
        {
            Matrix4 n0 = new Matrix4();

            n0.M11 = r.A1;
            n0.M12 = r.B1;
            n0.M13 = r.C1;
            n0.M14 = r.D1;

            n0.M21 = r.A2;
            n0.M22 = r.B2;
            n0.M23 = r.C2;
            n0.M24 = r.D2;

            n0.M31 = r.A3;
            n0.M32 = r.B3;
            n0.M33 = r.C3;
            n0.M34 = r.D3;

            n0.M41 = r.A4;
            n0.M42 = r.B4;
            n0.M43 = r.C4;
            n0.M44 = r.D4;

            return n0;
        }

        public static SceneGraph.Scene LoadFile(string file)
        {
            SceneGraph.Scene s0 = new SceneGraph.Scene();
            SceneGraph.MaterialDictionary m0 = new SceneGraph.MaterialDictionary();

            AssimpContext c = new AssimpContext();
            Scene s = c.ImportFile(file);

            List<List<string>> boneNames = new List<List<string>>();

            List<SceneGraph.Light> lights = new List<SceneGraph.Light>();
            List<SceneGraph.EngineObject> eObjs = new List<SceneGraph.EngineObject>();
            Dictionary<string, Animation> anim = new Dictionary<string, Animation>();

            if (s.HasLights)
            {
                for (int i = 0; i < s.LightCount; i++)
                {
                    SceneGraph.Light tmp = new SceneGraph.Light();
                    tmp.Position = new Vector3(s.Lights[i].Position.X, s.Lights[i].Position.Y, s.Lights[i].Position.Z);
                    tmp.ConstantAttenuation = s.Lights[i].AttenuationConstant;
                    tmp.LinearAttenuation = s.Lights[i].AttenuationLinear;
                    tmp.QuadraticAttenuation = s.Lights[i].AttenuationQuadratic;
                    tmp.Name = s.Lights[i].Name;
                    tmp.Direction = new Vector3(s.Lights[i].Direction.X, s.Lights[i].Direction.Y, s.Lights[i].Direction.Z);
                    tmp.Type = (SceneGraph.LightType)s.Lights[i].LightType;
                    tmp.AmbientColor = new Vector3(s.Lights[i].ColorAmbient.R, s.Lights[i].ColorAmbient.G, s.Lights[i].ColorAmbient.B);
                    tmp.DiffuseColor = new Vector3(s.Lights[i].ColorDiffuse.R, s.Lights[i].ColorDiffuse.G, s.Lights[i].ColorDiffuse.B);
                    tmp.SpecularColor = new Vector3(s.Lights[i].ColorSpecular.R, s.Lights[i].ColorSpecular.G, s.Lights[i].ColorSpecular.B);

                    lights.Add(tmp);
                }
            }

            if (s.HasMeshes)
            {
                for (int i = 0; i < s.MeshCount; i++)
                {
                    List<float> v = new List<float>();

                    SceneGraph.EngineObject eObj = new SceneGraph.EngineObject();

                    List<uint> indices = new List<uint>();
                    for (int k = 0; k < s.Meshes[i].FaceCount; k++)
                    {
                        if (s.Meshes[i].Faces[k].IndexCount == 4)
                        {
                            indices.Add((uint)s.Meshes[i].Faces[k].Indices[0]);
                            indices.Add((uint)s.Meshes[i].Faces[k].Indices[1]);
                            indices.Add((uint)s.Meshes[i].Faces[k].Indices[2]);
                            indices.Add((uint)s.Meshes[i].Faces[k].Indices[2]);
                            indices.Add((uint)s.Meshes[i].Faces[k].Indices[3]);
                            indices.Add((uint)s.Meshes[i].Faces[k].Indices[1]);
                        }
                        else if (s.Meshes[i].Faces[k].IndexCount == 3)
                        {
                            indices.Add((uint)s.Meshes[i].Faces[k].Indices[0]);
                            indices.Add((uint)s.Meshes[i].Faces[k].Indices[1]);
                            indices.Add((uint)s.Meshes[i].Faces[k].Indices[2]);
                        }
                    }

                    eObj.SetIndices(0, indices.ToArray(), false);

                    eObj.MaterialIndex = s.Meshes[i].MaterialIndex;

                    boneNames.Add(new List<string>());
                    eObj.Bones = new SceneGraph.Bone[s.Meshes[i].BoneCount];
                    for (int k = 0; k < s.Meshes[i].BoneCount; k++)
                    {
                        eObj.Bones[k] = new SceneGraph.Bone()
                        {
                            Weights = new List<SceneGraph.VertexWeight>(),
                            Name = s.Meshes[i].Bones[k].Name
                        };
                        boneNames[i].Add(eObj.Bones[k].Name);

                        for (int l = 0; l < s.Meshes[i].Bones[k].VertexWeightCount; l++)
                        {
                            eObj.Bones[k].Weights.Add(new SceneGraph.VertexWeight()
                            {
                                Index = s.Meshes[i].Bones[k].VertexWeights[l].VertexID,
                                Weight = s.Meshes[i].Bones[k].VertexWeights[l].Weight,
                            });
                        }

                        eObj.Bones[k].Offset = ToCustom(s.Meshes[i].Bones[k].OffsetMatrix);
                    }


                    if (s.Meshes[i].HasNormals)
                    {
                        v.Clear();
                        for (int j = 0; j < s.Meshes[i].Normals.Count; j++)
                        {
                            v.Add(s.Meshes[i].Normals[j].X);
                            v.Add(s.Meshes[i].Normals[j].Y);
                            v.Add(s.Meshes[i].Normals[j].Z);
                        }
                        eObj.SetNormals(0, v.ToArray(), false, 3);
                    }

                    if (s.Meshes[i].HasVertices)
                    {
                        v.Clear();
                        for (int j = 0; j < s.Meshes[i].Vertices.Count; j++)
                        {
                            v.Add(s.Meshes[i].Vertices[j].X);
                            v.Add(s.Meshes[i].Vertices[j].Y);
                            v.Add(s.Meshes[i].Vertices[j].Z);
                        }
                        eObj.SetVertices(0, v.ToArray(), false, 3);
                    }

                    if (s.Meshes[i].TextureCoordinateChannelCount > 0)
                    {
                        v.Clear();
                        for (int j = 0; j < s.Meshes[i].TextureCoordinateChannels[0].Count; j++)
                        {
                            v.Add(s.Meshes[i].TextureCoordinateChannels[0][j].X);
                            v.Add(s.Meshes[i].TextureCoordinateChannels[0][j].Y);
                        }
                        eObj.SetUVs(0, v.ToArray(), false, 2);
                    }
                    eObjs.Add(eObj);

                }
            }

            if (s.HasMaterials)
            {
                for (int i = 0; i < s.MaterialCount; i++)
                {
                    SceneGraph.Material m = new SceneGraph.Material();
                    Graphics.BitmapTextureSource bmpA = null, bmpD = null, bmpS = null, bmpN = null, bmpDD = null;

                    m.AmbientColor = new Vector3(s.Materials[i].ColorAmbient.R, s.Materials[i].ColorAmbient.G, s.Materials[i].ColorAmbient.B);
                    m.DiffuseColor = new Vector3(s.Materials[i].ColorDiffuse.R, s.Materials[i].ColorDiffuse.G, s.Materials[i].ColorDiffuse.B);
                    m.SpecularColor = new Vector3(s.Materials[i].ColorSpecular.R, s.Materials[i].ColorSpecular.G, s.Materials[i].ColorSpecular.B);
                    if (s.Materials[i].HasTextureAmbient) bmpA = new Graphics.BitmapTextureSource(s.Materials[i].TextureAmbient.FilePath, 0);
                    if (s.Materials[i].HasTextureDiffuse) bmpD = new Graphics.BitmapTextureSource(s.Materials[i].TextureDiffuse.FilePath, 0);
                    if (s.Materials[i].HasTextureSpecular) bmpS = new Graphics.BitmapTextureSource(s.Materials[i].TextureSpecular.FilePath, 0);
                    if (s.Materials[i].HasTextureNormal) bmpN = new Graphics.BitmapTextureSource(s.Materials[i].TextureNormal.FilePath, 0);
                    if (s.Materials[i].HasTextureDisplacement) bmpDD = new Graphics.BitmapTextureSource(s.Materials[i].TextureDisplacement.FilePath, 0);

                    if (bmpA != null) m.AmbientTexture = new Graphics.Texture();
                    if (bmpD != null) m.DiffuseTexture = new Graphics.Texture();
                    if (bmpS != null) m.PBRTexture = new Graphics.Texture();
                    if (bmpN != null) m.NormalMap = new Graphics.Texture();
                    if (bmpDD != null) m.DisplacementMap = new Graphics.Texture();

                    if (m.AmbientTexture != null) m.AmbientTexture.SetData(bmpA);
                    if (m.DiffuseTexture != null) m.DiffuseTexture.SetData(bmpD);
                    if (m.DisplacementMap != null) m.DisplacementMap.SetData(bmpDD);
                    if (m.NormalMap != null) m.NormalMap.SetData(bmpN);
                    if (m.PBRTexture != null) m.PBRTexture.SetData(bmpS);

                    m0[i.ToString()] = m;
                }
            }

            Func<Node, SceneGraph.SceneNode> rFunc = null;
            rFunc = (r) =>
            {
                SceneGraph.SceneNode n0 = new SceneGraph.SceneNode();
                n0.Meshes = r.MeshIndices.ToArray();

                n0.Transform = ToCustom(r.Transform);

                s0.Lights = lights.ToArray();
                s0.EngineObjects = eObjs.ToArray();
                s0.Materials = m0;


                n0.Name = r.Name;
                n0.Children = new SceneGraph.SceneNode[r.ChildCount];

                for (int i = 0; i < r.ChildCount; i++)
                {
                    n0.Children[i] = rFunc(r.Children[i]);
                }

                return n0;
            };

            s0.RootNode = rFunc(s.RootNode);

            return s0;
        }
    }
}
