﻿using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Editors.Shared.Core.Common.AnimationPlayer;
using Editors.Shared.Core.Common.ReferenceModel;
using GameWorld.Core.Services;
using Shared.Core.Services;
using Shared.Core.ToolCreation;

namespace Editors.Shared.Core.Common.BaseControl
{
    public abstract partial class EditorHostBase : ObservableObject, IEditorInterface, IEditorViewModelTypeProvider
    {
        private readonly FocusSelectableObjectService _focusSelectableObjectService;
        protected readonly SceneObjectViewModelBuilder _sceneObjectViewModelBuilder;
        protected readonly SceneObjectEditor _sceneObjectEditor;

        protected FocusSelectableObjectService FocusService => _focusSelectableObjectService;
        protected SceneObjectEditor SceneObjectEditor => _sceneObjectEditor;

        public string DisplayName { get; set; }
        public abstract Type EditorViewModelType { get; }

        [ObservableProperty] IWpfGame _gameWorld;
        [ObservableProperty] ObservableCollection<SceneObjectViewModel> _sceneObjects = new();
        [ObservableProperty] AnimationPlayerViewModel _player;

        [ObservableProperty] ICommand _resetCameraCommand;
        [ObservableProperty] ICommand _focusCamerasCommand;

        public EditorHostBase(IEditorHostParameters inputParams)
        {
            GameWorld = inputParams.GameWorld;
            Player = inputParams.AnimationPlayerViewModel;

            _focusSelectableObjectService = inputParams.FocusSelectableObjectService;
            _sceneObjectViewModelBuilder = inputParams.SceneObjectViewModelBuilder;
            _sceneObjectEditor = inputParams.SceneObjectEditor;

            ResetCameraCommand = new RelayCommand(ResetCameraAction);
            FocusCamerasCommand = new RelayCommand(FocusCameraAction);

            inputParams.ComponentInserter.Execute();
        }



        void ResetCameraAction() => _focusSelectableObjectService.ResetCamera();
        void FocusCameraAction() => _focusSelectableObjectService.FocusSelection();

        public virtual void Close()
        {
            GameWorld = null;
        }
    }
}
