using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FFXIVHSLib;
using SaintCoinach;
using SaintCoinach.Xiv;
using SaintCoinach.Graphics;
using SaintCoinach.Graphics.Lgb;
using SaintCoinach.Graphics.Sgb;
using SaintCoinach.Imaging;
using Directory = System.IO.Directory;
using File = System.IO.File;
using Vector3 = SaintCoinach.Graphics.Vector3;

namespace FFXIVHSLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const bool debug = true;

        //This doesn't do anything.
        const string mapPath = "";

        private string territoryName = "";
        private SaintCoinach.ARealmReversed realm;
        private Territory territory;
        private StringBuilder maplist;

        public MainWindow()
        {
            InitializeComponent();
            
            realm = new ARealmReversed(FFXIVHSPaths.GetGameDirectory(), SaintCoinach.Ex.Language.English);

            if (!realm.IsCurrentVersion)
            {
                MessageBox.Show("Game ver: " + realm.GameVersion + Environment.NewLine +
                                "Def ver: " + realm.DefinitionVersion + Environment.NewLine +
                                "Updating...");
                realm.Update(true);
            }
            
            TerritoryType[] territoryTypes = realm.GameData.GetSheet<TerritoryType>().ToArray();
            List<TerritoryType> relevantTerritories = new List<TerritoryType>();
            
            foreach (TerritoryType t in territoryTypes)
            {
                if (!String.IsNullOrEmpty(t.PlaceName.ToString()))
                {
                    byte intendedUse = (byte)t.GetRaw("TerritoryIntendedUse");

                    //Housing intended use column value is 13
                    if (intendedUse == 13)// || intendedUse == 16 || intendedUse == 17)
                    {
                        relevantTerritories.Add(t);
                    }
                }
            }
            amountLabel.Content = (String)amountLabel.Content + relevantTerritories.Count;

            placeBox.ItemsSource = relevantTerritories;
            placeBox.DisplayMemberPath = "PlaceName";
            placeBox.SelectedValuePath = ".";
        }

        private void placeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TerritoryType selT = (TerritoryType) placeBox.SelectedValue;

            #region progressbar testing
            //            Thread lWindow = new Thread(delegate()
            //            {
            //                LoadingWindow l = new LoadingWindow();
            //
            //                double parentTop = Top;
            //                double parentLeft = this.Left;
            //
            //                l.Top = (parentTop - l.Height) / 2;
            //                l.Left = (parentLeft - l.Width) / 2;
            //
            //                l.ShowDialog();
            //                System.Windows.Threading.Dispatcher.Run();
            //            });
            //
            //            lWindow.SetApartmentState(ApartmentState.STA);
            //
            //            lWindow.Start();
            #endregion

            //territory = new Territory(selT);

            //            territoryName = selT.PlaceName.ToString();
            //territoryName = territory.Name;
            if (maplist != null)
                maplist.Clear();
            else
                maplist = new StringBuilder();
        }

        #region Old code, keeping it here while moving away from it

        //Does not support recursive sgb
        private void getFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            maplist.Clear();

            StringBuilder sb = new StringBuilder(mapPath);
            sb.Append(territoryName + @"\");

            String territoryFolder = sb.ToString();

            if (!Directory.Exists(territoryFolder))
                Directory.CreateDirectory(territoryFolder);

            if (territory.Terrain != null)
            {
                foreach (var part in territory.Terrain.Parts)
                {
                    addToMapList("territory", part);
                    addToFileList(part.Model.File.Path);
                }
            }

            // Get the files we need
            foreach (var lgbFile in territory.LgbFiles)
            {
                foreach (var group in lgbFile.Groups)
                {
                    if (!eventCheck(group.Name))
                    {
                        addHeaderToMapList("LGBGroup: " + group.Name);
                        foreach (var part in group.Entries)
                        {
                            var asMdl = part as SaintCoinach.Graphics.Lgb.LgbModelEntry;
                            var asGim = part as SaintCoinach.Graphics.Lgb.LgbGimmickEntry;

                            string path = "";
                            if (asMdl?.Model?.Model != null)
                            {
                                path = asMdl.Model.Model.File.Path;
                                addToMapList(group.Name, asMdl.Model);
                                addToFileList(path);
                            }

                            if (asGim?.Gimmick != null)
                            {
                                List<String> gimmickFileList = getGimmickPaths(asGim);

                                addGimmickInfoToMapList(asGim);
                                addGimmicksToMapList(group.Name, asGim);
                                addHeaderToMapList("GimmickEnd");

                                foreach (String gPath in gimmickFileList)
                                    addToFileList(gPath);
                            }

                            addToFileList(path);
                        }
                    }
                    
                }
            }
        }

        //Old method
        private void getFilesBtn_Click2(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(mapPath);
            sb.Append(territoryName + @"\");

            String territoryFolder = sb.ToString();

            if (!Directory.Exists(territoryFolder))
                Directory.CreateDirectory(territoryFolder);

            if (territory.Terrain != null)
            {
                foreach (var part in territory.Terrain.Parts)
                {
                    addToMapList("territory", part);
                    addToFileList(part.Model.File.Path);
                }
            }

            // Get the files we need
            foreach (var lgbFile in territory.LgbFiles)
            {
                foreach (var group in lgbFile.Groups)
                {
                    if (!eventCheck(group.Name))
                    {
                        addHeaderToMapList("LGBGroup: " + group.Name);
                        foreach (var entry in group.Entries)
                        {
                            var asMdl = entry as LgbModelEntry;
                            var asGim = entry as LgbGimmickEntry;

                            string path = "";

                            //Entry is model
                            if (asMdl != null && asMdl.Model != null && asMdl.Model.Model != null)
                            {
                                string header = string.Format("LgbModelEntry,{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                asMdl.Header.Translation.X,
                                                                asMdl.Header.Translation.Y,
                                                                asMdl.Header.Translation.Z,
                                                                asMdl.Header.Rotation.X,
                                                                asMdl.Header.Rotation.Y,
                                                                asMdl.Header.Rotation.Z,
                                                                asMdl.Header.Scale.X,
                                                                asMdl.Header.Scale.Y,
                                                                asMdl.Header.Scale.Z);
                                addHeaderToMapList(header);
                                path = asMdl.Model.Model.File.Path;
                                //MessageBox.Show(path);
                                addToMapList(group.Name, asMdl.Model);
                                addToFileList(path);
                                addHeaderToMapList("EndLgbModelEntry");
                            }

                            //Entry is gimmick
                            if (asGim != null && asGim.Gimmick != null)
                            {
                                List<String> gimmickFileList = new List<string>();
                                getGimmickPaths(asGim.Gimmick, ref gimmickFileList);

                                addGimmickInfoToMapList(asGim);
                                ExportSgbFile(group.Name, asGim.Gimmick, 0, asGim.Header.Translation,
                                    asGim.Header.Rotation, asGim.Header.Scale);
                                addHeaderToMapList("GimmickEnd");
                            }

                            addToFileList(path);
                        }
                    }

                }
            }
        }

        //Recursively obtains model paths from an sgb file
        private void getGimmickPaths(SgbFile file, ref List<String> list)
        {
            if (file == null)
                return;

            bool onec = false;

            foreach (var sgbGroup in file.Data.OfType<SgbGroup>())
            {
                //Entry is model
                foreach (var mdl in sgbGroup.Entries.OfType<SgbModelEntry>())
                {
                    addToFileList(mdl.Model.Model.File.Path);
                }

                //Entry is another Sgb
                foreach (var gimmickEntry in sgbGroup.Entries.OfType<SgbGimmickEntry>())
                {
                    getGimmickPaths(gimmickEntry.Gimmick, ref list);
                }

                //Entry is Sgb1C
                foreach (var sgb1c in sgbGroup.Entries.OfType<SgbGroup1CEntry>())
                {
                    if (!onec)
                    {
                        getGimmickPaths(sgb1c.Gimmick, ref list);
                        onec = true;
                    }
                }
            }
        }

        //Retrieves model entries from this gimmick entry
        private List<String> getGimmickPaths(LgbGimmickEntry entry)
        {
            List<String> gimmickFileList = new List<String>();

            SgbFile thisFile = entry.Gimmick;

            foreach (var iGroup in thisFile.Data)
            {
                SgbGroup eGroup = iGroup as SgbGroup;
                foreach (var iEntry in eGroup.Entries)
                {
                    SgbModelEntry eEntry = iEntry as SgbModelEntry;
                    if (eEntry != null)
                    {
                        gimmickFileList.Add(eEntry.Model.Model.File.Path);
                    }
                }
            }
            return gimmickFileList;
        }
        
        //Returns true if the given string (most likely lgbgroup) 
        public static bool eventCheck(String s)
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

        //Outputs to the large textbox on the interface
        private void addToFileList(String path)
        {
            bool exists = realm.Packs.FileExists(path);

            if (exists && !box.Text.Contains(path))
                box.AppendText(path + Environment.NewLine);
        }


        private void addGimmicksToMapList(String lgbGroupName, LgbGimmickEntry gim)
        {
            SgbFile thisFile = gim.Gimmick;
            LgbGimmickEntry.HeaderData hdr = gim.Header;
            foreach (var iGroup in thisFile.Data)
            {
                SgbGroup eGroup = iGroup as SgbGroup;
                foreach (var iEntry in eGroup.Entries)
                {
                    SgbModelEntry eEntry = iEntry as SgbModelEntry;
                    if (eEntry != null)
                    {
                        TransformedModel mdl = eEntry.Model;
                        TransformedModel newMdl;

                        Vector3 pos = new Vector3();
                        Vector3 rot = new Vector3();
                        //Scale is not added or multiplied

                        pos.X = mdl.Translation.X + hdr.Translation.X;
                        pos.Y = mdl.Translation.Y + hdr.Translation.Y;
                        pos.Z = mdl.Translation.Z + hdr.Translation.Z;
                        rot.X = mdl.Rotation.X + hdr.Rotation.X;
                        rot.Y = mdl.Rotation.Y + hdr.Rotation.Y;
                        rot.Z = mdl.Rotation.Z + hdr.Rotation.Z;

                        newMdl = new TransformedModel(mdl.Model, pos, rot, mdl.Scale);

                        addToMapList(lgbGroupName, newMdl);
                    }
                }
            }
        }

        //Adds gimmick info in the form of a header to the maplist, accepts LgbGimmicks
        private void addGimmickInfoToMapList(LgbGimmickEntry gim)
        {
            StringBuilder sb = new StringBuilder();

            Vector3 pos = gim.Header.Translation;
            Vector3 rot = gim.Header.Rotation;
            Vector3 scal = gim.Header.Scale;

            sb.AppendFormat("{0},{1},{2},", pos.X, pos.Y, pos.Z);
            sb.AppendFormat("{0},{1},{2},", rot.X, rot.Y, rot.Z);
            sb.AppendFormat("{0},{1},{2}", scal.X, scal.Y, scal.Z);

            addHeaderToMapList("Gimmick," + sb.ToString());
        }

        //Adds gimmick info in the form of a header to the maplist, accepts SgbGimmicks
        private void addGimmickInfoToMapList(SgbGimmickEntry gim, int depth)
        {
            StringBuilder sb = new StringBuilder();

            if (gim != null)
            {
                Vector3 pos = gim.Header.Translation;
                Vector3 rot = gim.Header.Rotation;
                Vector3 scal = gim.Header.Scale;

                sb.AppendFormat("{0},{1},{2},", pos.X, pos.Y, pos.Z);
                sb.AppendFormat("{0},{1},{2},", rot.X, rot.Y, rot.Z);
                sb.AppendFormat("{0},{1},{2}", scal.X, scal.Y, scal.Z);
            }

            addHeaderToMapList("Gimmick," + sb.ToString(), depth);
        }
        
        //Adds a header to the csv file written by maplist
        private void addHeaderToMapList(String header)
        {
            if (debug && !String.IsNullOrEmpty(header.Trim()))
            {
                maplist.Append("#" + header + Environment.NewLine);
            }
        }

        //For recursive sgb
        private void addHeaderToMapList(String header, int depth)
        {
            if (debug && !String.IsNullOrEmpty(header.Trim()))
            {
                for (int i = 0; i < depth; i++)
                    maplist.Append("\t");
                maplist.Append("#");
                maplist.Append(header + Environment.NewLine);
            }
        }

        //Adds the given TransformedModel to the maplist to be written out
        private void addToMapList(String lgbGroupName = null, TransformedModel mdl = null)
        {
            const char sep = ',';

            string path = mdl.Model.File.Path;

            if (lgbGroupName != null)
                maplist.Append(lgbGroupName);

            System.Numerics.Vector3 pos = new System.Numerics.Vector3();
            System.Numerics.Vector3 rot = new System.Numerics.Vector3();
            System.Numerics.Vector3 scal = new System.Numerics.Vector3();

            pos.X = mdl.Translation.X;
            pos.Y = mdl.Translation.Y;
            pos.Z = mdl.Translation.Z;
            rot.X = mdl.Rotation.X;
            rot.Y = mdl.Rotation.Y;
            rot.Z = mdl.Rotation.Z;
            scal.X = mdl.Scale.X;
            scal.Y = mdl.Scale.Y;
            scal.Z = mdl.Scale.Z;

            maplist.Append(sep);
            maplist.Append(path + sep);
            maplist.Append(pos.X);
            maplist.Append(sep);
            maplist.Append(pos.Y);
            maplist.Append(sep);
            maplist.Append(pos.Z);
            maplist.Append(sep);
            maplist.Append(rot.X);
            maplist.Append(sep);
            maplist.Append(rot.Y);
            maplist.Append(sep);
            maplist.Append(rot.Z);
            maplist.Append(sep);
            maplist.Append(scal.X);
            maplist.Append(sep);
            maplist.Append(scal.Y);
            maplist.Append(sep);
            maplist.Append(scal.Z);
            maplist.Append(Environment.NewLine);
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(mapPath);

            String pathToSave = sb.ToString();

            SaintCoinach.IO.File f;

            if (realm.Packs.FileExists(pathBox.Text.Trim()))
                f = realm.Packs.GetFile(pathBox.Text.Trim());
            else
                return;
            
            FileStream s = new FileStream(pathToSave + f.Path.Substring(f.Path.LastIndexOf("/") + 1), FileMode.Create);
            byte[] data = f.GetData();
            s.Write(data, 0, data.Length);
        }

        private void csvButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(mapPath);

            sb.Append(territoryName + @"\");

            String pathToSave = sb.ToString();

            if (!Directory.Exists(pathToSave))
                Directory.CreateDirectory(pathToSave);

            File.WriteAllText(pathToSave + "main.csv", maplist.ToString());
        }

        private void getObjFilesBtn_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(mapPath);
                
            sb.Append(territoryName + @"\");
            sb.Append(@"objects\");

            String pathToSave = sb.ToString();

            if (!Directory.Exists(pathToSave))
                Directory.CreateDirectory(pathToSave);

            String[] list = box.Text.Split('\n');

            foreach (String line in list)
            {
                if (!String.IsNullOrEmpty(line))
                {
                    ModelFile file = (ModelFile)realm.Packs.GetFile(line.Trim());
                    ObjectFileWriter.WriteObjectFile(pathToSave, file);
                }
            }
        }

        private void recSgbBtn_Click(object sender, RoutedEventArgs e)
        {
            if (maplist == null)
                maplist = new StringBuilder();

            maplist.Clear();
            getFilesBtn_Click2(sender, e);
        }

        void ExportSgbFile(String lgbGroup, SgbFile sgbFile, int depth, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            if (sgbFile == null)
                return;

            bool onec = false;

            addToFileList(sgbFile.File.Path);

            foreach (var sgbGroup in sgbFile.Data.OfType<SgbGroup>())
            {
                //Entry is model
                foreach (var mdl in sgbGroup.Entries.OfType<SgbModelEntry>())
                {
                    addToFileList(mdl.Model.Model.File.Path);

                    TransformedModel tMdl = mdl.Model;
                    TransformedModel newMdl;

                    Vector3 pos = new Vector3();
                    Vector3 rot = new Vector3();

                    pos.X = tMdl.Translation.X + translation.X;
                    pos.Y = tMdl.Translation.Y + translation.Y;
                    pos.Z = tMdl.Translation.Z + translation.Z;
                    rot.X = tMdl.Rotation.X + rotation.X;
                    rot.Y = tMdl.Rotation.Y + rotation.Y;
                    rot.Z = tMdl.Rotation.Z + rotation.Z;

                    newMdl = new TransformedModel(tMdl.Model, pos, rot, tMdl.Scale);
                    
                    addToMapList(lgbGroup, newMdl);
                }

                //Entry is another Sgb
                foreach (var gimmickEntry in sgbGroup.Entries.OfType<SaintCoinach.Graphics.Sgb.SgbGimmickEntry>())
                {
                    addGimmickInfoToMapList(gimmickEntry, depth);
                    ExportSgbFile(lgbGroup, gimmickEntry.Gimmick, depth + 1, gimmickEntry.Header.Translation,
                        gimmickEntry.Header.Rotation, gimmickEntry.Header.Scale);
                    addHeaderToMapList("GimmickEnd", depth);
                }

                //Entry is Sgb1C
                foreach (var sgb1c in sgbGroup.Entries.OfType<SgbGroup1CEntry>())
                {
                    if (!onec)
                    {
                        addHeaderToMapList("Gimmick1C", depth);
                        ExportSgbFile(lgbGroup, sgb1c.Gimmick, depth + 1, translation,
                            rotation, scale);
                        addHeaderToMapList("GimmickEnd", depth);
                        onec = true;
                    }
                }
            }
        }

        private void TextureOutBtn_Click(object sender, RoutedEventArgs e)
        {
            SaintCoinach.IO.File f = realm.Packs.GetFile(pathBox.Text.Trim());
            String filename = pathBox.Text.Trim().Replace("/", "_");

            ImageFile img = new ImageFile(f.Pack, f.CommonHeader);
            img.GetImage().Save(mapPath + filename + ".png");
        }

        //Uses box house SGB and replaces required sgbs with the given house ID's appropriate parts
        private void HouseRecSgb_Click(object sender, RoutedEventArgs e)
        {
            if (maplist == null)
                maplist = new StringBuilder();

            maplist.Clear();

            String[] poop = box.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            box.Clear();

            List<String> list = new List<string>();

            foreach (String line in poop)
            {
                if (realm.Packs.FileExists(line.Trim()))
                {
                    SaintCoinach.IO.File f = realm.Packs.GetFile(line.Trim());
                    SgbFile sgb = new SgbFile(f);

                    //ExportSgbFileUHousing(houseIDBox.Text, "null", sgb, 0, Vector3.Zero, Vector3.Zero, Vector3.One);
                }
            }

            foreach (String path in list)
            {
                box.Text += path + Environment.NewLine;
            }
        }

        private void ExportSgbFileUHousing(String desiredHouseID, String lgbGroup, SgbFile sgbFile, int depth, Vector3 translation, Vector3 rotation, Vector3 scale)
        {
            if (sgbFile == null)
                return;

            String housingPattern = @"bg/ffxiv/(est_e1|sea_s1|fst_f1|wil_w1)/hou/dyna/(.*?)/(0000)/asset/(e1h0|s1h0|f1h0|w1h0)_(.*?)(0000)(\w?).sgb";
            String housingPattern2 = @"bgcommon/hou/dyna/(e1h0|s1h0|f1h0|w1h0)/(.*?)/(0000)/asset/(e1h0|s1h0|f1h0|w1h0)_(.*?)(0000)(\w?).sgb";
            String housingPattern3 = @"bgcommon/hou/dyna/opt/(.*?)/(0000)/asset/opt_(.*?)(0000).sgb";

            //bool onec = false;

            foreach (var sgbGroup in sgbFile.Data.OfType<SgbGroup>())
            {
                //Entry is model
                foreach (var mdl in sgbGroup.Entries.OfType<SgbModelEntry>())
                {
                    addToFileList(mdl.Model.Model.File.Path);

                    TransformedModel tMdl = mdl.Model;
                    TransformedModel newMdl;

                    Vector3 pos = new Vector3();
                    Vector3 rot = new Vector3();

                    pos.X = tMdl.Translation.X + translation.X;
                    pos.Y = tMdl.Translation.Y + translation.Y;
                    pos.Z = tMdl.Translation.Z + translation.Z;
                    rot.X = tMdl.Rotation.X + rotation.X;
                    rot.Y = tMdl.Rotation.Y + rotation.Y;
                    rot.Z = tMdl.Rotation.Z + rotation.Z;

                    newMdl = new TransformedModel(tMdl.Model, pos, rot, tMdl.Scale);

                    addToMapList(lgbGroup, newMdl);
                }

                //Entry is another Sgb
                foreach (var gimmickEntry in sgbGroup.Entries.OfType<SgbGimmickEntry>())
                {
                    //Check against each pattern to turn it into the appropriate path
                    String gimmickFileName = gimmickEntry.Gimmick.File.Path;
                    addToFileList(gimmickFileName);
                    if (Regex.IsMatch(gimmickFileName, housingPattern))
                    {
                        String newGimmickPath = CreateUnitedHouseString(desiredHouseID,
                            Regex.Match(gimmickFileName, housingPattern));
                        SgbFile newGim = null;
                        if (realm.Packs.FileExists(newGimmickPath))
                            newGim = new SgbFile(realm.Packs.GetFile(newGimmickPath));
                        if (newGim != null)
                        {
                            ExportSgbFile(lgbGroup, newGim, depth + 1, gimmickEntry.Header.Translation,
                                gimmickEntry.Header.Rotation, gimmickEntry.Header.Scale);
                        }
                    }
                    else if (Regex.IsMatch(gimmickFileName, housingPattern2))
                    {
                        String newGimmickPath = CreateUnitedHouseString(desiredHouseID,
                            Regex.Match(gimmickFileName, housingPattern2));
                        SgbFile newGim = null;
                        if (realm.Packs.FileExists(newGimmickPath))
                            newGim = new SgbFile(realm.Packs.GetFile(newGimmickPath));
                        if (newGim != null)
                        {
                            ExportSgbFile(lgbGroup, newGim, depth + 1, gimmickEntry.Header.Translation,
                                gimmickEntry.Header.Rotation, gimmickEntry.Header.Scale);
                        }
                    }
                    else if (Regex.IsMatch(gimmickFileName, housingPattern3))
                    {
                        String newGimmickPath = CreateUnitedHouseString(desiredHouseID,
                            Regex.Match(gimmickFileName, housingPattern3));
                        SgbFile newGim = null;
                        if (realm.Packs.FileExists(newGimmickPath))
                            newGim = new SgbFile(realm.Packs.GetFile(newGimmickPath));
                        if (newGim != null)
                        {
                            ExportSgbFile(lgbGroup, newGim, depth + 1, gimmickEntry.Header.Translation,
                                gimmickEntry.Header.Rotation, gimmickEntry.Header.Scale);
                        }
                    }
                    else
                    {
                        addGimmickInfoToMapList(gimmickEntry, depth);
                        ExportSgbFileUHousing(desiredHouseID, lgbGroup, gimmickEntry.Gimmick, depth + 1, gimmickEntry.Header.Translation,
                            gimmickEntry.Header.Rotation, gimmickEntry.Header.Scale);
                        addHeaderToMapList("GimmickEnd", depth);
                    }
                }

                //Entry is Sgb1C
//                foreach (var sgb1c in sgbGroup.Entries.OfType<SgbGroup1CEntry>())
//                {
//                    if (!onec)
//                    {
//                        addHeaderToMapList("Gimmick1C", depth);
//                        ExportSgbFile(lgbGroup, sgb1c.Gimmick, depth + 1, translation,
//                            rotation, scale);
//                        addHeaderToMapList("GimmickEnd", depth);
//                        onec = true;
//                    }
//                }
            }
        }

        public static string CreateUnitedHouseString(String houseID, Match m)
        {
            StringBuilder sb = new StringBuilder("bgcommon/hou/dyna/");

            bool opt = m.Groups.Count < 7;

            if (opt)
                sb.Append("opt/");
            else
                sb.Append("com/");

            // Part type
            if (opt)
                sb.Append(m.Groups[1] + "/");
            else
                sb.Append(m.Groups[2] + "/");

            // House type ID
            int id = 0;
            if (opt)
            {
                id = Int32.Parse(houseID);
                id += 10;
                sb.AppendFormat("{0:D4}/", id);
            }
            else
                sb.Append(houseID + "/");

            // Fill in some path
            sb.Append("asset/");

            if (opt)
                sb.Append("opt_");
            else
                sb.Append("com_");

            // Part type format 2
            if (opt)
                sb.Append(m.Groups[3]);
            else
                sb.Append(m.Groups[5]);

            // House type ID again
            if (opt)
                sb.AppendFormat("{0:D4}", id);
            else
                sb.Append(houseID);

            // Suffix if necessary
            if (!String.IsNullOrEmpty(m.Groups[7].Value))
                sb.Append(m.Groups[7]);

            sb.Append(".sgb");

            return sb.ToString();
        }

        #endregion

        //Replaces all sgbs in box with their model contents
        private void ResolveBoxSgbs_Click(object sender, RoutedEventArgs e)
        {
            String[] sgbs = box.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            box.Clear();

            List<String> list = new List<string>();

            foreach (String line in sgbs)
            {
                if (realm.Packs.FileExists(line.Trim()))
                {
                    SaintCoinach.IO.File f = realm.Packs.GetFile(line.Trim());
                    SgbFile sgb = new SgbFile(f);

                    getGimmickPaths(sgb, ref list);
                }
            }

            foreach (String path in list)
            {
                box.Text += path + Environment.NewLine;
            }
        }
        
        private void existsBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            const string h = "Offset: ";
            existsCheckBtn.IsChecked = realm.Packs.FileExists(existsBox.Text.Trim());
            if (existsCheckBtn.IsChecked.Value)
                offsetLabel.Content = h + realm.Packs.GetFile(existsBox.Text.Trim()).GetHashCode().ToString("X");
            else
                offsetLabel.Content = h;
        }

        private void JsonWriteButton_Click(object sender, RoutedEventArgs e)
        {
            DataWriter.WriteOutWardInfo(realm);
        }

        private void ExteriorHousingJsonBtn_Click(object sender, RoutedEventArgs e)
        {
            DataWriter.WriteOutHousingExteriorInfo(realm);
        }

        private void BlueprintButton_Click(object sender, RoutedEventArgs e)
        {
            DataWriter.WriteBlueprints(realm);
        }

        private void ExtractMapBtn_Click(object sender, RoutedEventArgs e)
        {
            TerritoryType t = (TerritoryType) placeBox.SelectedValue;

            DataWriter.WriteMap(realm, t);
        }
    }
}
