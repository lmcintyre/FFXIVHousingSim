using System.IO;
using FFXIVHSLib;
using SaintCoinach.Graphics;
using Vector3 = FFXIVHSLib.Vector3;

namespace FFXIVHSLauncher
{
    static class Extensions
    {
        public static MapModelEntry ToMapModelEntry(this TransformedModel t, int modelId, Transform parent = null)
        {
            MapModelEntry m = new MapModelEntry();

            m.modelId = modelId;

            Transform entryTransform = new Transform();
            entryTransform.translation = new Vector3(t.Translation.X, t.Translation.Y, t.Translation.Z);
            entryTransform.rotation = new Vector3(t.Rotation.X, t.Rotation.Y, t.Rotation.Z);
            entryTransform.scale = new Vector3(t.Scale.X, t.Scale.Y, t.Scale.Z);

            //Models within Sgbs inherit transform from their direct parent Sgb
            if (parent != null)
            {
                entryTransform.translation += new Vector3(parent.translation.x, parent.translation.y, parent.translation.z);
                entryTransform.rotation += new Vector3(parent.rotation.x, parent.rotation.y, parent.rotation.z);
            }

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
    }
}
