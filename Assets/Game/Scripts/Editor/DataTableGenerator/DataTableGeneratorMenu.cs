//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.DataTableTools
{
    public sealed class DataTableGeneratorMenu
    {
        [MenuItem("Generator/DataTables")]
        private static void GenerateDataTables()
        {
            string[] dataTableNames = new string[]
            {
                "Aircraft",
                "Armor",
                "Asteroid",
                "Entity",
                "Music",
                "Scene",
                "Sound",
                "Thruster",
                "UIForm",
                "UISound",
                "Weapon",
            };
            
            foreach (string dataTableName in dataTableNames)
            {
                DataTableProcessor dataTableProcessor = DataTableGenerator.CreateDataTableProcessor(dataTableName);
                if (!DataTableGenerator.CheckRawData(dataTableProcessor, dataTableName))
                {
                    Debug.LogError(Utility.Text.Format("Check raw data failure. DataTableName='{0}'", dataTableName));
                    break;
                }
            
                DataTableGenerator.GenerateDataFile(dataTableProcessor, dataTableName);
                DataTableGenerator.GenerateCodeFile(dataTableProcessor, dataTableName);
            }

            AssetDatabase.Refresh();
        }
    }
}
