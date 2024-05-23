﻿using Audio.Utility;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Audio.BnkCompiler.ObjectGeneration
{
    public class HircSorter
    {
        public List<IAudioProjectHircItem> Sort(CompilerData project)
        {
            // Sort
            var sortedProjectItems = new List<IAudioProjectHircItem>();

            // HashSet to keep track of added items
            var addedItems = new HashSet<IAudioProjectHircItem>();

            // Add mixers and their children
            var mixers = SortActorMixerList(project);
            foreach (var mixer in mixers)
            {
                var children = mixer.Children.ToList();
                children.Reverse();

                foreach (var childName in children)
                    Console.WriteLine($"########################## {childName} = {WWiseHash.Compute(childName)}");

                // Create a list to store children
                var mixerChildren = new List<IAudioProjectHircItem>();

                // Iterate over the mixer children
                foreach (var childName in children)
                {
                    // Find game sounds with the child name
                    IAudioProjectHircItem gameSound = null;
                    foreach (var sound in project.GameSounds)
                    {
                        if (sound.Name == childName)
                        {
                            gameSound = sound;
                            break;
                        }
                    }

                    if (gameSound != null)
                    {
                        Console.WriteLine($"Collected Game Sound \"{gameSound.Name}\"");
                        mixerChildren.Add(gameSound);
                        continue; // Move to the next child
                    }

                    // Find random containers with the child name
                    foreach (var container in project.RandomContainers)
                    {
                        if (container.Name == childName && !addedItems.Contains(container))
                        {
                            mixerChildren.Add(container);
                            Console.WriteLine($"Collected Random Container \"{container.Name}\" to sortedProjectItems");

                            // Add children of the random container to the list
                            foreach (var containerChildName in container.Children)
                            {
                                var gameSoundChild = project.GameSounds.FirstOrDefault(sound => sound.Name == containerChildName);
                                if (gameSoundChild != null && !addedItems.Contains(gameSoundChild))
                                {
                                    mixerChildren.Add(gameSoundChild);
                                    Console.WriteLine($"Collected Child \"{gameSoundChild.Name}\" from Random Container \"{container.Name}\" to sortedProjectItems");
                                }
                            }
                        }
                    }
                }

                // Add mixer children
                foreach (var child in mixerChildren)
                {
                    if (!addedItems.Contains(child))
                    {
                        sortedProjectItems.Add(child);
                        addedItems.Add(child);
                        Console.WriteLine($"Added {child.GetType().Name} \"{child.Name}\" to sortedProjectItems");
                    }
                }

                // Add current mixer
                if (!addedItems.Contains(mixer))
                {
                    sortedProjectItems.Add(mixer);
                    addedItems.Add(mixer);
                    Console.WriteLine($"Added Mixer \"{mixer.Name}\" to sortedProjectItems");
                }
            }

            // Add Events and actions
            var sortedEvents = project.Events.OrderBy(x => GetSortingId(x)).ToList();
            foreach (var currentEvent in sortedEvents)
            {
                var actions = currentEvent.Actions.Select(x => project.Actions.First(action => action.Name == x)).ToList();
                var sortedActions = actions.OrderBy(x => GetSortingId(x)).ToList();

                sortedProjectItems.AddRange(sortedActions);

                // Add current event
                if (!addedItems.Contains(currentEvent))
                {
                    sortedProjectItems.Add(currentEvent);
                    addedItems.Add(currentEvent);
                    Console.WriteLine($"Added Event \"{currentEvent.Name}\" to sortedProjectItems");
                }
            }

            return sortedProjectItems;
        }

        uint GetSortingId(IAudioProjectHircItem item) => WWiseHash.Compute(item.Name);

        List<ActorMixer> SortActorMixerList(CompilerData project)
        {
            List<ActorMixer> output = new List<ActorMixer>();

            var mixers = project.ActorMixers;//.Shuffle().ToList(); // For testing

            // Find the root
            var roots = mixers.Where(x => HasReferences(x, mixers) == false).ToList();
            //Guard.IsEqualTo(roots.Count(), 1);

            foreach (var mixer in roots)
            {
                var children = mixer.ActorMixerChildren.Select(childId => project.ActorMixers.First(x => x.Name == childId)).ToList();
                output.Add(mixer);
                Console.WriteLine($"Added root mixer: {mixer.Name}");
                    
                ProcessChildren(children, output, project);
            }

            output.Reverse();

            return output;
        }

        void ProcessChildren(List<ActorMixer> children, List<ActorMixer> outputList, CompilerData project)
        {
            var sortedChildren = children.OrderByDescending(x => GetSortingId(x)).ToList();

            outputList.AddRange(sortedChildren);
            foreach (var child in sortedChildren)
            {
                var childOfChildren = child.ActorMixerChildren.Select(childId => project.ActorMixers.First(x => x.Name == childId)).ToList();
                ProcessChildren(childOfChildren, outputList, project);
            }
        }

        bool HasReferences(ActorMixer currentMixer, List<ActorMixer> mixers)
        {
            foreach (var mixer in mixers)
            {
                if (mixer == currentMixer)
                    continue;

                bool isReferenced = mixers
                    .Where(x => x != currentMixer)
                    .Any(x => x.ActorMixerChildren.Contains(currentMixer.Name));
                if (isReferenced)
                    return true;
            }
            return false;
        }
    }
}
