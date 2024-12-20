﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using static Editors.Audio.AudioEditor.AudioEditorSettings;

namespace Editors.Audio.AudioEditor
{
    public class SettingsEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return value;

            if (value is EventType eventType)
                return EventTypeMappings.TryGetValue(eventType, out var displayString) ? displayString : eventType.ToString();

            if (value is EventSubtype eventSubtype)
                return EventSubtypeMappings.TryGetValue(eventSubtype, out var displayString) ? displayString : eventSubtype.ToString();

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
