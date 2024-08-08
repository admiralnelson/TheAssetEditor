﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Editors.Audio.Storage;
using Shared.Core.Misc;
using Shared.Core.PackFiles;
using Shared.Core.PackFiles.Models;
using Shared.Core.ToolCreation;
using static Editors.Audio.Presentation.AudioEditor.AudioEditorViewModelHelpers;

namespace Editors.Audio.Presentation.AudioEditor.ViewModels
{
    public partial class AudioEditorSettingsViewModel : ObservableObject, IEditorViewModel
    {
        private readonly IAudioRepository _audioRepository;
        private readonly PackFileService _packFileService;
        private readonly AudioEditorViewModel _audioEditorViewModel;

        //readonly ILogger _logger = Logging.Create<AudioEditorSettingsViewModel>();

        public NotifyAttr<string> DisplayName { get; set; } = new NotifyAttr<string>("Audio Editor");

        [ObservableProperty] private string _audioProjectFileName = "my_audio_project";
        [ObservableProperty] private string _customStatesFileName = "my_custom_states";

        [ObservableProperty] private string _selectedAudioProjectEventType;
        [ObservableProperty] private string _selectedAudioProjectEventSubtype;
        [ObservableProperty] private ObservableCollection<CheckBox> _dialogueEventCheckBoxes = [];

        [ObservableProperty] private ObservableCollection<AudioEditorSettings.EventType> _audioProjectEventTypes = new (Enum.GetValues(typeof(AudioEditorSettings.EventType)).Cast<AudioEditorSettings.EventType>());
        [ObservableProperty] private ObservableCollection<AudioEditorSettings.EventSubtype> _audioProjectSubtypes = []; // Determined according to what Event Type is selected

        public static List<string> AudioProjectDialogueEvents => AudioEditorData.Instance.AudioProjectDialogueEvents;

        public AudioEditorSettingsViewModel(IAudioRepository audioRepository, PackFileService packFileService, AudioEditorViewModel audioEditorViewModel)
        {
            _audioRepository = audioRepository;
            _packFileService = packFileService;
            _audioEditorViewModel = audioEditorViewModel;
        }

        partial void OnSelectedAudioProjectEventTypeChanged(string value)
        {
            DialogueEventCheckBoxes.Clear();

            // Update the ComboBox for EventSubType upon EventType selection.
            UpdateAudioProjectEventSubType();
        }

        partial void OnSelectedAudioProjectEventSubtypeChanged(string value)
        {
            DialogueEventCheckBoxes.Clear();

            // Update the ListBox with the appropriate Dialogue Events.
            CreateAudioProjectEventsList();
        }

        public void UpdateAudioProjectEventSubType()
        {
            AudioProjectSubtypes.Clear();

            if (Enum.TryParse(SelectedAudioProjectEventType.ToString(), out AudioEditorSettings.EventType eventType))
            {
                if (AudioEditorSettings.EventTypeToSubtypes.TryGetValue(eventType, out var subtypes))
                {
                    foreach (var subtype in subtypes)
                        AudioProjectSubtypes.Add(subtype);
                }
            }
        }

        public void CreateAudioProjectEventsList()
        {
            AudioProjectDialogueEvents.Clear();

            if (Enum.TryParse(SelectedAudioProjectEventType, out AudioEditorSettings.EventType eventType))
            {
                if (Enum.TryParse(SelectedAudioProjectEventSubtype, out AudioEditorSettings.EventSubtype eventSubtype))
                {
                    var dialogueEvents = AudioEditorSettings.DialogueEvents
                        .Where(de => de.Type == eventType)
                        .ToList();

                    PopulateDialogueEventsListBox(dialogueEvents, eventSubtype);
                    
                    CreateAudioProjectEventsList(dialogueEvents, eventSubtype);
                }
            }
        }

        public void PopulateDialogueEventsListBox(List<(string EventName, AudioEditorSettings.EventType Type, AudioEditorSettings.EventSubtype[] Subtype, bool Recommended)> dialogueEvents, AudioEditorSettings.EventSubtype eventSubtype)
        {
            DialogueEventCheckBoxes.Clear();

            foreach (var dialogueEvent in dialogueEvents)
            {
                if (dialogueEvent.Subtype.Contains(eventSubtype))
                {
                    var checkBox = new CheckBox
                    {
                        Content = AddExtraUnderScoresToString(dialogueEvent.EventName),
                        IsChecked = false
                    };

                    DialogueEventCheckBoxes.Add(checkBox);
                }
            }
        }

        public void CreateAudioProjectEventsList(List<(string EventName, AudioEditorSettings.EventType Type, AudioEditorSettings.EventSubtype[] Subtype, bool Recommended)> dialogueEvents, AudioEditorSettings.EventSubtype eventSubtype)
        {
            foreach (var dialogueEvent in dialogueEvents)
            {
                if (dialogueEvent.Subtype.Contains(eventSubtype))
                    AudioProjectDialogueEvents.Add(dialogueEvent.EventName);
            }
        }

        [RelayCommand] public void CreateAudioProject()
        {
            // Remove any pre-existing data.
            AudioEditorData.Instance.EventsData.Clear();
            _audioEditorViewModel.AudioEditorDataGridItems.Clear();
            _audioEditorViewModel.SelectedAudioProjectEvent = "";

            // Create the object for State Groups with qualifiers so that their keys in the EventsData dictionary are unique.
            AddQualifiersToStateGroups(_audioRepository.DialogueEventsWithStateGroups);

            // Initialise EventsData according to the Audio Project settings selected.
            InitialiseEventsData(this);

            // Add the Audio Project with empty events to the PackFile.
            AudioProjectData.AddAudioProjectToPackFile(_packFileService, AudioEditorData.Instance.EventsData, AudioProjectFileName);

            // Load the custom States so that they can be referenced when the Event is loaded.
            //PrepareCustomStatesForComboBox(this);
        }

        [RelayCommand] public void SelectAll()
        {
            foreach (var checkBox in DialogueEventCheckBoxes)
                checkBox.IsChecked = true;
        }

        /*
        [RelayCommand] public void SelectRecommended()
        {
            // Get the list of dialogue events with the "Recommended" category
            var recommendedEvents = AudioEditorSettings.FrontendVODialogueEvents.Where(e => e.Categories.Contains("Recommended")).Select(e => e.EventName).ToHashSet();

            // Iterate through the CheckBoxes and set IsChecked for those with matching EventName
            foreach (var checkBox in DialogueEventCheckBoxes)
            {
                if (recommendedEvents.Contains(checkBox.Content.ToString()))
                {
                    checkBox.IsChecked = true;
                }
            }
        }
        */

        [RelayCommand]public void SelectNone()
        {
            foreach (var checkBox in DialogueEventCheckBoxes)
            {
                checkBox.IsChecked = false;
            }
        }

        public void Close()
        {
        }

        public bool Save() => true;

        public PackFile MainFile { get; set; }

        public bool HasUnsavedChanges { get; set; } = false;
    }
}
