﻿using Audio.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Audio.BnkCompiler.ObjectConfiguration.Warhammer3;
using Shared.GameFormats.WWise.Hirc.Shared;

namespace Audio.BnkCompiler
{
    public abstract class IAudioProjectHircItem
    {
        public string Name { get; set; }
        public uint Id { get; set; }
        public uint SerializationId { get; set; }
    }

    public class Event : IAudioProjectHircItem
    {
        public List<string> Actions { get; set; }
    }

    public class DialogueEvent : IAudioProjectHircItem
    {
        public AkDecisionTree.Node RootNode { get; set; }
        public uint NodesCount { get; set; } = 0;
    }

    public class RandomContainer : IAudioProjectHircItem
    {
        public List<string> Children { get; set; }
        public string DirectParentId { get; set; } = null;
    }

    public class Action : IAudioProjectHircItem
    {
        public string ChildId { get; set; }
        public string Type { get; set; }
    }

    public class GameSound : IAudioProjectHircItem
    {
        public string Path { get; set; }
        public string DirectParentId { get; set; } = null;
        public string DialogueEvent { get; set; }
        public uint Attenuation { get; set; }

    }

    public class ActorMixer : IAudioProjectHircItem
    {
        public uint DirectParentId { get; set; } = 0;
        public List<string> Children { get; set; } = new List<string>();
        public List<string> ActorMixerChildren { get; set; } = new List<string>();
        public string DialogueEvent { get; set; }
    }

    public class ProjectSettings
    {
        public int Version { get; set; } = 1;
        public string OutputGame { get; set; } = CompilerConstants.Game_Warhammer3;
        public string BnkName { get; set; }
        public string Language { get; internal set; }
    }

    public class CompilerData
    {
        private List<IAudioProjectHircItem> _projectWwiseObjects = new List<IAudioProjectHircItem>();

        public ProjectSettings ProjectSettings { get; set; } = new ProjectSettings();
        public List<Event> Events { get; set; } = new List<Event>();
        public List<Action> Actions { get; set; } = new List<Action>();
        public List<GameSound> GameSounds { get; set; } = new List<GameSound>();
        public List<ActorMixer> ActorMixers { get; set; } = new List<ActorMixer>();
        public List<RandomContainer> RandomContainers { get; set; } = new List<RandomContainer>();
        public List<DialogueEvent> DialogueEvents { get; set; } = new List<DialogueEvent>();
        public List<string> DatStates { get; set; } = new List<string>();

        public void StoreWwiseObjects()
        {
            _projectWwiseObjects.AddRange(Events);
            _projectWwiseObjects.AddRange(Actions);
            _projectWwiseObjects.AddRange(GameSounds);
            _projectWwiseObjects.AddRange(ActorMixers);
            _projectWwiseObjects.AddRange(RandomContainers);
            _projectWwiseObjects.AddRange(DialogueEvents);
        }

        public void ProcessHircIds()
        {
            // Handle objects with a Name or Override Id and convert it into one common hirc Id property.
        }

        public uint GetHircItemIdFromName(string name)
        {
            return _projectWwiseObjects.First(x => x.Name == name).SerializationId;
        }

        public IAudioProjectHircItem GetActorMixerForObject(string objectName)
        {
            var mixers = _projectWwiseObjects.Where(x => x is ActorMixer).Cast<ActorMixer>().ToList();
            var mixer = mixers.Where(x => x.Children.Contains(objectName)).ToList();
            return mixer.FirstOrDefault();
        }
    }
}
