﻿using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using Shared.Core.ErrorHandling.Exceptions;

namespace Shared.Ui.Common.Exceptions
{
    public partial class CustomExceptionWindow : Window
    {
        private readonly ExceptionInformation _extendedExceptionInformation;

        public CustomExceptionWindow(ExceptionInformation extendedExceptionInformation)
        {
            InitializeComponent();
            _extendedExceptionInformation = extendedExceptionInformation;

            var allMessages = extendedExceptionInformation.ExceptionInfo.Select(x => x.Message).ToList();

            ErrorTextHandle.Text = string.Empty; 
            if (string.IsNullOrWhiteSpace(extendedExceptionInformation.UserMessage) == false)
            {
                ErrorTextHandle.Text += "Information:\n";
                ErrorTextHandle.Text += extendedExceptionInformation.UserMessage + "\n\n";
            }

            ErrorTextHandle.Text += string.Join("\n", allMessages);

            var lastStackFrame = extendedExceptionInformation.ExceptionInfo.LastOrDefault();
            if (lastStackFrame != null && lastStackFrame.StackTrace.Length != 0)
            {
                ErrorTextHandle.Text += "\n\nStackTrace:\n";
                ErrorTextHandle.Text += string.Join("\n", lastStackFrame.StackTrace);
            }

            var editorName = "";
            if (string.IsNullOrWhiteSpace(extendedExceptionInformation.CurrentEditorName) == false)
            {
                editorName = extendedExceptionInformation.CurrentEditorName + " : ";
                if (editorName.Contains("ViewModel", StringComparison.InvariantCultureIgnoreCase))
                    editorName = editorName.Replace("ViewModel", "", StringComparison.InvariantCultureIgnoreCase);
            }
            Title = $"{editorName}Error - v{extendedExceptionInformation.AssetEditorVersion} {extendedExceptionInformation.CurrentGame}";

            var extraInfo = new StringBuilder();
            extraInfo.AppendLine("Packed Files:");
            foreach (var item in extendedExceptionInformation.ActivePackFiles)
                extraInfo.AppendLine($"\t'{item.Name}' @ '{item.SystemPath}' IsCa:{item.IsCa} IsMain:{item.IsMainEditable}");

            extraInfo.AppendLine($"Runtime: {extendedExceptionInformation.RunTimeInSeconds}");
            extraInfo.AppendLine($"OSVersion: {extendedExceptionInformation.OSVersion}");
            extraInfo.AppendLine($"Culture: {extendedExceptionInformation.Culture}");
            extraInfo.AppendLine($"Open editors: {extendedExceptionInformation.NumberOfOpenEditors}");
            extraInfo.AppendLine($"Total Created editors: {extendedExceptionInformation.NumberOfOpenedEditors}");

            ExtraInfoHandle.Text = extraInfo.ToString();
        }

        private void CopyButtonPressed(object sender, RoutedEventArgs e)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
            var text = JsonSerializer.Serialize(_extendedExceptionInformation, options);
            Clipboard.SetText(text);
            MessageBox.Show("Error message copied to clipboard!");
        }

        private void CloseButtonPressed(object sender, RoutedEventArgs e) => Close();
    }
}
