using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mini.Battle.Core
{
    public enum GameStatus
    {
        None = -1,
        SplashLoading,     // ��Ϸ��ʼ��
        CharacterSelect,   // ��ɫѡ��
        GamePrepareRoom,   // ׼����ʼ��Ϸ����
        GameLoading,       // ��Ϸ��ʼ�������������ɫ��������
        GameMain,          // ��Ϸ������
    }

}
