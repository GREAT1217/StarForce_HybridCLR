using System.Collections.Generic;

namespace HybridCLR.Editor
{
    public static partial class BuildConfig
    {
        /// <summary>
        /// 所有热更新dll列表。放到此列表中的dll在打包时OnFilterAssemblies回调中被过滤。
        /// </summary>
        public static List<string> HotUpdateAssemblies { get; } = new List<string>
        {
            "Game.Hotfix.dll"
        };
    }
}
