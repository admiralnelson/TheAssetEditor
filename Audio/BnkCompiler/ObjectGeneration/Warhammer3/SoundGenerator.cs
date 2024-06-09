﻿using CommunityToolkit.Diagnostics;
using Shared.Core.PackFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Audio.BnkCompiler.ObjectConfiguration.Warhammer3;
using Shared.GameFormats.WWise;
using Shared.GameFormats.WWise.Hirc.V136;

namespace Audio.BnkCompiler.ObjectGeneration.Warhammer3
{
    public class SoundGenerator : IWWiseHircGenerator
    {
        public string GameName => CompilerConstants.GameWarhammer3;
        public Type AudioProjectType => typeof(GameSound);

        private readonly PackFileService _pfs;

        public SoundGenerator(PackFileService pfs)
        {
            _pfs = pfs;
        }

        public HircItem ConvertToWWise(IAudioProjectHircItem projectItem, CompilerData project)
        {
            var typedProjectItem = projectItem as GameSound;
            Guard.IsNotNull(typedProjectItem);
            return ConvertToWWise(typedProjectItem, project);
        }

        public CAkSound_v136 ConvertToWWise(GameSound inputSound, CompilerData project)
        {
            var file = _pfs.FindFile(inputSound.Path);
            var nodeBaseParams = NodeBaseParams.CreateDefault();
            var wavFile = Path.GetFileName(inputSound.Path);
            var wavFileName = wavFile.Replace(".wem", "");

            var wwiseSound = new CAkSound_v136()
            {
                Id = inputSound.Id,
                Type = HircType.Sound,
                AkBankSourceData = new AkBankSourceData()
                {
                    PluginId = 0x00040001, // [VORBIS]
                    StreamType = SourceType.Streaming,
                    akMediaInformation = new AkMediaInformation()
                    {
                        SourceId = uint.Parse(wavFileName),
                        uInMemoryMediaSize = (uint)file.DataSource.Size,
                        uSourceBits = 0x01,
                    }
                },
                NodeBaseParams = nodeBaseParams
            };

            wwiseSound.NodeBaseParams.DirectParentId = inputSound.DirectParentId;

            // Applying attenuation directly to sounds is necessary as they don't appear to use the vanilla mixer's attenuation even though they're being routed through it.
            var attenuationId = inputSound.Attenuation;

            if (attenuationId != 0)
            {
                wwiseSound.NodeBaseParams.NodeInitialParams.AkPropBundle0 = new AkPropBundle()
                {
                    Values = new List<AkPropBundle.AkPropBundleInstance>()
                    {
                        new(){Type = AkPropBundleType.Attenuation, Value = attenuationId}
                    }
                };
            }

            wwiseSound.UpdateSize();

            return wwiseSound;
        }

        public List<CAkSound_v136> ConvertToWWise(IEnumerable<GameSound> inputSound, CompilerData project)
        {
            return inputSound.Select(x => ConvertToWWise(x, project)).ToList();
        }
    }
}
