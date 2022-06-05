using GameFramework;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace Game.Hotfix
{
    public class GameHotfixEntry
    {
        public static HPBarComponent HPBar
        {
            get;
            private set;
        }

        public static void Start()
        {
            // 删除原生对话框。
            GameEntry.BuiltinData.DestroyDialog();

            // 重置流程组件，初始化热更新流程。
            GameEntry.Fsm.DestroyFsm<IProcedureManager>();
            var procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();
            ProcedureBase[] procedures =
            {
                new ProcedureChangeScene(),
                new ProcedureMain(),
                new ProcedureMenu(),
                new ProcedurePreload(),
            };
            procedureManager.Initialize(GameFrameworkEntry.GetModule<IFsmManager>(), procedures);
            procedureManager.StartProcedure<ProcedurePreload>();

            // 加载自定义组件。
            GameEntry.Resource.LoadAsset("Assets/Game/Game.prefab", new LoadAssetCallbacks(OnLoadAssetSuccess, OnLoadAssetFail));
        }

        private static void OnLoadAssetSuccess(string assetName, object asset, float duration, object userdata)
        {
            GameObject game = Object.Instantiate((GameObject)asset);
            game.name = "Game";

            HPBar = game.GetComponentInChildren<HPBarComponent>();
        }

        private static void OnLoadAssetFail(string assetName, LoadResourceStatus status, string errormessage, object userdata)
        {
            Log.Error("Load game failed. {0}", errormessage);
        }

    }
}
