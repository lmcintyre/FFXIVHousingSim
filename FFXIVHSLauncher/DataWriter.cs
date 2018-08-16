using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using SaintCoinach;
using SaintCoinach.Graphics;
using SaintCoinach.Graphics.Lgb;
using SaintCoinach.Graphics.Sgb;
using SaintCoinach.Xiv;
using FFXIVHSLib;
using SharpDX;
using File = System.IO.File;
using Directory = System.IO.Directory;
using Map = FFXIVHSLib.Map;
using Quaternion = FFXIVHSLib.Quaternion;
using Vector3 = FFXIVHSLib.Vector3;


namespace FFXIVHSLauncher
{
    /// <summary>
    /// Class that performs all data and serialization functions on the WPF/SaintCoinach side of things.
    /// </summary>
    static class DataWriter
    {
        /// <summary>
        /// Returns a FFXIVHSLib.Transform object from 3 SaintCoinach.Graphics.Vector3 vectors.
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Transform TransformFromVectors(SaintCoinach.Graphics.Vector3 translation,
                                                        SaintCoinach.Graphics.Vector3 rotation,
                                                        SaintCoinach.Graphics.Vector3 scale)
        {
            Transform t = new Transform();
            t.translation = new Vector3(translation.X, translation.Y, translation.Z);
            
            t.rotation = new Vector3(rotation.X, rotation.Y, rotation.Z).ToQuaternion();
            t.scale = new Vector3(scale.X, scale.Y, scale.Z);
            return t;
        }

        /// <summary>
        /// Returns a compatible Transform from header data from LGBs.
        /// </summary>
        /// <param name="hdr"></param>
        /// <returns></returns>
        public static Transform TransformFromGimmickHeader(SgbGimmickEntry.HeaderData hdr)
        {
            return new Transform(hdr.Translation.ToLibVector3(),
                                 hdr.Rotation.ToLibVector3().ToQuaternion(),
                                 hdr.Scale.ToLibVector3());
        }

        /// <summary>
        /// Returns a compatible Transform from header data from SGBs.
        /// </summary>
        /// <param name="hdr"></param>
        /// <returns></returns>
        public static Transform TransformFromGimmickHeader(LgbGimmickEntry.HeaderData hdr)
        {
            return new Transform(hdr.Translation.ToLibVector3(),
                hdr.Rotation.ToLibVector3().ToQuaternion(),
                hdr.Scale.ToLibVector3());
        }

        /// <summary>
        /// Occurs first in the ward data output flow and populates plots with the sizes of
        /// the appropriate plots from the sheet HousingLandSet.
        /// </summary>
        private static void ReadLandSetSheet(ARealmReversed realm, ref List<Plot> plots)
        {
            IXivSheet<XivRow> landSet  = realm.GameData.GetSheet("HousingLandSet");

            foreach (XivRow row in landSet)
            {
                Ward thisWard = (Ward) row.Key;

                for (int i = 0; i < 60; i++)
                {
                    //Get this plot's size
                    Size size = (Size) (byte) row[i];
                    Plot p = new Plot(thisWard, i > 29, (byte) (i % 30 + 1), size);
                    plots.Add(p);
                }
            }
        }

        /// <summary>
        /// Occurs second in the ward data output flow and reads all housing territories for their appropriate
        /// plot placeholders and calculates plot locations from them.<br />
        ///
        /// This <i>would</i> be read from HousingMapMarkerInfo but the height values are incorrect there,
        /// and would mess up camera movement. X coordinates are negated due to the map reflection in Unity.<br />
        ///
        /// This method takes a very long time due to Territory instantiation.
        /// </summary>
        private static void ReadTerritoryPlots(ARealmReversed realm, ref List<Plot> plots)
        {
            //Obtain all housing TerritoryTypes
            IXivSheet<TerritoryType> allTerr = realm.GameData.GetSheet<TerritoryType>();
            TerritoryType[] tTypes = allTerr.ToArray();
            List<TerritoryType> housingTeriTypes = new List<TerritoryType>();

            foreach (TerritoryType t in tTypes)
            {
                if (!String.IsNullOrEmpty(t.PlaceName.ToString()))
                {
                    byte intendedUse = (byte)t.GetRaw("TerritoryIntendedUse");
                    
                    //Housing territory intended use is 13
                    if (intendedUse == 13)
                    {
                        housingTeriTypes.Add(t);
                    }
                }
            }

            WardSetting[] settings = JsonConvert.DeserializeObject<WardSetting[]>(File.ReadAllText(FFXIVHSPaths.GetWardSettingsJson()));
            
            foreach (TerritoryType tType in housingTeriTypes)
            {
                Territory t = new Territory(tType);
                LgbFile bg = null;

                //Get the ward's information from the wardsettings.json
                string groupName = "", plotName = "", subdivisionName = "";

                foreach (WardSetting ws in settings)
                {
                    if (ws.Ward.ToString() == t.Name.ToUpper())
                    {
                        plotName = ws.plotName;
                        groupName = ws.group;
                        subdivisionName = ws.subdivisionSuffix + groupName;
                    }
                }
               
                //We only care about bg.lgb for this territory
                foreach (LgbFile lgbFile in t.LgbFiles)
                    if (lgbFile.File.Path.EndsWith("bg.lgb"))
                        bg = lgbFile;

                if (bg != null)
                {
                    //Define main and subdiv groups
                    var mainGroup = bg.Groups
                                    .Where(_ => _.Name == groupName)
                                    .Select(_ => _);

                    var subDivGroup = bg.Groups
                                    .Where(_ => _.Name == subdivisionName)
                                    .Select(_ => _);

                    //Get main and subdiv plot lists and sort by index
                    var mainPlotList = plots.Where(_ => _.ward.ToString() == t.Name.ToUpper())
                                        .Where(_ => _.subdiv == false)         
                                        .Select(_ => _).ToList();
                    
                    var subdivPlotList = plots.Where(_ => _.ward.ToString() == t.Name.ToUpper())
                                        .Where(_ => _.subdiv == true)
                                        .Select(_ => _).ToList();

                    mainPlotList.Sort((p1, p2) => p1.index.CompareTo(p2.index));
                    subdivPlotList.Sort((p1, p2) => p1.index.CompareTo(p2.index));

                    int plotIndex = 0;
                    foreach (var lgbGroup in mainGroup.ToArray())
                    {
                        foreach (var lgbGimmickEntry in lgbGroup.Entries.OfType<LgbGimmickEntry>())
                        {
                            foreach (var sgbGroup in lgbGimmickEntry.Gimmick.Data.OfType<SgbGroup>())
                            {
                                foreach (var modelEntry in sgbGroup.Entries.OfType<SgbModelEntry>())
                                {
                                    if (modelEntry.Model.Model.File.Path.Contains(plotName))
                                    {
                                        //Position is in gimmick header, not transformedmodel
                                        SaintCoinach.Graphics.Vector3 position = lgbGimmickEntry.Header.Translation;
                                        SaintCoinach.Graphics.Vector3 rotation = lgbGimmickEntry.Header.Rotation;
                                        Vector3 pos = new Vector3(position.X * -1, position.Y, position.Z);
                                        Quaternion rot = new Vector3(rotation.X, rotation.Y, rotation.Z).ToQuaternion();

                                        mainPlotList[plotIndex].position = pos;
                                        mainPlotList[plotIndex].rotation = rot;
                                        plotIndex++;
                                    }
                                }
                            }
                        }
                    }

                    plotIndex = 0;
                    foreach (var lgbGroup in subDivGroup.ToArray())
                    {
                        foreach (var lgbGimmickEntry in lgbGroup.Entries.OfType<LgbGimmickEntry>())
                        {
                            foreach (var sgbGroup in lgbGimmickEntry.Gimmick.Data.OfType<SgbGroup>())
                            {
                                foreach (var modelEntry in sgbGroup.Entries.OfType<SgbModelEntry>())
                                {
                                    if (modelEntry.Model.Model.File.Path.Contains(plotName))
                                    {
                                        //Position is in gimmick header, not transformedmodel
                                        SaintCoinach.Graphics.Vector3 position = lgbGimmickEntry.Header.Translation;
                                        SaintCoinach.Graphics.Vector3 rotation = lgbGimmickEntry.Header.Rotation;
                                        Vector3 pos = new Vector3(position.X * -1, position.Y, position.Z);
                                        Quaternion rot = new Vector3(rotation.X, rotation.Y, rotation.Z).ToQuaternion();

                                        subdivPlotList[plotIndex].position = pos;
                                        subdivPlotList[plotIndex].rotation = rot;
                                        plotIndex++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Reads from game data and serializes a dictionary containing exterior housing fixture information.<br />
        ///
        /// Reads from sheets HousingExterior and Item.
        /// </summary>
        private static Dictionary<int, HousingExteriorFixture> ReadHousingExteriorSheet(ARealmReversed realm)
        {
            Dictionary<int, HousingExteriorFixture> fixtures = new Dictionary<int, HousingExteriorFixture>();

            IXivSheet<XivRow> housingExteriorSheet = realm.GameData.GetSheet("HousingExterior");
            IXivSheet<XivRow> itemSheet = realm.GameData.GetSheet("Item");

            foreach (XivRow row in housingExteriorSheet)
            {
                if (row.Key == 0)
                    continue;

                HousingExteriorFixture rowFixture = new HousingExteriorFixture();
                rowFixture.size = (Size) (byte) row[3];
                
                rowFixture.fixtureId = row.Key;
                rowFixture.fixtureModelKey = (byte) row[0];
                rowFixture.fixtureIntendedUse = (ushort) row[2];
                rowFixture.fixtureType = (FixtureType) (byte) row[1];

                //Data from item sheet
                int[] stainResults = (from x in itemSheet
                    where (uint)x.GetRaw("Stain") == row.Key
                    select x.Key).ToArray();
                
                if (stainResults.Length == 1)
                {
                    rowFixture.itemId = stainResults[0];
                    rowFixture.name = (from x in itemSheet
                        where x.Key == rowFixture.itemId
                        select x["Name"]).Single().ToString();
                }
                else
                {
                    rowFixture.itemId = -1;
                    rowFixture.name = "";
                }
                
                //Get data for variants and modelEntries
                string p = row[4].ToString();
                string[] sgbPaths = {String.IsNullOrEmpty(p) ? null : p};

                //Fences too because row only has one .sgb for them - there are 4
                if (sgbPaths[0] == null)
                    sgbPaths = rowFixture.GetPaths();
                else if (rowFixture.fixtureType == FixtureType.fnc)
                    sgbPaths = rowFixture.GetPaths(sgbPaths[0]);

                /*
                 * For testing
                 
                foreach (string path in sgbPaths)
                {
                    string name = rowFixture.name;
                    if (String.IsNullOrEmpty(name))
                        name = rowFixture.fixtureIntendedUse.ToString();
                    if (!realm.Packs.FileExists(path))
                        Console.WriteLine(name + "|" + path);
                }*/

                //Load model data and things
                rowFixture.variants = ReadSgbForVariantInfo(realm, sgbPaths);

                fixtures.Add(rowFixture.fixtureId, rowFixture);
            }
            return fixtures;
        }

        /// <summary>
        /// Attempts to read all entries of type SgbModelEntry from the paths contained in sgbPaths,
        /// constructs a HousingExteriorFixtureModel for each entry and returns all variants.
        /// </summary>
        /// <param name="realm"></param>
        /// <param name="sgbPaths"></param>
        /// <returns></returns>
        private static HousingExteriorFixtureVariant[] ReadSgbForVariantInfo(ARealmReversed realm, string[] sgbPaths)
        {
            HousingExteriorFixtureVariant[] variants = new HousingExteriorFixtureVariant[sgbPaths.Length];
            for (int i = 0; i < sgbPaths.Length; i++)
            {
                //Check if sgb valid then get it
                SaintCoinach.IO.File baseFile;
                if (!realm.Packs.TryGetFile(sgbPaths[i], out baseFile))
                    throw new FileNotFoundException();
                SgbFile sgb = new SgbFile(baseFile);

                //Get modelEntries
                HousingExteriorFixtureVariant thisVariant = new HousingExteriorFixtureVariant();
                thisVariant.sgbPath = sgbPaths[i];
                
                List<HousingExteriorFixtureModel> modelsList = new List<HousingExteriorFixtureModel>();

                foreach (SgbGroup group in sgb.Data.OfType<SgbGroup>())
                {
                    foreach (SgbModelEntry mdl in group.Entries.OfType<SgbModelEntry>())
                    {
                        modelsList.Add(ReadTransformedModelForModelInfo(mdl.Model));
                    }
                }
                thisVariant.models = modelsList.ToArray();
                variants[i] = thisVariant;
            }

            return variants;
        }

        /// <summary>
        /// Returns an appropriate HousingExteriorFixtureModel corresponding to this TransformedModel.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static HousingExteriorFixtureModel ReadTransformedModelForModelInfo(TransformedModel t)
        {
            HousingExteriorFixtureModel thisModel = new HousingExteriorFixtureModel();

            thisModel.modelPath = t.Model.File.Path;
            thisModel.modelName = thisModel.modelPath.Substring(thisModel.modelPath.LastIndexOf('/') + 1);
            thisModel.modelName = Path.GetFileNameWithoutExtension(thisModel.modelName);
            thisModel.numMeshes = t.Model.GetModel(ModelQuality.High).Meshes.Length;
            thisModel.transform = TransformFromVectors(t.Translation, t.Rotation, t.Scale);

            return thisModel;
        }

        /// <summary>
        /// Returns a HousingExteriorBlueprintSet read from the small, medium, and large sgb paths in
        /// HousingExteriorBlueprintSet.
        /// </summary>
        /// <param name="realm"></param>
        /// <returns></returns>
        private static HousingExteriorBlueprintSet ReadExteriorBlueprintSet(ARealmReversed realm)
        {
            HousingExteriorBlueprintSet thisSet = new HousingExteriorBlueprintSet();
            thisSet.set = new HousingExteriorBlueprint[HousingExteriorBlueprintSet.SgbPaths.Length];

            for (int i = 0; i < HousingExteriorBlueprintSet.SgbPaths.Length; i++)
            {
                string thisPath = HousingExteriorBlueprintSet.SgbPaths[i];
                HousingExteriorBlueprint thisBlueprint = new HousingExteriorBlueprint { size = (Size) i};

                //These are hardcoded, double check they're there
                SaintCoinach.IO.File f;
                if (!realm.Packs.TryGetFile(thisPath, out f))
                    throw new FileNotFoundException();

                SgbFile sgb = new SgbFile(f);

                foreach (SgbGroup group in sgb.Data.OfType<SgbGroup>())
                {
                    foreach (SgbGimmickEntry gim in group.Entries.OfType<SgbGimmickEntry>())
                    {
                        string gimmickPath = gim.Gimmick.File.Path;

                        /*
                         * Group 1: s1h0 or opt
                         * Group 2: size, dor/wid variant, f for fence, or opt's 2chars
                         * Group 3: fixturetype or m for opt
                         * Group 4: a/b/c/d for fence, . for all others
                         */
                        Regex pattern = new Regex(@"asset\/(.*)?_(.{1,2})_(.{1,3})([0-9]{4})([a-z]|\.)");
                        Match m = pattern.Match(gimmickPath);

                        if (!m.Success)
                            continue;
                        
                        //Obtain fixture type
                        string fType = m.Groups[3].Value;
                        if (fType == "m")
                            fType = "o" + m.Groups[2].Value;
                        FixtureType fixtureType = (FixtureType) Enum.Parse(typeof(FixtureType), fType);

                        //Obtains the variant string, examples: s, m, l, co, ca, ci, a, b, c, d
                        string strVariant = m.Groups[5].Value == "." ? m.Groups[2].Value : m.Groups[5].Value;
                        int variant = 0;

                        //Attempt to parse group 2 into a Size to get variant
                        if (!Enum.TryParse(strVariant, out Size fixtureSize))
                        {
                            //If we can't parse variant to a size, we have to parse into a different fixture variant
                            if (fixtureType == FixtureType.dor)
                                variant = (int) Enum.Parse(typeof(DoorVariants), strVariant);
                            else if (fixtureType == FixtureType.wid)
                                variant = (int) Enum.Parse(typeof(WindowVariants), strVariant);
                            else if (fixtureType == FixtureType.fnc)
                                variant = (int)Enum.Parse(typeof(FenceVariants), strVariant);
                        }
                        else
                        {   
                            //If rof/wal is not same size as blueprint, skip it
                            if (fixtureSize != thisBlueprint.size)
                                continue;
                        }

                        if (thisBlueprint.fixtureTransforms[fixtureType][variant] == null)
                            thisBlueprint.fixtureTransforms[fixtureType][variant] = new List<Transform>();

                        List<Transform> listForVariant = thisBlueprint.fixtureTransforms[fixtureType][variant];

                        Transform t = TransformFromVectors(gim.Header.Translation, gim.Header.Rotation,
                            gim.Header.Scale);
                        
                        listForVariant.Add(t);

                        #region switch statement bad
//                        switch (m.Groups[3].Value)
//                        {
//                            case nameof(FixtureType.rof):
//                                if (m.Groups[2].Value == size.ToString())
//                                {
//                                    //exampe but cool
//                                }
//                                break;
//                            case nameof(FixtureType.wal):
//                                //Do stuff
//                                break;
//                            case nameof(FixtureType.wid):
//                                //Do stuff
//                                break;
//                            case nameof(FixtureType.dor):
//                                //Do stuff
//                                break;
//                            case "m":
//                                switch ("o" + m.Groups[2].Value)
//                                {
//                                    case nameof(FixtureType.orf):
//                                        //Do stuff
//                                        break;
//                                    case nameof(FixtureType.owl):
//                                        //Do stuff
//                                        break;
//                                    case nameof(FixtureType.osg):
//                                        //Do stuff
//                                        break;
//                                }
//                                break;
//                            case nameof(FixtureType.fnc):
//                                //Do stuff
//                                break;
//                        }
                        #endregion
                    }
                }
                /*
                 * The transforms relevant to the current blueprint come first,
                 * so figure out how many Transforms aren't for this size house
                 * by checking how many transforms are collectively in the blueprints
                 * below this size and variant, then remove that number of entries.
                 */

                if (i > 0)
                {
                    int[][] numTransformsInSmallerBlueprints = new int[Enum.GetValues(typeof(FixtureType)).Length][];
                    //For every smaller blueprint
                    for (int smallerBlueprintIndex = 0; smallerBlueprintIndex < i; smallerBlueprintIndex++)
                    {
                        //For every fixture type
                        for (int fixtureTypeIndex = 0; fixtureTypeIndex < numTransformsInSmallerBlueprints.Length; fixtureTypeIndex++)
                        {
                            FixtureType fixtureType = (FixtureType)fixtureTypeIndex + 1;
                            int numberOfVariants = HousingExteriorFixture.GetVariants(fixtureType);

                            if (numTransformsInSmallerBlueprints[fixtureTypeIndex] == null)
                                numTransformsInSmallerBlueprints[fixtureTypeIndex] = new int[numberOfVariants];

                            for (int variantIndex = 0; variantIndex < numberOfVariants; variantIndex++)
                            {
                                int? toAddn = thisSet.set[smallerBlueprintIndex].fixtureTransforms[fixtureType][variantIndex]?.Count;
                                int toAdd = toAddn ?? 0;

                                numTransformsInSmallerBlueprints[fixtureTypeIndex][variantIndex] += toAdd;
                            }
                        }
                    }

                    //For every fixture type
                    for (int fixtureTypeIndex = 0; fixtureTypeIndex < numTransformsInSmallerBlueprints.Length; fixtureTypeIndex++)
                    {
                        FixtureType fixtureType = (FixtureType)fixtureTypeIndex + 1;
                        int numberOfVariants = HousingExteriorFixture.GetVariants(fixtureType);

                        for (int variantIndex = 0; variantIndex < numberOfVariants; variantIndex++)
                        {
                            List<Transform> variantTransformsList = thisBlueprint.fixtureTransforms[fixtureType][variantIndex];

                            if (thisBlueprint.fixtureTransforms[fixtureType][variantIndex] == null)
                                continue;
                            if (variantTransformsList == null)
                                continue;
                            
                            int numTransforms = variantTransformsList.Count;
                            int difference = numTransforms - numTransformsInSmallerBlueprints[fixtureTypeIndex][variantIndex];

                            if (difference > 0)
                            {
                                List<Transform> newTransforms = new List<Transform>();
                                for (int tCount = 0; tCount < difference; tCount++)
                                {
                                    newTransforms.Add(variantTransformsList[tCount]);
                                }
                                thisBlueprint.fixtureTransforms[fixtureType][variantIndex] = newTransforms;
                            }
                        }
                    }
                }
                thisSet.set[i] = thisBlueprint;
            }
            return thisSet;
        }

        /// <summary>
        /// Returns a Map containing all groups in the Territory instantiated via
        /// the given TerritoryType.
        /// </summary>
        /// <param name="teriType"></param>
        /// <returns></returns>
        private static Map ReadTerritory(TerritoryType teriType)
        {
            Map map = new Map();
            Territory teri = new Territory(teriType);
                        
            if (teri.Terrain != null)
            {
                MapGroup terrainMapGroup = new MapGroup(MapGroup.GroupType.TERRAIN, "teri");
                terrainMapGroup.groupTransform = Transform.Empty;

                foreach (TransformedModel mdl in teri.Terrain.Parts)
                {
                    int modelId = map.TryAddUniqueModel(mdl.Model.ToMapModel());
                    terrainMapGroup.AddEntry(mdl.ToMapModelEntry(modelId));
                }
                map.AddMapGroup(terrainMapGroup);
            }
            
            foreach (LgbFile lgbFile in teri.LgbFiles)
            {
                var validGroups = lgbFile.Groups.Where(_ => !EventCheck(_.Name)).Select(_ => _);

                foreach (LgbGroup lgbGroup in validGroups)
                {
                    MapGroup lgbMapGroup = new MapGroup(MapGroup.GroupType.LGB, lgbGroup.Name);
                    lgbMapGroup.groupTransform = Transform.Empty;
                    
                    foreach (var mdl in lgbGroup.Entries.OfType<LgbModelEntry>())
                    {
                        int modelId = map.TryAddUniqueModel(mdl.Model.Model.ToMapModel());
                        lgbMapGroup.AddEntry(mdl.Model.ToMapModelEntry(modelId));
                    }

                    foreach (var gim in lgbGroup.Entries.OfType<LgbGimmickEntry>())
                    {
                        MapGroup gimMapGroup = new MapGroup(MapGroup.GroupType.SGB, GetGimmickName(gim.Name, gim.Gimmick.File.Path));
                        gimMapGroup.groupTransform = TransformFromGimmickHeader(gim.Header);

                        AddSgbModelsToMap(ref map, ref gimMapGroup, gim.Gimmick);

                        foreach (var rootGimGroup in gim.Gimmick.Data.OfType<SgbGroup>())
                        {
                            foreach (var rootGimEntry in rootGimGroup.Entries.OfType<SgbGimmickEntry>())
                            {
                                if (rootGimEntry.Gimmick != null)
                                {
                                    MapGroup rootGimMapGroup = new MapGroup(MapGroup.GroupType.SGB, GetGimmickName(rootGimEntry.Name, rootGimEntry.Gimmick.File.Path));
                                    rootGimMapGroup.groupTransform = TransformFromGimmickHeader(rootGimEntry.Header);
                                    
                                    AddSgbModelsToMap(ref map, ref rootGimMapGroup, rootGimEntry.Gimmick);
                                    
                                    foreach (var subGimGroup in rootGimEntry.Gimmick.Data.OfType<SgbGroup>())
                                    {
                                        foreach (var subGimEntry in subGimGroup.Entries.OfType<SgbGimmickEntry>())
                                        {
                                            MapGroup subGimMapGroup = new MapGroup(MapGroup.GroupType.SGB, GetGimmickName(subGimEntry.Name, subGimEntry.Gimmick.File.Path));
                                            subGimMapGroup.groupTransform = TransformFromGimmickHeader(subGimEntry.Header);

                                            AddSgbModelsToMap(ref map, ref subGimMapGroup, subGimEntry.Gimmick);

                                            rootGimMapGroup.AddGroup(subGimMapGroup);
                                        }
                                    }
                                    gimMapGroup.AddGroup(rootGimMapGroup);
                                }
                            }
                        }
                        lgbMapGroup.AddGroup(gimMapGroup);
                    }
                    map.AddMapGroup(lgbMapGroup);
                }
            }
            return map;
        }

        /// <summary>
        /// Returns the gimmick group name if not empty. If it is empty,
        /// returns the gimmick's filename without extension.
        /// </summary>
        /// <param name="gimmickName"></param>
        /// <param name="gimmickPath"></param>
        /// <returns></returns>
        private static string GetGimmickName(string gimmickName, string gimmickPath)
        {
            if (String.IsNullOrEmpty(gimmickName))
            {
                return gimmickPath.Substring(gimmickPath.LastIndexOf('\\') + 1).Replace(".sgb", "");
            }
            return gimmickName;
        }

        /// <summary>
        /// Parses an SgbFile for model entries or further gimmicks, and adds groups
        /// to the given List&lt;MapModelEntry&gt;.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="parent"></param>
        /// <param name="models"></param>
        private static void AddSgbModelsToMap(ref Map map, ref MapGroup mg, SgbFile file)
        {
            foreach (var sgbGroup in file.Data.OfType<SgbGroup>())
            {
                foreach (var mdl in sgbGroup.Entries.OfType<SgbModelEntry>())
                {
                    int modelId = map.TryAddUniqueModel(mdl.Model.Model.ToMapModel());
                    mg.AddEntry(mdl.Model.ToMapModelEntry(modelId));
                }
            }
        }

        /// <summary>
        /// Returns true if the given LgbGroup name is for use with an in-game event.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static bool EventCheck(string s)
        {
            return (s.Contains("anniversary") ||
                    s.Contains("christmas") ||
                    s.Contains("china") ||
                    s.Contains("easter") ||
                    s.Contains("goldsaucer") ||
                    s.Contains("halloween") ||
                    s.Contains("korea") ||
                    s.Contains("newyear") ||
                    s.Contains("princess") ||
                    s.Contains("summer") ||
                    s.Contains("valentine"));
        }

        public static void WriteOutWardInfo(ARealmReversed realm)
        {
            List<Plot> plots = new List<Plot>();
            ReadLandSetSheet(realm, ref plots);
            ReadTerritoryPlots(realm, ref plots);

            string outpath = FFXIVHSPaths.GetWardInfoJson();

            if (File.Exists(outpath))
                File.Delete(outpath);

            string json = JsonConvert.SerializeObject(plots, Formatting.Indented);

            File.WriteAllText(outpath, json);
        }

        public static void WriteOutHousingExteriorInfo(ARealmReversed realm)
        {
            string outpath = FFXIVHSPaths.GetHousingExteriorJson();

            if (File.Exists(outpath))
            {
                WriteOutHousingExteriorModels(realm);
                return;
            }

            Dictionary<int, HousingExteriorFixture> fixtures = ReadHousingExteriorSheet(realm);

            string json = JsonConvert.SerializeObject(fixtures, Formatting.Indented);

            File.WriteAllText(outpath, json);
        }

        public static void WriteOutHousingExteriorModels(ARealmReversed realm)
        {
            string inpath = FFXIVHSPaths.GetHousingExteriorJson();
            if (!File.Exists(inpath))
                throw new FileNotFoundException();

            string outpath = FFXIVHSPaths.GetHousingExteriorObjectsDirectory();

            string jsonText = File.ReadAllText(inpath);

            Dictionary<int, HousingExteriorFixture> fixtures =
                JsonConvert.DeserializeObject<Dictionary<int, HousingExteriorFixture>>(jsonText);

            foreach (HousingExteriorFixture fixture in fixtures.Values)
            {
                foreach (HousingExteriorFixtureVariant variant in fixture.variants)
                {
                    foreach (HousingExteriorFixtureModel model in variant.models)
                    {
                        if (realm.Packs.TryGetFile(model.modelPath, out SaintCoinach.IO.File f))
                            ObjectFileWriter.WriteObjectFile(outpath, (ModelFile) f);
                    }
                }
            }
        }

        public static void WriteBlueprints(ARealmReversed realm)
        {
            HousingExteriorBlueprintSet blueprintSet = ReadExteriorBlueprintSet(realm);

            string outpath = FFXIVHSPaths.GetHousingExteriorBlueprintSetJson();
            if (File.Exists(outpath))
                File.Delete(outpath);

            string json = JsonConvert.SerializeObject(blueprintSet, Formatting.Indented);

            File.WriteAllText(outpath, json);
        }

        public static void WriteMap(ARealmReversed realm, TerritoryType teriType)
        {
            Ward ward = Plot.StringToWard(teriType.Name);

            string outpath = FFXIVHSPaths.GetWardJson(ward);

            if (File.Exists(outpath))
            {
                WriteMapModels(realm, teriType);
                return;
            }

            Map map = ReadTerritory(teriType);

            string json = JsonConvert.SerializeObject(map, Formatting.Indented);

            File.WriteAllText(outpath, json);
        }

        public static void WriteMapModels(ARealmReversed realm, TerritoryType teriType)
        {
            Ward ward = Plot.StringToWard(teriType.Name);

            string inpath = FFXIVHSPaths.GetWardJson(ward);
            if (!File.Exists(inpath))
                throw new FileNotFoundException();

            string outpath = FFXIVHSPaths.GetWardObjectsDirectory(ward);

            string json = File.ReadAllText(inpath);
                
            Map map = JsonConvert.DeserializeObject<Map>(json);

            foreach (MapModel model in map.models.Values)
            {
                if (realm.Packs.TryGetFile(model.modelPath, out SaintCoinach.IO.File f))
                    ObjectFileWriter.WriteObjectFile(outpath, (ModelFile)f);
            }
        }
    }

    public static class VectorConverter
    {
        public static SharpDX.Vector3 ToDx(this SaintCoinach.Graphics.Vector3 self)
        {
            return new SharpDX.Vector3
            {
                X = self.X,
                Y = self.Y,
                Z = self.Z
            };
        }
        public static SharpDX.Vector3 ToDx(this SaintCoinach.Graphics.Vector3? self, SharpDX.Vector3 defaultValue)
        {
            if (self.HasValue)
                return self.Value.ToDx();
            return defaultValue;
        }
        public static SharpDX.Vector4 ToDx(this SaintCoinach.Graphics.Vector4 self)
        {
            return new SharpDX.Vector4
            {
                X = self.X,
                Y = self.Y,
                Z = self.Z,
                W = self.W
            };
        }
        public static SharpDX.Vector3 ToDx3(this SaintCoinach.Graphics.Vector4 self)
        {
            return new SharpDX.Vector3
            {
                X = self.X,
                Y = self.Y,
                Z = self.Z
            };
        }
        public static SharpDX.Vector4 ToDx(this SaintCoinach.Graphics.Vector4? self, SharpDX.Vector4 defaultValue)
        {
            if (self.HasValue)
                return self.Value.ToDx();
            return defaultValue;
        }
    }
}



