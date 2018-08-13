using System.IO;
using FFXIVHSLib;
using SaintCoinach.Graphics;
using SharpDX;
using Quaternion = FFXIVHSLib.Quaternion;
using Vector3 = FFXIVHSLib.Vector3;

namespace FFXIVHSLauncher
{
    static class Extensions
    {
        public static MapModelEntry ToMapModelEntry(this TransformedModel t, int modelId,
                                                    ref Matrix lgbTMatrix,
                                                    ref Matrix rootGimTMatrix,
                                                    ref Matrix thisGimTMatrix,
                                                    ref Matrix modelTMatrix,
                                                    Vector3 parentTranslation = null)
        {
            MapModelEntry m = new MapModelEntry();
            m.modelId = modelId;

            Matrix finalTransform = modelTMatrix * rootGimTMatrix * thisGimTMatrix * lgbTMatrix;

            Transform entryTransform = new Transform();
            entryTransform.translation = new Vector3(t.Translation.X, t.Translation.Y, t.Translation.Z);
//            if (parentTranslation != null)
//                entryTransform.translation += new Vector3(parentTranslation.x, parentTranslation.y, parentTranslation.z);
            entryTransform.scale = new Vector3(t.Scale.X, t.Scale.Y, t.Scale.Z);
            //entryTransform.rotation = finalTransform.ExtractRotationQuaternion();

            Quaternion test1 = ExtractRotationQuaternion(finalTransform);

            Matrix mTest2 = Matrix.Identity *
                            Matrix.RotationX(t.Rotation.X) *
                            Matrix.RotationY(t.Rotation.Y) *
                            Matrix.RotationZ(t.Rotation.Z);
            Quaternion test2 = ExtractRotationQuaternion(mTest2);

            Matrix mTest3 = Matrix.RotationX(t.Rotation.X) *
                            Matrix.RotationY(t.Rotation.Y) *
                            Matrix.RotationZ(t.Rotation.Z);
            Quaternion test3 = ExtractRotationQuaternion(mTest3);

            entryTransform.rotation = test2;

            //Fix for map reflection
//            entryTransform.translation = new Vector3(entryTransform.translation.x * -1,
//                                                        entryTransform.translation.y,
//                                                        entryTransform.translation.z);

            m.transform = entryTransform;
            
            return m;
        }

        public static MapModel ToMapModel(this ModelDefinition m)
        {
            MapModel mModel = new MapModel();

            mModel.modelPath = m.File.Path;
            mModel.modelName = Path.GetFileNameWithoutExtension(mModel.modelPath.Substring(mModel.modelPath.LastIndexOf('/') + 1));
            mModel.numMeshes = m.GetModel(ModelQuality.High).Meshes.Length;

            return mModel;
        }

        public static Quaternion ExtractRotationQuaternion(this Matrix m)
        {
            SharpDX.Quaternion dxRot = SharpDX.Quaternion.RotationMatrix(m);
            return new Quaternion(dxRot.X, dxRot.Y, dxRot.Z, dxRot.W);
        }

        public static Vector3 ToLibVector3(this SaintCoinach.Graphics.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
