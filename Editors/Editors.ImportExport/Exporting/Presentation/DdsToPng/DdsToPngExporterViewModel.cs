﻿using Editors.ImportExport.Exporting.Exporters;
using Editors.ImportExport.Exporting.Exporters.DdsToPng;
using Editors.ImportExport.Misc;
using Shared.Core.PackFiles.Models;
using Shared.Ui.Common.DataTemplates;

namespace Editors.ImportExport.Exporting.Presentation.DdsToPng
{
    internal class DdsToPngExporterViewModel : IExporterViewModel, IViewProvider
    {
        private readonly DdsToPngExporter _exporter;

        public Type ViewType => typeof(DdsToPngView);
        public string DisplayName => "Dds_to_Png";
        public string OutputExtension => ".png";

        public DdsToPngExporterViewModel(DdsToPngExporter exporter)
        {
            _exporter = exporter;
        }

        public ExportSupportEnum CanExportFile(PackFile file) => _exporter.CanExportFile(file);

        public void Execute(string outputPath, bool generateImporter)
        {
            _exporter.Export(outputPath);
        }
    }
}
