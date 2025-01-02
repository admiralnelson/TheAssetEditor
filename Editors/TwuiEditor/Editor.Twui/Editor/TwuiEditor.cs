﻿using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Editors.Twui.Editor.PreviewRendering;
using Shared.Core.Events;
using Shared.Core.PackFiles.Models;
using Shared.Core.ToolCreation;
using Editors.Twui.Editor.ComponentEditor;
using Shared.GameFormats.Twui;
using Shared.GameFormats.Twui.Data;
using Editors.Twui.Editor.Events;

namespace Editors.Twui.Editor
{
    public partial class TwuiEditor : ObservableObject, IEditorInterface, ISaveableEditor, IFileEditor
    {
        private readonly IEventHub _eventHub;

        [ObservableProperty] string _displayName = "Twui Editor";

        public bool HasUnsavedChanges { get; set; } = false;
        public PackFile CurrentFile { get; set; }

        [ObservableProperty] TwuiFile _parsedTwuiFile;
   
        [ObservableProperty] ComponentManger _componentManager;
        [ObservableProperty] EditorRenderHandler _previewRenderer;

        public TwuiEditor(IEventHub eventHub, ComponentManger componentEditor, EditorRenderHandler previewRenderer)
        {
            _eventHub = eventHub;
            _componentManager = componentEditor;
            _previewRenderer = previewRenderer;
        }

        public bool Save() { return true; } 
        public void Close() { }

        public void LoadFile(PackFile file)
        {
            if (file == CurrentFile)
                return;

            var serializer = new TwuiSerializer();
            ParsedTwuiFile = serializer.Load(file);
            DisplayName = "Twui Editor:" + Path.GetFileName(file.Name);

            ComponentManager.SetFile(ParsedTwuiFile);
            _eventHub.Publish(new RedrawTwuiEvent(ParsedTwuiFile, null));
        }
    }
}