﻿using CommonControls.Common;
using CommonControls.Editors.AnimationPack;
using CommonControls.FileTypes.AnimationPack;
using CommonControls.FileTypes.AnimationPack.AnimPackFileTypes;
using CommonControls.FileTypes.MetaData;
using CommonControls.FileTypes.MetaData.Definitions;
using CommonControls.FileTypes.PackFiles.Models;
using CommonControls.Services;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wh3AnimPackCreator
{
    partial class Program
    {


        static void Main(string[] args)
        {
            MetaDataTagDeSerializer.EnsureMappingTableCreated();

            var troyGameSettings = new ApplicationSettingsService().CurrentSettings.GameDirectories.First(x => x.Game == GameTypeEnum.Troy);
            var troyPfs = new PackFileService(new PackFileDataBase(), new SkeletonAnimationLookUpHelper(), new ApplicationSettingsService());
            troyPfs.LoadAllCaFiles(troyGameSettings.Path, troyGameSettings.Game.ToString());

            var wh3Pfs = new PackFileService(new PackFileDataBase(), new SkeletonAnimationLookUpHelper(), new ApplicationSettingsService());
            var pfsContainer = wh3Pfs.CreateNewPackFileContainer("AnimResource_v0_cerberus", PackFileCAType.MOD);
            wh3Pfs.SetEditablePack(pfsContainer);
            var wh3AnimPack = new AnimationPackFile();

            try
            {
                //  var currentFragmentName = @"animations/animation_tables/cerb1_mth_dlc_cerberus.frg";
                var currentFragmentName = @"animations/animation_tables/ce1_myth_dlc_centaur_spear_and_shield.frg";
                AnimationTransferHelper instance = new AnimationTransferHelper(troyPfs, new TroyResourceSwapRules(), wh3Pfs, wh3AnimPack);
                instance.Run(currentFragmentName);

                // Copy the skeleton db
                var battleSkeletonTable = troyPfs.FindFile(@"db\battle_skeletons_tables\data__");
                SaveHelper.Save(wh3Pfs, @"db\battle_skeletons_tables\troy_data__", null, battleSkeletonTable.DataSource.ReadData());

                // Save the animPack
                var bytes = AnimationPackSerializer.ConvertToBytes(wh3AnimPack);
                SaveHelper.Save(wh3Pfs, @"animations/database/battle/bin/AnimPackTest.animpack", null, bytes);

                // Save the packfile
                wh3Pfs.Save(pfsContainer, "C:\\temp\\temp_animResources.pack", false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return;
            



           //
           //     return;
           //
           //
           //     // Create output packfile
           //     var outputPfs = new PackFileService(new PackFileDataBase(), null, new ApplicationSettingsService());
           //     
           //     AnimationPackFile outputAnimPackFile = new AnimationPackFile();
           //
           //
           //     
           //     PrintDebugInformation(troyPfs, currentFragmentName);
           //
           //     // Create output bin
           //     //var currentOutputAnimBin = new AnimationBinWh3(Path.GetFileNameWithoutExtension(currentFragmentName));
           //     
           //     // Do the work
           //     var animContainer = GetAnimationContainers(troyPfs, currentFragmentName);
           //
           //     var groupedSlots = animContainer.FragmentFile.Fragments.GroupBy(x => x.Slot.Value).ToList();
           //
           //     var animFilesToCopy = new List<string>();
           //     var metaFilesToCopy = new List<string>();
           //     foreach (var groupedSlot in groupedSlots)
           //     {
           //         Console.WriteLine($"\t {groupedSlot.Key}[{groupedSlot.Count()}]");
           //
           //         foreach (var slot in groupedSlot)
           //         {
           //             if (string.IsNullOrWhiteSpace(slot.AnimationFile) == false)
           //                 animFilesToCopy.Add(slot.AnimationFile);
           //
           //             if (string.IsNullOrWhiteSpace(slot.MetaDataFile) == false)
           //                 metaFilesToCopy.Add(slot.MetaDataFile);
           //         }
           //     }
           //
           //     var distinctAnimFiles = animFilesToCopy.Distinct();
           //     Console.WriteLine($"AnimFiles {distinctAnimFiles.Count()}:");
           //     distinctAnimFiles.ForEach(i => Console.WriteLine($"\t {i}"));
           //
           //     var distinctMetaFiles = metaFilesToCopy.Distinct();
           //     Console.WriteLine($"MetaFiles {distinctMetaFiles.Count()}:");
           //     distinctMetaFiles.ForEach(i => Console.WriteLine($"\t {i}"));
           //
           //     // Add the packfile
           //
           //
           //     //outputAnimPackFile.
           //
           // }
           // catch (Exception e)
           // {
           //     Console.WriteLine(e.Message);
           // }
           //
           //
           //var wh3 = new InformationContainer(GameTypeEnum.Warhammer3);
           //var troy = new InformationContainer(GameTypeEnum.Troy);


        }


        static (AnimationBin AnimBin, AnimationFragmentFile FragmentFile) GetAnimationContainers(PackFileService pfs, string fragmentName)
        {
            var gameAnimPackFile = pfs.FindFile(@"animations\animation_tables\animation_tables.animpack");
            var gameAnimPack = AnimationPackSerializer.Load(gameAnimPackFile, pfs);
            var animBin = gameAnimPack.Files.First(x => x.FileName == @"animations/animation_tables/animation_tables.bin") as AnimationBin;
            var fragment = gameAnimPack.Files.First(x => x.FileName == fragmentName) as AnimationFragmentFile;

            return (animBin, fragment);
        }


        static void PrintDebugInformation(PackFileService pfs, string fragmentName)
        {
            try
            {
                var metaParser = new MetaDataFileParser();
                var currentFragmentName = fragmentName;
                Console.Clear();
                Console.WriteLine($"Starting Debug Print - {currentFragmentName}");

                var slotCreator = new BaseAnimationSlotHelper(GameTypeEnum.Troy);
                var gameAnimPackFile = pfs.FindFile(@"animations\animation_tables\animation_tables.animpack");
                var gameAnimPack = AnimationPackSerializer.Load(gameAnimPackFile, pfs, slotCreator);
                var animBin = gameAnimPack.Files.First(x => x.FileName == @"animations/animation_tables/animation_tables.bin") as AnimationBin;
                var fragment = gameAnimPack.Files.First(x => x.FileName == currentFragmentName) as AnimationFragmentFile;

                var allMetaTags = new List<string>();
                var allEffects = new List<string>();
                var allSoundEvents = new List<string>();
                var missingSlots = new List<string>();

                foreach (var slot in fragment.Fragments)
                {
                    var animationName = slot.AnimationFile;
                    var meta = slot.MetaDataFile;
                    var sound = slot.SoundMetaDataFile;


                    var wh3SlotName = AnimationSlotTypeHelperWh3.GetfromValue(slot.Slot.Value);
                    if (wh3SlotName == null)
                        missingSlots.Add(slot.Slot.Value);

                    if (string.IsNullOrWhiteSpace(meta) == false)
                    {
                        var metaPackFile = pfs.FindFile(meta);
                        if (metaPackFile != null)
                        {
                            var metaFile = metaParser.ParseFile(metaPackFile.DataSource.ReadData());
                            foreach (var metaEntry in metaFile.Items)
                            {
                                allMetaTags.Add(metaEntry.DisplayName);

                                if (metaEntry is Effect_v11 effectMeta)
                                    allEffects.Add(effectMeta.VfxName);
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(sound) == false)
                    {
                        var metaPackFile = pfs.FindFile(sound);
                        if (metaPackFile != null)
                        {
                            var metaFile = metaParser.ParseFile(metaPackFile.DataSource.ReadData());
                            foreach (var metaEntry in metaFile.Items)
                            {
                                allMetaTags.Add(metaEntry.DisplayName);

                                if (metaEntry is SoundTrigger_v10 soundMeta)
                                    allSoundEvents.Add(soundMeta.SoundEvent);
                            }
                        }
                    }
                }

                // Print data
                var distinctMetaTags = allMetaTags.Distinct();
                var distinctEffects = allEffects.Distinct();
                var distinctSoundEvents = allSoundEvents.Distinct();
                var distinctMissingSlots = missingSlots.Distinct();

                Console.WriteLine("\t MetaTags:");
                foreach (var item in distinctMetaTags)
                    Console.WriteLine($"\t\t {item}");

                Console.WriteLine("\n\t Effects:");
                foreach (var item in distinctEffects)
                    Console.WriteLine($"\t\t {item}");

                Console.WriteLine("\n\t SoundEvents:");
                foreach (var item in distinctSoundEvents)
                    Console.WriteLine($"\t\t {item}");

                Console.WriteLine("\n\t MissingSlots:");
                foreach (var item in distinctMissingSlots)
                    Console.WriteLine($"\t\t {item}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine($"Done Debug Print");
        }
    }


    class InformationContainer
    {
        public PackFileService Pfs { get; set; }

        public InformationContainer(GameTypeEnum game)
        {
            var settings = new ApplicationSettingsService();
            var gameSettings = settings.CurrentSettings.GameDirectories.First(x => x.Game == game);

            Pfs = new PackFileService(new PackFileDataBase(), null, settings) ;
            Pfs.LoadAllCaFiles(gameSettings.Path, gameSettings.Game.ToString());
        }


        /*public void Create()
        {
            var gameName = GameInformationFactory.GetGameById(_settingsService.CurrentSettings.CurrentGame).DisplayName;
            var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var gameOutputDir = $"{DirectoryHelper.ReportsDirectory}\\MetaData\\{gameName}_{timeStamp}\\";
            var gameOutputDirFailed = $"{gameOutputDir}\\Failed\\";
            if (Directory.Exists(gameOutputDir))
                Directory.Delete(gameOutputDir, true);
            DirectoryHelper.EnsureCreated(gameOutputDir);
            DirectoryHelper.EnsureCreated(gameOutputDirFailed);

            var output = new Dictionary<string, FileReport>();

            var fileList = Pfs.FindAllWithExtentionIncludePaths(".meta");
            var failedFiles = new List<string>();

            var metaTable = new List<(string Path, MetaDataFile File)>();
            for (int i = 0; i < fileList.Count; i++)
            {
                var fileName = fileList[i].FileName;
                var packFile = fileList[i].Pack;
                try
                {
                    var data = packFile.DataSource.ReadData();
                    if (data.Length == 0)
                        continue;

                    var parser = new MetaDataFileParser();
                    var metaData = parser.ParseFile(data);
                    metaTable.Add((fileName, metaData));

                    var completedTags = 0;
                    foreach (var item in metaData.Items)
                    {
                        var tagName = item.Name + "_v" + item.Version;
                        tagName = tagName.ToLower();

                        if (output.ContainsKey(tagName) == false)
                            output[tagName] = new FileReport();

                        try
                        {
                            var variables = MetaDataTagDeSerializer.DeSerializeToStrings(item, out var errorMessage);
                            if (variables != null)
                            {

                                if (output[tagName].CompletedFiles.Count == 0)
                                {
                                    foreach (var variable in variables)
                                        output[tagName].Headers.Add(variable.Header);
                                }

                                var variableValues = variables.Select(x => x.Value).ToList();
                                variableValues.Insert(0, fileName);
                                variableValues.Insert(1, "");

                                output[tagName].CompletedFiles.Add(variableValues);
                                completedTags++;
                            }
                            else
                            {
                                var variableValues = new List<string>() { fileName, errorMessage, item.Data.Length.ToString() };
                                output[tagName].FailedFiles.Add(variableValues);
                            }
                        }
                        catch (Exception e)
                        {
                            var variableValues = new List<string>() { fileName, e.Message, item.Data.Length.ToString() };
                            output[tagName].FailedFiles.Add(variableValues);
                        }
                    }

                    _logger.Here().Information($"File processed {i}/{fileList.Count} - {completedTags}/{metaData.Items.Count} tags loaded correctly");
                }
                catch
                {
                    _logger.Here().Information($"File processed {i}/{fileList.Count} - Parsing failed completly");
                    failedFiles.Add(fileName);
                }
            }

            // Write the data 
            foreach (var item in output)
            {
                if (item.Value.CompletedFiles.Count != 0)
                {
                    var content = new StringWriter();
                    content.WriteLine("sep=|");
                    content.WriteLine(string.Join("|", item.Value.Headers));
                    foreach (var competed in item.Value.CompletedFiles)
                        content.WriteLine(string.Join("|", competed));

                    var fileName = gameOutputDir + item.Key + $"_{item.Value.CompletedFiles.Count}.csv";
                    File.WriteAllText(fileName, content.ToString());
                }

                if (item.Value.FailedFiles.Count != 0)
                {
                    var content = new StringWriter();
                    content.WriteLine("sep=|");
                    content.WriteLine("FileName|Error|DataLength");
                    foreach (var failed in item.Value.FailedFiles)
                        content.WriteLine(string.Join("|", failed));

                    var fileName = gameOutputDirFailed + item.Key + $"_{item.Value.FailedFiles.Count}.csv";
                    File.WriteAllText(fileName, content.ToString());
                }
            }

            var summaryContent = new StringWriter();
            summaryContent.WriteLine("sep=|");
            summaryContent.WriteLine("Tag|Completed|Failed|Ratio");
            foreach (var item in output)
            {
                var str = $"{item.Key}| {item.Value.CompletedFiles.Count}| {item.Value.FailedFiles.Count} |{item.Value.FailedFiles.Count}/{item.Value.CompletedFiles.Count + item.Value.FailedFiles.Count}";
                _logger.Here().Information(str);
                summaryContent.WriteLine(str);
            }
            var summaryFileName = gameOutputDir + "Summary.csv";
            File.WriteAllText(summaryFileName, summaryContent.ToString());

            var commonHeaderContent = new StringWriter();
            commonHeaderContent.WriteLine("sep=|");
            commonHeaderContent.WriteLine("Type|FileName|Error|Version|StartTime|EndTime|Filter|Id");
            foreach (var item in output)
            {
                foreach (var competed in item.Value.CompletedFiles)
                {
                    commonHeaderContent.Write(item.Key + "|");
                    commonHeaderContent.WriteLine(string.Join("|", competed.Take(7)));
                }
            }

            var commonHeaderFile = gameOutputDir + "CommonHeader.csv";
            File.WriteAllText(commonHeaderFile, commonHeaderContent.ToString());

            MessageBox.Show($"Done - Created at {gameOutputDir}");
            Process.Start("explorer.exe", gameOutputDir);
        }*/

        void LoadMetaDataFiles()
        { 
        
        }

        void GetAllEffects()
        { }

        void GetAllSounds()
        { }

        void GetAllMetaDataTags()
        {

        }
    }
}
