using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Game;
using Game.Hotfix;
using GameFramework.Fsm;
using GameFramework.Procedure;
using GameFramework.Resource;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace HybridCLR.Generators
{
    internal class GeneratorConfig
    {
        /// <summary>
        /// 目前已经根据热更新dll的依赖自动计算需要扫描哪些dll来收集桥接函数。
        /// 只要你的热更新以assembly def形式放到项目中，是不需要改这个的
        /// </summary>
        /// <returns></returns>
        public static List<string> GetExtraAssembiles()
        {
            return new List<string>
            {
                // "mscorlib",
            };
        }

        /// <summary>
        /// 暂时没有仔细扫描泛型，如果运行时发现有生成缺失，先手动在此添加类
        /// </summary>
        /// <returns></returns>
        public static List<Type> PrepareCustomGenericTypes()
        {
            return new List<Type>
            {
                typeof(Action<int, string, Vector3>),

                typeof(BaseComponent),
                typeof(ConfigComponent),
                typeof(DataNodeComponent),
                typeof(DataTableComponent),
                typeof(DebuggerComponent),
                typeof(DownloadComponent),
                typeof(EntityComponent),
                typeof(EventComponent),
                typeof(FileSystemComponent),
                typeof(FsmComponent),
                typeof(LocalizationComponent),
                typeof(NetworkComponent),
                typeof(ObjectPoolComponent),
                typeof(ProcedureComponent),
                typeof(ResourceComponent),
                typeof(SceneComponent),
                typeof(SettingComponent),
                typeof(SoundComponent),
                typeof(SoundComponent),
                typeof(UIComponent),
                typeof(WebRequestComponent),
                typeof(BuiltinDataComponent),
                typeof(Log),

                typeof(Dictionary<KeyValuePair<CampType, CampType>, RelationType>),
                typeof(Dictionary<KeyValuePair<CampType, RelationType>, CampType[]>),
            };
        }

        /// <summary>
        /// 如果提示缺失桥接函数，将提示缺失的签名加入到下列列表是简单的做法。
        /// 这里添加64位App缺失的桥接函数签名
        /// </summary>
        /// <returns></returns>
        public static List<string> PrepareCustomMethodSignatures64()
        {
            return new List<string>
            {
                "vi8i8",
                "i4i8i8i8i4i1i8",
                "vi8i1i8",
                "S40i8",
                "S16i8",
                "vi8i4i8",
                "vi8i8i4i8",
                "i8i8i1i8i8",
                "vi8i4i4",
                "vi8i1",
                "i4i8i8i8i4i8i8i8",
                "i8i1",
                "S24i8",
                "i1i8S2i8"
            };
        }

        /// <summary>
        /// 如果提示缺失桥接函数，将提示缺失的签名加入到下列列表是简单的做法。
        /// 这里添加32位App缺失的桥接函数签名
        /// </summary>
        /// <returns></returns>
        public static List<string> PrepareCustomMethodSignatures32()
        {
            return new List<string>
            {
                "vi4i4",
            };
        }
    }
}
