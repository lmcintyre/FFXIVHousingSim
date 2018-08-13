using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using UnityEngine;

namespace FFXIVHSLib
{
    //Utility for serialization shared between WPF/SaintCoinach and Unity.
    //TODO: Extract classes to their own files

    public enum Size
    {
        s, m, l, x = 254
    }

    public enum FixtureType
    {
        rof = 1,
        wal,
        wid,
        dor,
        orf,
        owl,
        osg,
        fnc
    }

    public enum DoorVariants
    {
        ca, cb, ci, co
    }

    public enum WindowVariants
    {
        ci, co
    }

    public enum FenceVariants
    {
        a, b, c, d
    }

    public class Vector3
    {
        public float x, y, z;

        public Vector3()
        {
            x = 0;
            y = 0;
            z = 0;
        }

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static implicit operator UnityEngine.Vector3(Vector3 v)
        {
            return new UnityEngine.Vector3(v.x, v.y, v.z);
        }

        public static Vector3 operator+(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator +(Vector3 a, UnityEngine.Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static Vector3 operator +(UnityEngine.Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public override bool Equals(object obj)
        {
            Vector3 v3 = obj as Vector3;

            if (v3 == null)
                return false;

            return (v3.x == x && v3.y == y && v3.z == z);
        }

        /// <summary>
        /// Returns this Vector3 with its x, y, and z components converted from
        /// degrees to radians using Mathf.Rad2Deg.
        /// </summary>
        /// <returns></returns>
        public Vector3 RadiansToDegreesRotation()
        {
            Vector3 v = new Vector3(x, y, z);
            v.x *= Mathf.Rad2Deg;
            v.y *= Mathf.Rad2Deg;
            v.z *= Mathf.Rad2Deg;
            return v;
        }

        public Quaternion ToQuaternion()
        {
            Matrix m = Matrix.Identity *
                       Matrix.RotationX(x) *
                       Matrix.RotationY(y) *
                       Matrix.RotationZ(z);

            SharpDX.Quaternion dxQuat = SharpDX.Quaternion.RotationMatrix(m);

            return new Quaternion(dxQuat.X, dxQuat.Y, dxQuat.Z, dxQuat.W);
        }
    }

    public class Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion()
        {

        }

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static implicit operator UnityEngine.Quaternion(Quaternion q)
        {
            return new UnityEngine.Quaternion(q.x, q.y, q.z, q.w);
        }

        public static implicit operator SharpDX.Quaternion(Quaternion q)
        {
            return new SharpDX.Quaternion(q.x, q.y, q.z, q.w);
        }

        /// <summary>
        /// Cannot be called from outside of Unity code.
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public Vector3 ToVector3()
        {
            UnityEngine.Vector3 euler = ((UnityEngine.Quaternion) this).eulerAngles;
            Vector3 vector = new Vector3(euler.x, euler.y, euler.z);
            return vector;
        }
    }

    public class Transform
    {
        public Vector3 translation { get; set; }
        public Vector3 rotation { get; set; }
        public Vector3 scale { get; set; }

        public static Transform Empty =
            new Transform(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1));

        public Transform()
        {
            translation = new Vector3();
            rotation = new Vector3();
            scale = new Vector3();
        }

        public Transform(Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            this.translation = translation;
            this.rotation = rotation;
            this.scale = scale;
        }

        public override bool Equals(object obj)
        {
            Transform tr = obj as Transform;

            if (tr == null)
                return false;

            return (tr.translation == translation &&
                    tr.rotation == rotation &&
                    tr.scale == scale);
        }
    }
    
    /// <summary>
    /// Settings relevant to parsing data to serialize into WardInfo.json.
    /// See FFXIVHousingSim.DataWriter for more info.
    /// </summary>
    public class WardSetting
    {
        public Plot.Ward Ward { get; set; }
        public string group { get; set; }
        public string subdivisionSuffix { get; set; }
        public string plotName { get; set; }

        public WardSetting() { }
    }

    /// <summary>
    /// Class representing a serialized bg.lgb.
    /// </summary>
    public class Map
    {
        /// <summary>
        /// Handles the groups of the map and their positions.
        /// </summary>
        public Dictionary<int, MapGroup> groups { get; set; }

        /// <summary>
        /// Maps a first-come, first-serve ID to each unique model.
        /// </summary>
        public Dictionary<int, MapModel> models { get; set; }

        public void AddMapGroup(MapGroup group)
        {
            if (groups == null)
                groups = new Dictionary<int, MapGroup>();
            
            int id = groups.Keys.Count;
            
            group.id = id;
            groups.Add(id, group);
        }

        /// <summary>
        /// Attempts to add a new model to the models dictionary.<br />
        /// Always returns the ID of the model that was attempted to add.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int TryAddUniqueModel(MapModel model)
        {
            if (models == null)
                models = new Dictionary<int, MapModel>();

            //Attempt to get model
            var res = models.Where(_ => _.Value.Equals(model)).Select(_ => _);

            if (res.Count() == 1)
                return res.Single().Key;
            
            int id = models.Count;
            model.id = id;
            models.Add(id, model);
            return id;
        }
    }

    public class MapGroup
    {
        public enum GroupType
        {
            LGB, SGB, TERRAIN
        }

        public int id;
        public GroupType type;
        public string groupName;
        public Transform groupTransform;

        public MapGroup[] groups;
        public MapModelEntry[] entries;

        public MapGroup()
        {

        }

        public MapGroup(GroupType t, string name)
        {
            type = t;
            groupName = name;
        }

    }

    /// <summary>
    /// Class representing a TransformedModel located within a bg.lgb.
    /// </summary>
    public class MapModelEntry
    {
        //Determine if id necessary
        public int id { get; set; }
        public int modelId { get; set; }
        public Transform transform { get; set; }
    }

    /// <summary>
    /// Class representing a Model.
    /// </summary>
    public class MapModel
    {
        public int id { get; set; }
        public string modelPath { get; set; }
        public string modelName { get; set; }
        public int numMeshes { get; set; }

        public override bool Equals(object l)
        {
            if (l is MapModel)
            {
                MapModel m = (MapModel) l;
                return modelPath == m.modelPath &&
                       modelName == m.modelName &&
                       numMeshes == m.numMeshes;
            }
            return false;
        }
    }

    /// <summary>
    /// Plot data relevant to serializing plot data into WardInfo.json.
    /// See FFXIVHousingSim.DataWriter for more info.
    /// </summary>
    public class Plot
    {
        public enum Ward { S1H1, F1H1, W1H1, E1H1 }

        public Ward ward { get; set; }
        public bool subdiv { get; set; }
        public byte index { get; set; }
        public Size size { get; set; }
        public Vector3 position { get; set; }
        public Quaternion rotation { get; set; }

        public Plot() { }

        public Plot(Ward ward, bool sub, byte ind, Size size)
        {
            this.ward = ward;
            this.index = ind;
            this.subdiv = sub;
            this.size = size;
        }

        public static Ward StringToWard(String ward)
        {
            return (Ward) Enum.Parse(typeof(Ward), ward.ToUpperInvariant());
        }
    }

    /// <summary>
    /// Class representing the exterior of a house via IDs. Structured to be compatible with FFXIV WardInfo landSet data.
    /// </summary>
    public class HousingExteriorStructure
    {
        public Size size { get; set; }
        public int[] fixtures;
    }

    public class HousingExteriorBlueprintSet
    {
        public static string[] SgbPaths = {"bg/ffxiv/sea_s1/hou/dyna/house/s1h0_03_s_house.sgb",
                                            "bg/ffxiv/sea_s1/hou/dyna/house/s1h0_01_m_house.sgb",
                                            "bg/ffxiv/sea_s1/hou/dyna/house/s1h0_02_l_house.sgb"};

        public HousingExteriorBlueprint[] set { get; set; }
    }

    /// <summary>
    /// A class representation of the locations in which to place HousingExteriorFixtures.<br />
    /// </summary>
    public class HousingExteriorBlueprint
    {
        public Size size { get; set; }
        public Dictionary<FixtureType, List<Transform>[]> fixtureTransforms;

        /// <summary>
        /// Constructor that populates the Blueprint dictionary.
        /// </summary>
        public HousingExteriorBlueprint()
        {
            fixtureTransforms = new Dictionary<FixtureType, List<Transform>[]>();
            fixtureTransforms.Add(FixtureType.rof, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.wal, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.wid, new List<Transform>[2]);
            fixtureTransforms.Add(FixtureType.dor, new List<Transform>[4]);
            fixtureTransforms.Add(FixtureType.orf, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.owl, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.osg, new List<Transform>[1]);
            fixtureTransforms.Add(FixtureType.fnc, new List<Transform>[4]);
        }
    }

    /// <summary>
    /// A class representation of an exterior housing fixture.<br />
    /// For fixtures that have only one variant, the length of variant must be 1.
    /// </summary>
    public class HousingExteriorFixture
    {
        public int itemId { get; set; }

        public int fixtureId { get; set; }
        public byte fixtureModelKey { get; set; }
        public FixtureType fixtureType { get; set; }
        public int fixtureIntendedUse { get; set; }
        public Size size { get; set; }

        public string name { get; set; }
        public HousingExteriorFixtureVariant[] variants { get; set; }

        /// <summary>
        /// Returns a string array of paths to .sgbs for this Fixture.
        /// TODO: I'm not even done writing this method, but please clean it up at some point.
        /// </summary>
        /// <returns></returns>
        public string[] GetPaths()
        {
            int variants = GetVariants(fixtureType);

            string[] paths = new string[variants];

            /*  Opt paths are constructed differently
                Their intended use is always 20   */
            if (fixtureType == FixtureType.orf ||
                fixtureType == FixtureType.osg ||
                fixtureType == FixtureType.owl)
            {
                string fixtureTypePath = fixtureType.ToString().Substring(1);
                string sgbFormat = "bgcommon/hou/dyna/opt/{0}/{1:D4}/asset/opt_{0}_m{1:D4}.sgb";

                //Only one variant here
                paths[0] = string.Format(sgbFormat, fixtureTypePath, fixtureModelKey);
            }
            /*
             * Doors and windows must take fixtureIntendedUse into account.
             */
            else if (fixtureType == FixtureType.dor)
            {
                string fixtureTypePath = fixtureType.ToString();
                string[] variantKey = {"ca", "cb", "ci", "co"};

                string sgbFormat = "bgcommon/hou/dyna/{0}/{1}/{2:D4}/asset/{0}_{3}_{1}{2:D4}.sgb";
                string placeString = "";

                //Can get info from TerritoryType... but this is fine
                switch (fixtureIntendedUse)
                {
                    case 20:
                        placeString = "com";
                        break;
                    case 22:
                        placeString = "s1h0";
                        break;
                    case 23:
                        placeString = "f1h0";
                        break;
                    case 24:
                        placeString = "w1h0";
                        break;
                    case 2402:
                        placeString = "e1h0";
                        break;
                }

                for (int i = 0; i < variants; i++)
                    paths[i] = string.Format(sgbFormat, placeString, fixtureTypePath, fixtureModelKey,
                        ((DoorVariants) i).ToString());
            }
            else if (fixtureType == FixtureType.wid)
            {
                string fixtureTypePath = fixtureType.ToString();
                string[] variantKey = {"ci", "co"};

                string sgbFormat = "bgcommon/hou/dyna/{0}/{1}/{2:D4}/asset/{0}_{3}_{1}{2:D4}.sgb";
                string placeString = "";

                //Can get info from TerritoryType... but this is fine
                switch (fixtureIntendedUse)
                {
                    case 20:
                        placeString = "com";
                        break;
                    case 22:
                        placeString = "s1h0";
                        break;
                    case 23:
                        placeString = "f1h0";
                        break;
                    case 24:
                        placeString = "w1h0";
                        break;
                    case 2402:
                        placeString = "e1h0";
                        break;
                }

                for (int i = 0; i < variants; i++)
                    paths[i] = string.Format(sgbFormat, placeString, fixtureTypePath, fixtureModelKey,
                        ((WindowVariants) i).ToString());
            }
            else if (fixtureType == FixtureType.fnc)
            {
                string fixtureTypePath = fixtureType.ToString();
                string[] variantKey = {"a", "b", "c", "d"};

                string sgbFormat = "bgcommon/hou/dyna/com/c_{0}/{1:D4}/asset/com_f_{0}{1:D4}{2}.sgb";

                for (int i = 0; i < variants; i++)
                    paths[i] = string.Format(sgbFormat, fixtureTypePath, fixtureModelKey,
                        ((FenceVariants) i).ToString());
            }
            else
            {
                string fixtureSizeStr = size.ToString();
                string fixtureTypePath = fixtureType.ToString();

                string sgbFormat = "bgcommon/hou/dyna/com/{0}_{1}/{2:D4}/asset/com_{0}_{1}{2:D4}.sgb";

                paths[0] = string.Format(sgbFormat, fixtureSizeStr, fixtureTypePath, fixtureModelKey);
            }

            return paths;
        }

        /// <summary>
        /// For use with fences in which the .sgb files are in HousingExterior.
        /// JANK AF
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string[] GetPaths(string s)
        {
            string[] paths = new string[4];

            string cutoff = s.Substring(0, s.Length - 5);

            paths[0] = s;
            paths[1] = cutoff + "b.sgb";
            paths[2] = cutoff + "c.sgb";
            paths[3] = cutoff + "d.sgb";

            return paths;
        }

        /// <summary>
        /// Returns the number of variants available for a given FixtureType.
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public static int GetVariants(FixtureType f)
        {
            switch (f)
            {
                case FixtureType.wid:
                    return 2;
                case FixtureType.dor:
                    return 4;
                case FixtureType.fnc:
                    return 4;
                default:
                    return 1;
            }
        }
    }

    /// <summary>
    /// A class representation of an .sgb file for an exterior housing fixture.
    /// </summary>
    public class HousingExteriorFixtureVariant
    {
        public string sgbPath { get; set; }
        public HousingExteriorFixtureModel[] models { get; set; }
    }

    /// <summary>
    /// A class representation of an .mdl file belonging to a HousingExteriorFixtureVariant.
    /// </summary>
    public class HousingExteriorFixtureModel
    {
        public string modelPath { get; set; }
        public string modelName { get; set; }
        public int numMeshes { get; set; }
        public Transform transform { get; set; }
    }
}
