﻿using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Serilog;
using Shared.Core.ErrorHandling;
using Shared.Core.Events.Global;
using Shared.Core.Misc;
using Shared.Core.PackFiles.Models;
using Shared.Core.Services;

namespace Shared.Core.PackFiles
{
    public class PackFileService
    {
        public delegate void FileLookUpHander(string fileName, PackFileContainer? container, bool found);
        public event FileLookUpHander? FileLookUpEvent;

        private readonly ILogger _logger = Logging.Create<PackFileService>();
        private readonly GlobalEventSender? _globalEventSender;
        private readonly IAnimationFileDiscovered? _skeletonAnimationLookUpHelper;
        public PackFileDataBase Database { get; private set; }
        public IPackFileUiProvider? UiProvider { get; }

        private readonly ApplicationSettingsService _settingsService;
        private readonly GameInformationFactory _gameInformationFactory;

        public PackFileService(PackFileDataBase database,
            ApplicationSettingsService settingsService,
            GameInformationFactory gameInformationFactory,
            GlobalEventSender? globalEventSender,
            IAnimationFileDiscovered? skeletonAnimationLookUpHelper,
            IPackFileUiProvider? packFileUiProvider)
        {
            _globalEventSender = globalEventSender;
            Database = database;
            _skeletonAnimationLookUpHelper = skeletonAnimationLookUpHelper;
            _settingsService = settingsService;
            _gameInformationFactory = gameInformationFactory;
            UiProvider = packFileUiProvider;
        }

        public bool TriggerFileUpdates { get; set; } = true;

        public PackFileContainer? LoadFolderContainer(string packFileSystemPath)
        {
            if (Directory.Exists(packFileSystemPath) == false)
            {
                var location = Assembly.GetEntryAssembly()!.Location;
                var loactionDir = Path.GetDirectoryName(location);
                throw new Exception($"Unable to find folder {packFileSystemPath}. Curret systempath is {loactionDir}");
            }

            var container = new PackFileContainer(packFileSystemPath);
            AddFolderContentToPackFile(container, packFileSystemPath, packFileSystemPath.ToLower() + "\\");
            Database.AddPackFile(container);
            return container;
        }

        void AddFolderContentToPackFile(PackFileContainer container, string folderPath, string rootPath)
        {
            var files = Directory.GetFiles(folderPath);
            foreach (var filePath in files)
            {
                var sanatizedFilePath = filePath.ToLower();
                var relativePath = sanatizedFilePath.Replace(rootPath, "");
                var fileName = Path.GetFileName(sanatizedFilePath);

                container.FileList[relativePath] = PackFile.CreateFromFileSystem(fileName, sanatizedFilePath);
            }

            var folders = Directory.GetDirectories(folderPath);
            foreach (var folder in folders)
            {
                AddFolderContentToPackFile(container, folder, rootPath);
            }
        }


        public PackFileContainer? Load(string packFileSystemPath, bool setToMainPackIfFirst = false, bool allowLoadWithoutCaPackFiles = false)
        {
            try
            {
                var caPacksLoaded = Database.PackFiles.Count(x => x.IsCaPackFile);
                if (caPacksLoaded == 0 && allowLoadWithoutCaPackFiles != true)
                {
                    MessageBox.Show("You are trying to load a oack file before loading CA packfile. Most editors EXPECT the CA packfiles to be loaded and will cause issues if they are not.\nFile not loaded!", "Error");

                    if (System.Diagnostics.Debugger.IsAttached == false)
                        return null;
                }

                if (!File.Exists(packFileSystemPath))
                {
                    _logger.Here().Error($"Trying to load file {packFileSystemPath}, which can not be located.", "Error");
                    System.Windows.MessageBox.Show($"Unable to locate pack file \"{packFileSystemPath}\"");
                    return null;
                }

                foreach (var packFile in Database.PackFiles)
                {
                    if (packFile.SystemFilePath == packFileSystemPath)
                    {
                        MessageBox.Show($"Pack file \"{packFileSystemPath}\" is already loaded.", "Error");
                        return null;
                    }
                }

                using var fileStream = File.OpenRead(packFileSystemPath);
                using var reader = new BinaryReader(fileStream, Encoding.ASCII);
                var container = Load(reader, packFileSystemPath);
                var notCaPacksLoaded = Database.PackFiles.Count(x => !x.IsCaPackFile);
                if (container.IsCaPackFile == false && setToMainPackIfFirst)
                    SetEditablePack(container);

                _settingsService.AddRecentlyOpenedPackFile(packFileSystemPath);
                _settingsService.Save();
                return container;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to load file {packFileSystemPath}. Error : {e.Message}", "Error");
                _logger.Here().Error($"Failed to load file {packFileSystemPath}. Error : {e}");
                return null;
            }
        }

        public List<string> SearchForFile(string partOfFileName)
        {
            var output = new List<string>();
            foreach (var pf in Database.PackFiles)
            {
                foreach (var file in pf.FileList)
                {
                    if (file.Key.Contains(partOfFileName, StringComparison.InvariantCultureIgnoreCase))
                        output.Add(file.Key);
                }
            }

            return output;
        }

        public List<PackFile> FindAllWithExtention(string extention, PackFileContainer packFileContainer = null)
        {
            return FindAllWithExtentionIncludePaths(extention, packFileContainer).Select(x => x.Item2).ToList();
        }

        public List<PackFile> GetAllAnimPacks()
        {
            var animPacks = FindAllWithExtention(@".animpack");
            var itemsToRemove = animPacks.Where(x => GetFullPath(x).Contains("animation_culture_packs", StringComparison.InvariantCultureIgnoreCase)).ToList();
            foreach (var item in itemsToRemove)
                animPacks.Remove(item);

            return animPacks;
        }

        public List<(string FileName, PackFile Pack)> FindAllWithExtentionIncludePaths(string extention, PackFileContainer packFileContainer = null)
        {
            extention = extention.ToLower();
            var output = new List<ValueTuple<string, PackFile>>();
            if (packFileContainer == null)
            {
                foreach (var pf in Database.PackFiles)
                {
                    foreach (var file in pf.FileList)
                    {
                        var fileExtention = Path.GetExtension(file.Key);
                        if (fileExtention == extention)
                            output.Add(new ValueTuple<string, PackFile>(file.Key, file.Value));
                    }
                }
            }
            else
            {
                foreach (var file in packFileContainer.FileList)
                {
                    var fileExtention = Path.GetExtension(file.Key);
                    if (fileExtention == extention)
                        output.Add(new ValueTuple<string, PackFile>(file.Key, file.Value));
                }
            }

            return output;
        }

        public List<string> DeepSearch(string searchStr, bool caseSensetive)
        {

            _logger.Here().Information($"Searching for : '{searchStr}'");

            var filesWithResult = new List<KeyValuePair<string, string>>();
            var files = Database.PackFiles.SelectMany(x => x.FileList.Select(x => (x.Value.DataSource as PackedFileSource).Parent.FilePath)).Distinct().ToList();

            var indexLock = new object();
            var currentPackFileIndex = 0;

            Parallel.For(0, files.Count,
              index =>
              {
                  var currentIndex = 0;

                  lock (indexLock)
                  {
                      currentIndex = currentPackFileIndex;
                      currentPackFileIndex++;
                  }

                  var packFilePath = files[currentIndex];
                  if (packFilePath.Contains("audio", StringComparison.InvariantCultureIgnoreCase))
                  {
                      _logger.Here().Information($"Skipping audio file {currentIndex}/{files.Count}");
                  }
                  else
                  {
                      using (var fileStram = File.OpenRead(packFilePath))
                      {
                          using (var reader = new BinaryReader(fileStram, Encoding.ASCII))
                          {
                              var pfc = PackFileSerializer.Load(packFilePath, reader, null, false, new CaPackDuplicatePackFileResolver());

                              _logger.Here().Information($"Searching through packfile {currentIndex}/{files.Count} -  {packFilePath} {pfc.FileList.Count} files");

                              foreach (var packFile in pfc.FileList.Values)
                              {
                                  var pf = packFile;
                                  var ds = pf.DataSource as PackedFileSource;
                                  var bytes = ds.ReadDataForFastSearch(fileStram);
                                  var str = Encoding.ASCII.GetString(bytes);

                                  if (str.Contains(searchStr, StringComparison.InvariantCultureIgnoreCase))
                                  {
                                      var fillPathFile = pfc.FileList.FirstOrDefault(x => x.Value == packFile).Key;
                                      _logger.Here().Information($"Found result in '{fillPathFile}' in '{packFilePath}'");

                                      lock (filesWithResult)
                                      {
                                          filesWithResult.Add(new KeyValuePair<string, string>(fillPathFile, packFilePath));
                                      }
                                  }
                              }
                          }
                      }
                  }
              });

            _logger.Here().Information($"[{filesWithResult.Count}] Result for '{searchStr}'_________________:");
            foreach (var item in filesWithResult)
                _logger.Here().Information($"\t\t'{item.Key}' in '{item.Value}'");

            return filesWithResult.Select(x => x.Value).ToList();
        }

        public List<PackFile> FindAllFilesInDirectory(string dir, bool includeSubFolders = true)
        {
            dir = dir.Replace('/', '\\').ToLower();
            var output = new List<PackFile>();

            foreach (var pf in Database.PackFiles)
            {
                foreach (var file in pf.FileList)
                {
                    var includeFile = false;
                    if (includeSubFolders)
                    {
                        includeFile = file.Key.IndexOf(dir) == 0;
                    }
                    else
                    {
                        var dirName = Path.GetDirectoryName(file.Key);
                        var compareResult = string.Compare(dirName, dir, StringComparison.InvariantCultureIgnoreCase);
                        if (compareResult == 0)
                            includeFile = true;
                    }

                    if (includeFile)
                        output.Add(file.Value);
                }
            }


            return output;
        }

        public string GetFullPath(PackFile file, PackFileContainer? container = null)
        {
            if (container == null)
            {
                foreach (var pf in Database.PackFiles)
                {
                    var res = pf.FileList.FirstOrDefault(x => x.Value == file).Key;
                    if (string.IsNullOrWhiteSpace(res) == false)
                        return res;
                }
            }
            else
            {
                var res = container.FileList.FirstOrDefault(x => x.Value == file).Key;
                if (string.IsNullOrWhiteSpace(res) == false)
                    return res;
            }
            throw new Exception("Unknown path for " + file.Name);
        }

        public PackFileContainer Load(BinaryReader binaryReader, string packFileSystemPath)
        {
            var pack = PackFileSerializer.Load(packFileSystemPath, binaryReader, _skeletonAnimationLookUpHelper, _settingsService.CurrentSettings.LoadWemFiles, new CustomPackDuplicatePackFileResolver());
            Database.AddPackFile(pack);
            return pack;
        }

        public bool LoadAllCaFiles(GameTypeEnum gameEnum)
        {
            var game = _gameInformationFactory.GetGameById(gameEnum);
            var path = _settingsService.CurrentSettings.GameDirectories.FirstOrDefault(x => x.Game == game.Type);
            return LoadAllCaFiles(path.Path, game.DisplayName);
        }

        public bool LoadAllCaFiles(string gameDataFolder, string gameName)
        {
            try
            {
                _logger.Here().Information($"Loading pack files for {gameName} located in {gameDataFolder}");
                var allCaPackFiles = GetPackFilesFromManifest(gameDataFolder);

                var packList = new List<PackFileContainer>();
                foreach (var packFilePath in allCaPackFiles)
                {
                    var path = gameDataFolder + "\\" + packFilePath;
                    if (File.Exists(path))
                    {
                        using var fileStram = File.OpenRead(path);
                        using var reader = new BinaryReader(fileStram, Encoding.ASCII);

                        var pack = PackFileSerializer.Load(path, reader, _skeletonAnimationLookUpHelper, _settingsService.CurrentSettings.LoadWemFiles, new CaPackDuplicatePackFileResolver());
                        packList.Add(pack);
                    }
                    else
                    {
                        _logger.Here().Warning($"{gameName} pack file '{path}' not found, loading skipped");
                    }
                }

                var caPackFileContainer = new PackFileContainer($"All Game Packs - {gameName}");
                caPackFileContainer.IsCaPackFile = true;
                caPackFileContainer.SystemFilePath = gameDataFolder;
                var packFilesOrderedByGroup = packList.GroupBy(x => x.Header.LoadOrder).OrderBy(x => x.Key);

                foreach (var group in packFilesOrderedByGroup)
                {
                    var packFilesOrderedByName = group.OrderBy(x => x.Name);
                    foreach (var packfile in packFilesOrderedByName)
                        caPackFileContainer.MergePackFileContainer(packfile);
                }

                Database.AddPackFile(caPackFileContainer);
            }
            catch (Exception e)
            {
                _logger.Here().Error($"Trying to get all CA packs in {gameDataFolder}. Error : {e.ToString()}");
                return false;
            }

            return true;
        }

        List<string> GetPackFilesFromManifest(string gameDataFolder)
        {
            var output = new List<string>();
            var manifestFile = gameDataFolder + "\\manifest.txt";
            if (File.Exists(manifestFile))
            {
                var lines = File.ReadAllLines(manifestFile);
                foreach (var line in lines)
                {
                    var items = line.Split('\t');
                    if (items[0].Contains(".pack"))
                        output.Add(items[0].Trim());
                }
                return output;
            }
            else
            {
                var files = Directory.GetFiles(gameDataFolder)
                    .Where(x => Path.GetExtension(x) == ".pack")
                    .Select(x => Path.GetFileName(x))
                    .ToList();
                return files;
            }
        }

        // Add
        // ---------------------------
        public PackFileContainer CreateNewPackFileContainer(string name, PackFileCAType type, bool setEditablePack = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Name can not be empty");

            var newPackFile = new PackFileContainer(name)
            {
                Header = new PFHeader("PFH5", type),

            };
            Database.AddPackFile(newPackFile);
            if (setEditablePack)
                SetEditablePack(newPackFile);
            return newPackFile;
        }


        public void AddFileToPack(PackFileContainer container, string directoryPath, PackFile newFile)
        {
            if (container.IsCaPackFile)
                throw new Exception("Can not add files to ca pack file");

            if (string.IsNullOrWhiteSpace(newFile.Name))
                throw new Exception("Name can not be empty");

            if (!string.IsNullOrWhiteSpace(directoryPath))
                directoryPath += "\\";
            directoryPath += newFile.Name;
            container.FileList[directoryPath.ToLower()] = newFile;

            if (TriggerFileUpdates)
            {
                _skeletonAnimationLookUpHelper?.UnloadAnimationFromContainer(this, container);
                _skeletonAnimationLookUpHelper?.LoadFromPackFileContainer(this, container);

                Database.TriggerPackFileAdded(container, new List<PackFile>() { newFile });
                _globalEventSender?.TriggerEvent(new PackFileSavedEvent());
            }
        }

        public void AddFilesToPack(PackFileContainer container, List<string> directoryPaths, List<PackFile> newFiles)
        {
            if (container.IsCaPackFile)
                throw new Exception("Can not add files to ca pack file");

            if (directoryPaths.Count != newFiles.Count)
                throw new Exception("Different number of directories and files");

            for (var i = 0; i < directoryPaths.Count; i++)
            {
                var path = directoryPaths[i];
                if (!string.IsNullOrWhiteSpace(path))
                    path += "\\";
                path += newFiles[i].Name;
                container.FileList[path.ToLower()] = newFiles[i];

            }
            _skeletonAnimationLookUpHelper?.UnloadAnimationFromContainer(this, container);
            _skeletonAnimationLookUpHelper?.LoadFromPackFileContainer(this, container);

            Database.TriggerPackFileAdded(container, newFiles);
        }

        public void CopyFileFromOtherPackFile(PackFileContainer source, string path, PackFileContainer target)
        {
            var lowerPath = path.Replace('/', '\\').ToLower().Trim();
            if (source.FileList.ContainsKey(lowerPath))
            {
                var file = source.FileList[lowerPath];
                var newFile = new PackFile(file.Name, file.DataSource);
                target.FileList[lowerPath] = newFile;

                Database.TriggerPackFileAdded(target, new List<PackFile>() { newFile });
            }
        }

        public void AddFolderContent(PackFileContainer container, string path, string folderDir)
        {
            var originalFilePaths = Directory.GetFiles(folderDir, "*", SearchOption.AllDirectories);
            var filePaths = originalFilePaths.Select(x => x.Replace(folderDir + "\\", "")).ToList();
            if (!string.IsNullOrWhiteSpace(path))
                path += "\\";

            var filesAdded = new List<PackFile>();
            for (var i = 0; i < filePaths.Count; i++)
            {
                var currentPath = filePaths[i];
                var filename = Path.GetFileName(currentPath);

                var source = MemorySource.FromFile(originalFilePaths[i]);
                var file = new PackFile(filename, source);
                filesAdded.Add(file);

                container.FileList[path.ToLower() + currentPath.ToLower()] = file;
            }

            _skeletonAnimationLookUpHelper?.UnloadAnimationFromContainer(this, container);
            _skeletonAnimationLookUpHelper?.LoadFromPackFileContainer(this, container);

            Database.TriggerPackFileAdded(container, filesAdded);
        }

        public void SetEditablePack(PackFileContainer pf)
        {
            Database.PackSelectedForEdit = pf;
            Database.TriggerContainerUpdated(pf);
        }

        public PackFileContainer? GetEditablePack()
        {
            return Database.PackSelectedForEdit;
        }

        public bool HasEditablePackFile()
        {
            if (GetEditablePack() == null)
            {
                MessageBox.Show("Unable to complete operation, Editable packfile not set.", "Error");
                return false;
            }
            return true;
        }

        public PackFileContainer? GetPackFileContainer(PackFile file)
        {
            foreach (var pf in Database.PackFiles)
            {
                var res = pf.FileList.FirstOrDefault(x => x.Value == file).Value;
                if (res != null)
                    return pf;
            }
            _logger.Here().Information($"Unknown packfile container for {file.Name}");
            return null;
        }

        public List<PackFileContainer> GetAllPackfileContainers()
        {
            return Database.PackFiles.ToList();
        }

        // Remove
        // ---------------------------
        public void UnloadPackContainer(PackFileContainer pf)
        {
            _skeletonAnimationLookUpHelper?.UnloadAnimationFromContainer(this, pf);
            Database.RemovePackFile(pf);
        }

        public void DeleteFolder(PackFileContainer pf, string folder)
        {
            if (pf.IsCaPackFile)
                throw new Exception("Can not delete folder inside CA pack file");

            var folderLower = folder.ToLower();
            var itemsToDelete = pf.FileList
                .Where(x => string.Equals(Path.GetDirectoryName(x.Key), folder, StringComparison.InvariantCultureIgnoreCase))
                .ToList();

            Database.TriggerPackFileFolderRemoved(pf, folder);

            foreach (var item in itemsToDelete)
            {
                _logger.Here().Information($"Deleting file {item.Key} in directory {folder}");
                pf.FileList.Remove(item.Key);
            }
        }

        public void DeleteFile(PackFileContainer pf, PackFile file)
        {
            if (pf.IsCaPackFile)
                throw new Exception("Can not delete files inside CA pack file");

            var key = pf.FileList.FirstOrDefault(x => x.Value == file).Key;
            _logger.Here().Information($"Deleting file {key}");

            Database.TriggerPackFileRemoved(pf, new List<PackFile>() { file });
            pf.FileList.Remove(key);
        }

        public void MoveFile(PackFileContainer pf, PackFile file, string newFolderPath)
        {
            if (pf.IsCaPackFile)
                throw new Exception("Can not move files inside CA pack file");

            var newFullPath = newFolderPath + "\\" + file.Name;

            var key = pf.FileList.FirstOrDefault(x => x.Value == file).Key;
            pf.FileList.Remove(key);
            pf.FileList[newFullPath] = file;

            _logger.Here().Information($"Moving file {key}");

            _skeletonAnimationLookUpHelper?.UnloadAnimationFromContainer(this, pf);
            _skeletonAnimationLookUpHelper?.LoadFromPackFileContainer(this, pf);
        }

        public void RenameDirectory(PackFileContainer pf, string currentNodeName, string newName)
        {
            if (pf.IsCaPackFile)
                throw new Exception("Can not rename in ca pack file");

            if (string.IsNullOrWhiteSpace(newName))
                throw new Exception("Name can not be empty");

            var oldNodePath = currentNodeName;
            var newNodePath = currentNodeName;

            var files = pf.FileList.Where(x => x.Key.StartsWith(oldNodePath)).ToList();
            foreach (var (path, file) in files)
            {
                pf.FileList.Remove(path);
                var newPath = newNodePath;
                if (oldNodePath.Length != 0)
                    newPath = path.Replace(oldNodePath, newNodePath);

                pf.FileList[newPath] = file;
            }

            _skeletonAnimationLookUpHelper?.UnloadAnimationFromContainer(this, pf);
            _skeletonAnimationLookUpHelper?.LoadFromPackFileContainer(this, pf);

            Database.TriggerPackFileFolderRenamed(pf, newNodePath);
        }

        // Modify
        // ---------------------------
        public void RenameFile(PackFileContainer pf, PackFile file, string newName)
        {
            if (pf.IsCaPackFile)
                throw new Exception("Can not rename file in ca pack file");

            if (string.IsNullOrWhiteSpace(newName))
                throw new Exception("Name can not be empty");

            var key = pf.FileList.FirstOrDefault(x => x.Value == file).Key;
            pf.FileList.Remove(key);

            var dir = Path.GetDirectoryName(key);
            file.Name = newName;
            pf.FileList[dir + "\\" + file.Name] = file;

            Database.TriggerPackFilesUpdated(pf, new List<PackFile>() { file });
        }

        public void SaveFile(PackFile file, byte[] data)
        {
            var pf = GetPackFileContainer(file);

            if (pf.IsCaPackFile)
                throw new Exception("Can not save ca pack file");
            file.DataSource = new MemorySource(data);
            Database.TriggerPackFilesUpdated(pf, new List<PackFile>() { file });
            _globalEventSender?.TriggerEvent(new PackFileSavedEvent());
        }

        public void Save(PackFileContainer pf, string path, bool createBackup)
        {
            if (File.Exists(path) && DirectoryHelper.IsFileLocked(path))
            {
                throw new IOException($"Cannot access {path} because another process has locked it, most likely the game.");
            }

            if (pf.IsCaPackFile)
                throw new Exception("Can not save ca pack file");
            if (createBackup)
                SaveHelper.CreateFileBackup(path);

            // Check if file has changed in size
            if (pf.OriginalLoadByteSize != -1)
            {
                var fileInfo = new FileInfo(pf.SystemFilePath);
                var byteSize = fileInfo.Length;
                if (byteSize != pf.OriginalLoadByteSize)
                    throw new Exception("File has been changed outside of AssetEditor. Can not save the file as it will cause corruptions");
            }

            _skeletonAnimationLookUpHelper?.UnloadAnimationFromContainer(this, pf);

            pf.SystemFilePath = path;
            using (var memoryStream = new FileStream(path + "_temp", FileMode.OpenOrCreate))
            {
                using (var writer = new BinaryWriter(memoryStream))
                    pf.SaveToByteArray(writer);
            }

            File.Delete(path);
            File.Move(path + "_temp", path);

            pf.OriginalLoadByteSize = new FileInfo(path).Length;
            _skeletonAnimationLookUpHelper?.LoadFromPackFileContainer(this, pf);
        }

        public PackFile? FindFile(string path, PackFileContainer? container = null)
        {
            var lowerPath = path.Replace('/', '\\').ToLower().Trim();

            if (container == null)
            {
                for (var i = Database.PackFiles.Count - 1; i >= 0; i--)
                {
                    if (Database.PackFiles[i].FileList.ContainsKey(lowerPath))
                    {
                        FileLookUpEvent?.Invoke(path, Database.PackFiles[i], true);
                        return Database.PackFiles[i].FileList[lowerPath];
                    }
                }
            }
            else
            {
                if (container.FileList.ContainsKey(lowerPath))
                {
                    FileLookUpEvent?.Invoke(path, container, true);
                    return container.FileList[lowerPath];
                }
            }

            FileLookUpEvent?.Invoke(path, null, false);
            return null;
        }
    }
}
