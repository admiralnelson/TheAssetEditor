﻿using Audio.Utility;
using CommonControls.Common;
using CommonControls.Services;
using CommunityToolkit.Diagnostics;
using System.IO;

namespace Audio.BnkCompiler
{
    public class ResultHandler
    {
        private readonly PackFileService _pfs;
        public string WWiserPath { get; set; } = "D:\\Research\\Audio\\WWiser\\wwiser.pyz";

        public ResultHandler(PackFileService pfs)
        {
            _pfs = pfs;
        }

        internal Result<bool> ProcessResult(CompileResult compileResult, CompilerData compilerData, CompilerSettings settings)
        {
            SaveToPackFile(compileResult, compilerData, settings);
            ExportToDirectory(compileResult, settings.FileExportPath, settings.ConvertResultToXml);
            return Result<bool>.FromOk(true);
        }

        void SaveToPackFile(CompileResult compileResult, CompilerData compilerData, CompilerSettings settings)
        {
            var ouputPath = "wwise\\audio";
            if (string.IsNullOrWhiteSpace(compilerData.ProjectSettings.Language) == false)
                ouputPath += $"\\{compilerData.ProjectSettings.Language}";

            SaveHelper.SavePackFile(_pfs,ouputPath, compileResult.OutputBnkFile, false);
            SaveHelper.SavePackFile(_pfs, ouputPath, compileResult.OutputDatFile, false);
        }

        void ExportToDirectory(CompileResult result, string outputDirectory, bool convertResultToXml)
        {
            if (string.IsNullOrWhiteSpace(outputDirectory) == false)
            {
                var bnkPath = Path.Combine(outputDirectory, $"{result.Project.ProjectSettings.BnkName}.bnk");
                File.WriteAllBytes(bnkPath, result.OutputBnkFile.DataSource.ReadData());

                var datPath = Path.Combine(outputDirectory, $"{result.Project.ProjectSettings.BnkName}.dat");
                File.WriteAllBytes(datPath, result.OutputDatFile.DataSource.ReadData());

                if (convertResultToXml)
                {
                    Guard.IsNotNullOrEmpty(WWiserPath);
                    BnkToXmlConverter.Convert(WWiserPath, bnkPath, true);
                }
            }
        }
    }

}