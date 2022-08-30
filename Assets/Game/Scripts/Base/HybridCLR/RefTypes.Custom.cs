using System.Collections.Generic;
using Game;
using GameFramework;
using UnityEngine;

enum ByteEnum : byte
{
    A,
    B,
}

public class RefTypes_Custom : MonoBehaviour
{
    // TODO 游戏中使用的泛型
    void RefGame()
    {
        new Dictionary<string, bool>();
        new Dictionary<byte, object>();
        new Dictionary<ByteEnum, object>();
        new KeyValuePair<ByteEnum, ByteEnum>();
        new Dictionary<KeyValuePair<ByteEnum, ByteEnum>, ByteEnum>();
        new Dictionary<KeyValuePair<ByteEnum, ByteEnum>, ByteEnum[]>();
        Utility.Text.Format<ByteEnum, string, string>(null, ByteEnum.A, null, null);
        GameEntry.Localization.GetString<string, string, string, string, string, string>(null, null, null, null, null, null, null);
    }
}
