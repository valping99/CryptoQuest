﻿using System;
using UnityEditor;

namespace IndiGamesEditor.UnityBuilderAction.Versioning
{
    public static class VersionApplicator
    {
        public static void SetVersion(string version)
        {
            Console.WriteLine($"Setting version to {version}");
            if (version == "none")
            {
                return;
            }

            Apply(version);
        }

        public static void SetAndroidVersionCode(string androidVersionCode)
        {
            int bundleVersionCode = Int32.Parse(androidVersionCode);
            if (bundleVersionCode <= 0)
            {
                return;
            }

            PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
        }

        static void Apply(string version)
        {
            PlayerSettings.bundleVersion = version;
            PlayerSettings.macOS.buildNumber = version;
        }
    }
}