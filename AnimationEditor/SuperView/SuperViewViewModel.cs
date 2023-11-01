﻿using System;
using AnimationEditor.Common.ReferenceModel;
using AnimationEditor.PropCreator.ViewModels;
using AnimationMeta.Presentation;
using CommonControls.Common;
using CommonControls.FileTypes.PackFiles.Models;
using CommonControls.Services;
using Microsoft.Xna.Framework;
using View3D.Animation;

namespace AnimationEditor.SuperView
{
    public class SuperViewViewModel : IHostedEditor<SuperViewViewModel>
    {
        SceneObject _asset;
        AnimationToolInput _debugDataToLoad;

        private readonly SceneObjectBuilder _sceneObjectBuilder;
        private readonly SceneObjectViewModelBuilder _sceneObjectViewModelBuilder;
        private readonly PackFileService _packFileService;

        public NotifyAttr<string> PersistentMetaFilePath { get; set; } = new NotifyAttr<string>("");
        public NotifyAttr<string> MetaFilePath { get; set; } = new NotifyAttr<string>("");

        public EditorViewModel PersistentMetaEditor { get; private set; }
        public EditorViewModel MetaEditor { get; private set; }

        public string EditorName { get; } = "Super View";

        public SuperViewViewModel(SceneObjectViewModelBuilder sceneObjectViewModelBuilder,
            PackFileService packFileService, 
            SceneObjectBuilder sceneObjectBuilder, 
            CopyPasteManager copyPasteManager)
        {
            _sceneObjectViewModelBuilder = sceneObjectViewModelBuilder;
            _packFileService = packFileService;
            _sceneObjectBuilder = sceneObjectBuilder;

            PersistentMetaEditor = new EditorViewModel(_packFileService, copyPasteManager);
            PersistentMetaEditor.EditorSavedEvent += PersistentMetaEditor_EditorSavedEvent;

            MetaEditor = new EditorViewModel(_packFileService, copyPasteManager);
            MetaEditor.EditorSavedEvent += MetaEditor_EditorSavedEvent;
        }

        public void SetDebugInputParameters(AnimationToolInput debugDataToLoad)
        {
            _debugDataToLoad = debugDataToLoad;
        }

        public void Initialize(EditorHost<SuperViewViewModel> editorOwner)
        {
            var assetViewModel = _sceneObjectViewModelBuilder.CreateAsset(true, "Root", Color.Black, _debugDataToLoad, true);
            editorOwner.SceneObjects.Add(assetViewModel);

            _asset = assetViewModel.Data;
            _asset.MetaDataChanged += UpdateMetaDataInfoFromAsset;
            _asset.AnimationChanged += AnimationChanged;
            UpdateMetaDataInfoFromAsset(_asset);
        }

        private void AnimationChanged(AnimationClip newValue)
        {
            Console.WriteLine("test");
        }

        private void UpdateMetaDataInfoFromAsset(SceneObject asset)
        {
            PersistentMetaEditor.MainFile = asset.PersistMetaData;
            PersistentMetaFilePath.Value = BuildMetaDataName(asset.PersistMetaData);

            MetaEditor.MainFile = asset.MetaData;
            MetaFilePath.Value = BuildMetaDataName(asset.MetaData);
        }

        string BuildMetaDataName(PackFile file)
        {
            if (file == null)
                return "";

            var containerName = _packFileService.GetPackFileContainer(PersistentMetaEditor.MainFile).Name;
            var filePath = PersistentMetaFilePath.Value = _packFileService.GetFullPath(PersistentMetaEditor.MainFile);
            return $"[{containerName}]{filePath}";
        }

        private void MetaEditor_EditorSavedEvent(PackFile newFile)
        {
            _sceneObjectBuilder.SetMetaFile(_asset, newFile, _asset.PersistMetaData);
        }

        private void PersistentMetaEditor_EditorSavedEvent(PackFile newFile)
        {
            _sceneObjectBuilder.SetMetaFile(_asset, _asset.MetaData, newFile);
        }

        public void RefreshAction() => _asset.TriggerMeshChanged();
    }
}
