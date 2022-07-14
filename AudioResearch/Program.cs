﻿using CommonControls.FileTypes.PackFiles.Models;
using CommonControls.FileTypes.Sound;
using CommonControls.FileTypes.Sound.WWise;
using CommonControls.FileTypes.Sound.WWise.Hirc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AudioResearch
{
    class Program
    {
        static string OnlyProcessThisFile = "";// "battle_individual_magic__warhammer3.bnk";

        static void Main(string[] args)
        {
            string[] allBnkFiles = GetAllBnkFiles();

            Console.WriteLine($"Found {allBnkFiles.Length} bnk files");
            allBnkFiles = FilterUnvantedFiles(allBnkFiles, out var removedFiles);
            Console.WriteLine($"Found {allBnkFiles.Length} bnk files after filtering");

            var globalSoundDatabase = new Dictionary<string, SoundDataBase>();
            var bnksWithError = new List<string>();
            var failedBnks = new List<(string bnkFile, string Error)>();

            var counter = 1;
            foreach (var bnkFile in allBnkFiles)
            {
                if (string.IsNullOrWhiteSpace(OnlyProcessThisFile) == false)
                {
                    if (bnkFile.Contains(OnlyProcessThisFile, StringComparison.InvariantCultureIgnoreCase) == false)
                        continue;
                }

                Console.WriteLine($"{counter++}/{allBnkFiles.Count()} - {bnkFile}");
                try
                {
                    var pf = new PackFile(bnkFile, new FileSystemSource(bnkFile));
                    var soundDb = Bnkparser.Parse(pf);

                    PrintHircData(soundDb.Hircs);

                    if (soundDb.Hircs.Count(y => (y is CAkUnknown) == true || y.HasError) != 0)
                        bnksWithError.Add(bnkFile);

                    globalSoundDatabase.Add(bnkFile, soundDb);
                    Console.WriteLine();
                }
                catch (Exception e)
                {
                    failedBnks.Add((bnkFile, e.Message));
                }
            }

            Console.WriteLine("Result:");
            var allHircs = globalSoundDatabase.SelectMany(x => x.Value.Hircs);
            PrintHircData(allHircs);

            Console.WriteLine("Hello World!");
        }



        static void PrintHircData(IEnumerable<HircItem> hircItems)
        {
            var unknownHirc = hircItems.Where(X => X is CAkUnknown).Count();
            var errorHirc = hircItems.Where(X => X.HasError).Count();
            Console.WriteLine($"\t Total HircObjects: {hircItems.Count()} Unknown: {unknownHirc} Decoding Errors:{errorHirc}");

            var grouped = hircItems.GroupBy(x => x.Type);
            var groupedWithError = grouped.Where(x => x.Count(y => (y is CAkUnknown) == true || y.HasError) != 0);
            var groupedWithoutError = grouped.Where(x => x.Count(y => (y is CAkUnknown) == false && y.HasError == false) != 0);

            Console.WriteLine("\t\t Correct:");
            foreach (var group in groupedWithoutError)
                Console.WriteLine($"\t\t\t {group.Key}: Count: {group.Count()}");

            if (groupedWithError.Any())
            {
                Console.WriteLine("\t\t Error:");
                foreach (var group in groupedWithError)
                    Console.WriteLine($"\t\t\t {group.Key}: {group.Where(x => (x is CAkUnknown) == true || x.HasError).Count()}/{group.Count()} Failed");
            }
        }

        static string[] FilterUnvantedFiles(string[] files, out string[] removedFiles)
        {
            var tempRemoveFiles = new List<string>();
            var fileList = files.ToList();

            // Files that contains multiple items not decoded.
            var removeFilter = new[]{ "media", "init.bnk", "animation_blood_data.bnk" };

            foreach (var file in fileList)
            {
                foreach (var removeName in removeFilter)
                {
                    if (file.Contains(removeName))
                    {
                        tempRemoveFiles.Add(file);
                        break;
                    }
                }
            }

            foreach (var item in tempRemoveFiles)
                fileList.Remove(item);

            removedFiles = tempRemoveFiles.ToArray();
            return fileList.ToArray();
        }

        static string GetExportedWWiseFolder()
        {
            var possibleFolders = new[] { @"C:\Users\ole_k\Desktop\Wh3 sounds\audio\wwise", @"c:\KlissiansFolders.." };
            foreach (var folder in possibleFolders)
            {
                if (Directory.Exists(folder))
                    return folder;
            }

            throw new Exception("Exported folder not found. Please add to the array above...");
        }

        private static string[] GetAllBnkFiles()
        {
            var allBnkFiles = Directory.GetFiles(GetExportedWWiseFolder(), "*.bnk", SearchOption.AllDirectories);
            if (allBnkFiles.Length != 611)
                throw new Exception("The export folder should contain 611 bnk files. (Core files + English Folder)");
            return allBnkFiles;
        }
    }


    /*
      void ParsBnkFiles(ExtenededSoundDataBase masterDb, NameLookupHelper nameHelper, List<PackFile> files, VisualEventOutputNode parent, Stopwatch timer)
        {
            for(int fileIndex = 0; fileIndex < files.Count; fileIndex++)
            {
                try
                {
                    var soundDb = Bnkparser.Parse(files[fileIndex]);

                    var events = soundDb.Hircs
                        .Where(x => x.Type == HircType.Event || x.Type == HircType.Dialogue_Event)
                        .Where(x => x.HasError == false);

                    var eventsCount = events.Count();
                    var fileNodeOutputStr = $"{files[fileIndex].Name} Total EventCount:{eventsCount}";
                    _logger.Here().Information($"{fileIndex}/{files.Count} {fileNodeOutputStr}");

                    var fileOutput = parent.AddChild(fileNodeOutputStr);
                    var fileOutputError = fileOutput.AddChild("Errors while parsing :");
                    bool procesedCorrectly = true;

                    var itemsProcessed = 0;
                    foreach (var currentEvent in events)
                    {
                        var visualEvent = new EventHierarchy(currentEvent, masterDb, nameHelper, fileOutput, fileOutputError, files[fileIndex].Name);

                        if (itemsProcessed % 100 == 0 && itemsProcessed != 0)
                            _logger.Here().Information($"\t{itemsProcessed}/{eventsCount} events processsed [{timer.Elapsed.TotalSeconds}s]");

                        itemsProcessed++;
                        procesedCorrectly = visualEvent.ProcesedCorrectly && procesedCorrectly;
                    }

                    if (procesedCorrectly == true)
                        fileOutput.Children.Remove(fileOutputError);

                    if (events.Any())
                        _logger.Here().Information($"\t{itemsProcessed}/{eventsCount} events processsed [{timer.Elapsed.TotalSeconds}s]");
                }
                catch
                { }
            }
        }
     */
}
