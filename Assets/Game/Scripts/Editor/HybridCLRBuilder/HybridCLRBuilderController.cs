using System;
using System.IO;
using Game.Hotfix;
using GameFramework;
using HybridCLR.Editor;
using UnityEditor;
using UnityGameFramework.Editor.ResourceTools;
using Debug = UnityEngine.Debug;

namespace Game.Editor
{
    public class HybridCLRBuilderController
    {
        private const string HotfixDllPath = "Assets/Game/Hotfix";
        private const string HotfixDllName = "Game.Hotfix.dll";

        public string[] PlatformNames { get; }

        public HybridCLRBuilderController()
        {
            PlatformNames = Enum.GetNames(typeof(Platform));
        }

        /// <summary>
        /// 由 UnityGameFramework.Editor.ResourceTools.Platform 得到 BuildTarget。
        /// </summary>
        /// <param name="platformIndex"></param>
        /// <returns>BuildTarget。</returns>
        public BuildTarget GetBuildTarget(int platformIndex)
        {
            Platform platform = (Platform) Enum.Parse(typeof(Platform), PlatformNames[platformIndex]);
            switch (platform)
            {
                case Platform.Windows:
                    return BuildTarget.StandaloneWindows;

                case Platform.Windows64:
                    return BuildTarget.StandaloneWindows64;

                case Platform.MacOS:
#if UNITY_2017_3_OR_NEWER
                    return BuildTarget.StandaloneOSX;
#else
                    return BuildTarget.StandaloneOSXUniversal;
#endif
                case Platform.Linux:
                    return BuildTarget.StandaloneLinux64;

                case Platform.IOS:
                    return BuildTarget.iOS;

                case Platform.Android:
                    return BuildTarget.Android;

                case Platform.WindowsStore:
                    return BuildTarget.WSAPlayer;

                case Platform.WebGL:
                    return BuildTarget.WebGL;

                default:
                    throw new GameFrameworkException("Platform is invalid.");
            }
        }

        /// <summary>
        /// 将 dll 文件拷贝至项目目录，用于 AssetBundle 资源的编辑和打包。
        /// </summary>
        /// <param name="buildTarget"></param>
        public void CopyDllAssets(BuildTarget buildTarget)
        {
            IOUtility.CreateDirectoryIfNotExists(HotfixDllPath);
            string buildPath = BuildConfig.GetHotFixDllsOutputDirByTarget(buildTarget);

            // Copy Hotfix Dll
            string oriFileName = string.Format("{0}/{1}", buildPath, HotfixDllName);
            string desFileName = string.Format("{0}/{1}.bytes", HotfixDllPath, HotfixDllName);
            File.Copy(oriFileName, desFileName, true);

            // Copy AOT Dll
            string aotDllPath = string.Format("{0}/{1}", BuildConfig.AssembliesPostIl2CppStripDir, buildTarget);
            foreach (var dllName in GameHotfixEntry.AOTDllNames)
            {
                oriFileName = string.Format("{0}/{1}", aotDllPath, dllName);
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOT dll.");
                    continue;
                }
                desFileName = string.Format("{0}/{1}.bytes", HotfixDllPath, dllName);
                File.Copy(oriFileName, desFileName, true);
            }

            Debug.Log("Hotfix dll build complete.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
