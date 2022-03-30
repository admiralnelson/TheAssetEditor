﻿using CommonControls.Common;
using CommonControls.FileTypes.PackFiles.Models;
using CommonControls.FileTypes.RigidModel.Types;
using CommonControls.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace View3D.Utility
{
    public class TextureConverter
    {
        static readonly ILogger _logger = Logging.CreateStatic(typeof(TextureConverter));

        static string GetTextureConverterPath()
        {
            var texconvPath = $"{DirectoryHelper.Temp}\\texconv.exe";

            if (!File.Exists(texconvPath))
            {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("View3D.Content.Game.texconv.exe");
                using var fStream = new FileStream(texconvPath, FileMode.OpenOrCreate);
                stream!.CopyTo(fStream);
            }

            return texconvPath;
        }

        public static bool SaveAsPNG(PackFile pfs, string outputFileName)
        {
            try
            {
                var tempTextureDir = $"{DirectoryHelper.Temp}\\temp_textures\\";
                DirectoryHelper.EnsureCreated(tempTextureDir);

                var bytes = pfs.DataSource.ReadData();
                var tempFilePath = tempTextureDir + Guid.NewGuid() + ".dds";
                File.WriteAllBytes(tempFilePath, bytes);

                var pngPath = SaveDDSTextureAsPNG(tempFilePath);
                File.Move(pngPath, outputFileName);
                File.Delete(tempFilePath);
            }
            catch (Exception e)
            {
                _logger.Here().Error($"Eror converting texture {e.Message}");
                return false;
            }

            var newFileFound = File.Exists(outputFileName);
            if (newFileFound == false)
                _logger.Here().Error($"Failed to create texture as PNG for unkown reason");

            return newFileFound;
        }

        public static string SaveDDSTextureAsPNG(string fileToConvert)
        {
            var texconvPath = GetTextureConverterPath();

            using var pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = texconvPath;
            pProcess.StartInfo.Arguments =$"-ft png -f R8G8B8A8_UNORM -y -o \"{Path.GetDirectoryName(fileToConvert)}\" \"{fileToConvert}\"";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            var output = pProcess.StandardOutput.ReadToEnd();
            _logger.Here().Information(output);

            return Path.ChangeExtension(fileToConvert, ".png");
        }

        public static PackFile LoadTexture(PackFileService pfs, string packFilePath, string systemFilePath, TexureType texureType)
        {
            var ddsPath = SaveTextureAsDDS(systemFilePath, texureType);
            var data = File.ReadAllBytes(ddsPath);
            var correctPath = Path.ChangeExtension(packFilePath, "dds");
            var result = SaveHelper.Save(pfs, correctPath, null, data, false);
            File.Delete(ddsPath);
            return result;
        }

        public static string SaveTextureAsDDS(string systemFilePath, TexureType texureType)
        {
            var texconvArguments = texureType switch
            {
                TexureType.BaseColour => "-f BC1_UNORM_SRGB",
                TexureType.MaterialMap => "-f BC1_UNORM_SRGB",
                TexureType.Normal => "-f BC3_UNORM",
                TexureType.Mask => "-f BC3_UNORM",
                _ => throw new Exception("Unkown texture type"),
            };

            if (File.Exists(systemFilePath) == false)
                throw new Exception($"Unable to find file {systemFilePath}");

            var texconvPath = GetTextureConverterPath();
            using var pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = texconvPath;
            pProcess.StartInfo.Arguments = $"{texconvArguments} -y -o \"{Path.GetDirectoryName(systemFilePath)}\" \"{systemFilePath}\"";
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            var output = pProcess.StandardOutput.ReadToEnd();
            _logger.Here().Information(output);
            pProcess.WaitForExit();

            return Path.ChangeExtension(systemFilePath, ".dds");
        }
    }
}
