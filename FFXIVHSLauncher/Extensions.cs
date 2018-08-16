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
        public static MapModelEntry ToMapModelEntry(this TransformedModel t, int modelId)
        {
            //Id
            MapModelEntry m = new MapModelEntry();
            m.modelId = modelId;
            
            //Translation, rotation, scale
            Transform entryTransform = new Transform();

            entryTransform.translation = new Vector3(t.Translation.X, t.Translation.Y, t.Translation.Z);

            Matrix rotationMatrix = Matrix.Identity *
                                    Matrix.RotationX(t.Rotation.X) *
                                    Matrix.RotationY(t.Rotation.Y) *
                                    Matrix.RotationZ(t.Rotation.Z);
            Quaternion rotationQuaternion = ExtractRotationQuaternion(rotationMatrix);
            entryTransform.rotation = rotationQuaternion;

            entryTransform.scale = new Vector3(t.Scale.X, t.Scale.Y, t.Scale.Z);
            
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

        public static Quaternion ToQuaternion(this Vector3 v)
        {
            Matrix m = Matrix.Identity *
                       Matrix.RotationX(v.x) *
                       Matrix.RotationY(v.y) *
                       Matrix.RotationZ(v.z);

            SharpDX.Quaternion dxQuat = SharpDX.Quaternion.RotationMatrix(m);

            return new Quaternion(dxQuat.X, dxQuat.Y, dxQuat.Z, dxQuat.W);
        }
    }
}
