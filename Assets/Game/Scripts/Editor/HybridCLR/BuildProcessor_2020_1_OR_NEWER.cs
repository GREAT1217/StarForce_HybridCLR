using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HybridCLR.Editor.Builder;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Il2Cpp;
using UnityEditor.UnityLinker;
using UnityEngine;

namespace UnityEditor
{
    public class BuildProcessor_2020_1_OR_NEWER : IPreprocessBuildWithReport, IFilterBuildAssemblies, IPostBuildPlayerScriptDLLs, IUnityLinkerProcessor
#if !UNITY_2021_1_OR_NEWER
        , IIl2CppProcessor
#endif

#if UNITY_ANDROID
        , IPostGenerateGradleAndroidProject
#else
        , IPostprocessBuildWithReport
#endif

    {
        public int callbackOrder
        {
            get
            {
                return 0;
            }
        }

        public string[] OnFilterAssemblies(BuildOptions buildOptions, string[] assemblies)
        {
            // 将热更dll从打包列表中移除
            return assemblies.Where(ass => BuildConfig.AllHotUpdateDllNames.All(dll => !ass.EndsWith(dll, StringComparison.OrdinalIgnoreCase))).ToArray();
        }

        [Serializable]
        public class ScriptingAssemblies
        {
            public List<string> names;
            public List<int> types;
        }

#if UNITY_ANDROID
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // 由于 Android 平台在 OnPostprocessBuild 调用时已经生成完 apk 文件，因此需要提前调用
            AddBackHotFixAssembliesToJson(null, path);
        }
#endif

        public void OnPostprocessBuild(BuildReport report)
        {
#if !UNITY_ANDROID
            AddBackHotFixAssembliesToJson(report, report.summary.outputPath);
#endif
        }

        private void AddBackHotFixAssembliesToJson(BuildReport report, string path)
        {
            /*
             * ScriptingAssemblies.json 文件中记录了所有的dll名称，此列表在游戏启动时自动加载，
             * 不在此列表中的dll在资源反序列化时无法被找到其类型
             * 因此 OnFilterAssemblies 中移除的条目需要再加回来
             */
            string directoryName = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directoryName))
            {
                Debug.LogErrorFormat("ScriptingAssemblies.json path is invalid: {0}", path);
                return;
            }

            string[] jsonFiles = Directory.GetFiles(directoryName, "ScriptingAssemblies.json", SearchOption.AllDirectories);

            if (jsonFiles.Length == 0)
            {
                Debug.LogError("Can not find file ScriptingAssemblies.json");
                return;
            }

            foreach (string file in jsonFiles)
            {
                string content = File.ReadAllText(file);
                ScriptingAssemblies scriptingAssemblies = JsonUtility.FromJson<ScriptingAssemblies>(content);
                foreach (string name in BuildConfig.MonoHotUpdateDllNames)
                {
                    if (!scriptingAssemblies.names.Contains(name))
                    {
                        scriptingAssemblies.names.Add(name);
                        scriptingAssemblies.types.Add(16); // user dll type
                    }
                }
                content = JsonUtility.ToJson(scriptingAssemblies);
                File.WriteAllText(file, content);
                Debug.LogFormat("Update ScriptingAssemblies.json: {0}", file);
            }
        }

        public void OnPostBuildPlayerScriptDLLs(BuildReport report)
        {
#if UNITY_2021_1_OR_NEWER
            var buildTarget = report.summary.platform;
            CopyStripDlls(buildTarget);
#endif
        }

        public void OnBeforeConvertRun(BuildReport report, Il2CppBuildPipelineData data)
        {
#if !UNITY_2021_1_OR_NEWER
            CopyStripDlls(data.target);
#endif
        }

        private void CopyStripDlls(BuildTarget target)
        {
            var dstPath = BuildConfig.GetAssembliesPostIl2CppStripDir(target);
            Directory.CreateDirectory(dstPath);

            string srcStripDllPath = BuildConfig.GetOriginBuildStripAssembliesDir(target);
            foreach (var fileFullPath in Directory.GetFiles(srcStripDllPath, "*.dll"))
            {
                var file = Path.GetFileName(fileFullPath);
                var destFile = string.Format("{0}/{1}", dstPath, file);
                File.Copy(fileFullPath, destFile, true);
                Debug.LogFormat("Copy strip dll {0} ==> {1}", fileFullPath, destFile);
            }
        }

        #region useless

        private static void BuildExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
        }

        public void OnPreprocessBuild(BuildReport report)
        {
        }

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            return String.Empty;
        }

        public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

        public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
        {
        }

#if UNITY_IOS
    // hook UnityEditor.BuildCompletionEventsHandler.ReportPostBuildCompletionInfo() ? 因为没有 mac 打包平台因此不清楚
#endif

        #endregion
    }
}
