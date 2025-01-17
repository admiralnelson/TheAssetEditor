﻿using System;
using Shared.GameFormats.Wwise.Hirc;

namespace Editors.Audio.BnkCompiler.ObjectGeneration
{
    public interface IWwiseHircGenerator
    {
        public string GameName { get; }
        public Type AudioProjectType { get; }
        public HircItem ConvertToWwise(IAudioProjectHircItem projectItem, CompilerData project);
    }
}
