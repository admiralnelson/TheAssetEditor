﻿using System.Xml.Linq;

namespace Shared.GameFormats.Twui.Data.DataTypes
{
    public enum DockingHorizontal
    {
        None,
        Center,
        Left,
        Right
    }

    public enum DockingVertical
    {
        None,
        Center,
        Top,
        Bottom

    }

    public static class DockingParser
    {
        public static void ConvertFrom(XElement componentXml, out DockingHorizontal horizontal, out DockingVertical vertical)
        {
            // Set default values
            horizontal = DockingHorizontal.None;
            vertical = DockingVertical.None;

            // Get the data
            var dockingNode = componentXml.Attribute("docking")?.Value;       // twui v141
            if (dockingNode == null)
                dockingNode = componentXml.Attribute("dock_point")?.Value;    // twui v142

            if (dockingNode == null)
                return;

            var entries = dockingNode.Split(" ");

            vertical = entries[0].ToLower() switch
            {
                "center" => DockingVertical.Center,
                "top" => DockingVertical.Top,
                "bottom" => DockingVertical.Center,
                _ => throw new Exception($"Unknown {nameof(DockingVertical)} - {entries[0]}"),
            };

            if (entries.Length == 1)
                return;

            horizontal = entries[1].ToLower() switch
            {
                "center" => DockingHorizontal.Center,
                "left" => DockingHorizontal.Left,
                "right" => DockingHorizontal.Right,
                _ => throw new Exception($"Unknown {nameof(DockingHorizontal)} - {entries[1]}"),
            };
        }
    }
}