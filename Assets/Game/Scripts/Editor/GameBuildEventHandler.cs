﻿//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor
{
    public sealed class GameBuildEventHandler : IBuildEventHandler
    {
        private string m_GameVersion;
        private int m_InternalResourceVersion;
        private string m_OutputDirectory;

        public bool ContinueOnFailure
        {
            get
            {
                return false;
            }
        }

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions,
            string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            m_GameVersion = applicableGameVersion;
            m_InternalResourceVersion = internalResourceVersion;
            m_OutputDirectory = outputDirectory;

            string streamingAssetsPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(streamingAssetsPath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                if (fileName.Contains(".gitkeep"))
                {
                    continue;
                }

                File.Delete(fileName);
            }

            Utility.Path.RemoveEmptyDirectory(streamingAssetsPath);
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions,
            string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
        }

        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode)
        {
            string platformPath = GetPlatformPath(platform);
            string gameVersion = m_GameVersion.Replace('.', '_');
            VersionInfo versionInfo = new VersionInfo
            {
                ForceUpdateGame = false,
                UpdatePrefixUri = string.Format("http://www.game.com/{0}_{1}/{2}", gameVersion, m_InternalResourceVersion.ToString(), platformPath),
                LatestGameVersion = m_GameVersion,
                InternalGameVersion = 1,
                InternalResourceVersion = m_InternalResourceVersion,
                VersionListLength = versionListLength,
                VersionListHashCode = versionListHashCode,
                VersionListCompressedLength = versionListCompressedLength,
                VersionListCompressedHashCode = versionListCompressedHashCode,
            };
            string versionJson = LitJson.JsonMapper.ToJson(versionInfo);
            IOUtility.SaveFileSafe(m_OutputDirectory, platformPath + "Version.txt", versionJson);

            Debug.LogFormat("Version save success. \n length is {0} , hash code is {1} . \n compressed length is {2} , compressed hash code is {3} . \n list path is {4} \n ", versionListLength, versionListHashCode, versionListCompressedLength, versionListCompressedHashCode, versionListPath);
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            if (!outputPackageSelected)
            {
                return;
            }

            if (platform != Platform.Windows && platform != Platform.Windows64)
            {
                return;
            }

            string streamingAssetsPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(outputPackagePath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string destFileName = Utility.Path.GetRegularPath(Path.Combine(streamingAssetsPath, fileName.Substring(outputPackagePath.Length)));
                FileInfo destFileInfo = new FileInfo(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(fileName, destFileName);
            }
        }
        
        /// <summary>
        /// 由 UnityGameFramework.Editor.ResourceTools.Platform 得到 平台标识符。
        /// </summary>
        /// <param name="platform">UnityGameFramework.Editor.ResourceTools.Platform。</param>
        /// <returns>平台标识符。</returns>
        public string GetPlatformPath(Platform platform)
        {
            // 这里和 ProcedureVersionCheck.GetPlatformPath() 对应。
            // 使用 平台标识符 关联 UnityEngine.RuntimePlatform 和 UnityGameFramework.Editor.ResourceTools.Platform
            switch (platform)
            {
                case Platform.Windows:
                case Platform.Windows64:
                    return "Windows";

                case Platform.MacOS:
                    return "MacOS";

                case Platform.IOS:
                    return "IOS";

                case Platform.Android:
                    return "Android";

                case Platform.WindowsStore:
                    return "WSA";

                case Platform.WebGL:
                    return "WebGL";

                case Platform.Linux:
                    return "Linux";

                default:
                    throw new GameFrameworkException("Platform is invalid.");
            }
        }
    }
}
