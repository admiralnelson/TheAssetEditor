﻿using Microsoft.Xna.Framework;

namespace Shared.Core.Settings
{
    public enum BackgroundColour
    {
        DarkGrey,
        LegacyBlue,
        Green,
    }
    public class RenderEngineBackgroundColourController
    {
        public static string GetEnumAsString(BackgroundColour colour)
        {
            return colour switch
            {
                BackgroundColour.DarkGrey => "Dark Grey",
                BackgroundColour.LegacyBlue => "Legacy Blue",
                BackgroundColour.Green => "Green",
            };
        }
        public static Color GetEnumAsColour(BackgroundColour colour)
        {
            return colour switch
            {
                BackgroundColour.DarkGrey => new Color(50, 50, 50),
                BackgroundColour.LegacyBlue => new Color(94, 150, 239),
                BackgroundColour.Green => new Color(0, 177, 64),
            };
        }
    }
}
