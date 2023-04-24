﻿using System;
using System.IO;

namespace CommonControls.Common
{
    public class DirectoryHelper
    {
        public static string UserDirectory { get { return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile); } }
        public static string ApplicationDirectory { get { return UserDirectory + "\\AssetEditor"; } }
        public static string SchemaDirectory { get { return ApplicationDirectory + "\\Schemas"; } }
        public static string LogDirectory { get { return ApplicationDirectory + "\\Logs"; } }
        public static string ReportsDirectory { get { return ApplicationDirectory + "\\Reports"; } }
        public static string Applications { get { return ApplicationDirectory + "\\Applications"; } }
        public static string Temp { get { return ApplicationDirectory + "\\Temp"; } }
        public static string AnimationIndexMappingDirectory { get { return ApplicationDirectory + "\\Animation\\BoneIndexMapping"; } }

        

        public static void EnsureCreated()
        {
            EnsureCreated(ApplicationDirectory);
            EnsureCreated(SchemaDirectory);
            EnsureCreated(LogDirectory);
            EnsureCreated(ReportsDirectory);
            EnsureCreated(Applications);
            EnsureCreated(Temp);
            EnsureCreated(AnimationIndexMappingDirectory);
        }

        public static void EnsureCreated(string path)
        {
            var directoryPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        public static bool IsFileLocked(string path)
        {
            try
            {
                using (Stream stream = new FileStream(path, FileMode.Open))
                {
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        public static void OpenFolderAndSelectFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            // combine the arguments together
            // it doesn't matter if there is a space after ','
            string argument = "/select, \"" + filePath + "\"";

            System.Diagnostics.Process.Start("explorer.exe", argument);
        }
    }
}
