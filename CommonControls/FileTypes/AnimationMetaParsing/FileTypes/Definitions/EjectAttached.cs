﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using AnimationMeta.FileTypes.Parsing;
using Microsoft.Xna.Framework;

namespace AnimationMeta.FileTypes.Definitions
{

    [MetaData("EJECT_ATTACHED", 10)]
    public class EjectAttached_v10 : DecodedMetaEntryBase
    {
        [MetaDataTag(5)]
        public Vector3 Direction { get; set; }
    }
}
