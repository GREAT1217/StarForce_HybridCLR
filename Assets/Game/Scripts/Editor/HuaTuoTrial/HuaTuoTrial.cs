using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using GameFramework;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Editor
{
    public class HuaTuoTrial : EditorWindow
    {
        private static string s_InitDat;
        private static string s_SetVersionDat;
        private static string s_InitLocalDat;
        private static string[] s_UnityVersion = {
            "2020.3.33",
            "2021.3.1"
        };

        private static int s_UnityVersionSelect;
        private static string s_UnityInstallDirectory;

        [MenuItem("Game/Huatuo Trial", false, 0)]
        private static void Open()
        {
            HuaTuoTrial window = GetWindow<HuaTuoTrial>("HuaTuoTrial", true);
            window.minSize = new Vector2(800f, 400f);

            s_InitDat = Application.dataPath + "/../HuatuoData/init_huatuo_repos.bat";
            s_SetVersionDat = Application.dataPath + "/../HuatuoData/set_version.bat";
            s_InitLocalDat = Application.dataPath + "/../HuatuoData/init_local_il2cpp_data.bat";

            if (string.IsNullOrEmpty(s_UnityInstallDirectory))
            {
                s_UnityInstallDirectory = "C:/Software/UnityEditor/Unity 2020.3.33f1c2/Editor/Data/il2cpp";
            }
        }

        private void OnGUI()
        {
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Environment", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            s_UnityVersionSelect = EditorGUILayout.Popup("Unity版本", s_UnityVersionSelect, s_UnityVersion);
            GUISelect(ref s_UnityInstallDirectory, "Unity安装目录的il2cpp", "Select");
            EditorGUILayout.EndVertical();

            // 安装
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Install Huatuo：(git and network required)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            GUIItem("初始化Huatuo仓库，需要git和网络。", "Start", InitHuatuoRepos);
            GUIItem("设置Huatuo仓库适配的Unity版本分支。", "Start", SetVersion);
            GUIItem("安装Huatuo到本项目。", "Start", InitLocalHuatuo);
            EditorGUILayout.EndVertical();

            // 设置
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            GUIItem("由于ab包依赖裁剪后的dll，因此首先需要build工程。", "Build", BuildPlayerWindow.ShowBuildPlayerWindow);
            EditorGUILayout.EndVertical();
        }

        private void GUIItem(string content, string button, Action onClick)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(content);
            if (GUILayout.Button(button, GUILayout.Width(100)))
            {
                onClick?.Invoke();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void GUISelect(ref string directory, string content, string selectButton)
        {
            EditorGUILayout.BeginHorizontal();
            directory = EditorGUILayout.TextField(content, directory);
            if (GUILayout.Button(selectButton, GUILayout.Width(100)))
            {
                string temp = EditorUtility.OpenFolderPanel(content, directory, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    directory = temp;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void InitHuatuoRepos()
        {
            if (!File.Exists(s_InitDat))
            {
                Debug.LogErrorFormat("File not Exit : {0}", s_InitDat);
                return;
            }
            Debug.LogFormat(s_InitDat);

            RunProcess(s_InitDat);
        }

        private void SetVersion()
        {
            if (!File.Exists(s_SetVersionDat))
            {
                Debug.LogErrorFormat("File not Exit : {0}", s_SetVersionDat);
                return;
            }
            Debug.LogFormat(s_SetVersionDat);

            var fileLines = File.ReadAllLines(s_SetVersionDat);
            Debug.LogFormat(fileLines[1]);
            string version = s_UnityVersion[s_UnityVersionSelect];
            fileLines[1] = Utility.Text.Format("set BRANCH={0}", version);
            File.WriteAllLines(s_SetVersionDat, fileLines, Encoding.UTF8);

            RunProcess(s_SetVersionDat);
        }

        private void InitLocalHuatuo()
        {
            if (!File.Exists(s_InitLocalDat))
            {
                Debug.LogErrorFormat("File not Exit : {0}", s_InitLocalDat);
                return;
            }
            Debug.LogFormat(s_InitLocalDat);

            var fileLines = File.ReadAllLines(s_InitLocalDat);
            Debug.LogFormat(fileLines[3]);
            string path = s_UnityInstallDirectory.Replace('/', '\\');
            fileLines[3] = Utility.Text.Format("set IL2CPP_PATH={0}", path);
            File.WriteAllLines(s_InitLocalDat, fileLines, Encoding.UTF8);

            RunProcess(s_InitLocalDat);
        }

        private void RunProcess(string fileName)
        {
            using (Process p = new Process())
            {
                p.StartInfo.WorkingDirectory = Application.dataPath + "/../HuatuoData";
                p.StartInfo.FileName = fileName;
                p.StartInfo.UseShellExecute = true;
                p.Start();
                p.WaitForExit();
            }
        }
    }
}
