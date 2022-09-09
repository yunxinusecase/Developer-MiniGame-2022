using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mini.Battle.Core
{
    public enum GameStatus
    {
        None = -1,
        SplashLoading,     // 游戏初始化
        CharacterSelect,   // 角色选择
        GamePrepareRoom,   // 准备开始游戏界面
        GameLoading,       // 游戏初始化，创建网络角色，加载中
        GameMain,          // 游戏主场景
    }

}
