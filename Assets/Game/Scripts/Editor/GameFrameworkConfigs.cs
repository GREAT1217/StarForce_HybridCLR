//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System.IO;
using UnityEngine;
using UnityGameFramework.Editor;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor
{
    public static class GameFrameworkConfigs
    {
        [BuildSettingsConfigPath]
        public static string BuildSettingsConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Game/Configs/Editor/BuildSettings.xml"));

        [ResourceCollectionConfigPath]
        public static string ResourceCollectionConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Game/Configs/Editor/ResourceCollection.xml"));

        [ResourceEditorConfigPath]
        public static string ResourceEditorConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Game/Configs/Editor/ResourceEditor.xml"));

        [ResourceBuilderConfigPath]
        public static string ResourceBuilderConfig = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "Game/Configs/Editor/ResourceBuilder.xml"));
    }
}
