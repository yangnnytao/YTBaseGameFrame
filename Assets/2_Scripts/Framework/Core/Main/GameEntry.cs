using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 游戏入口
/// Editor:杨涛
/// </summary>
namespace YGZFrameWork
{
    public class GameEntry : MonoBehaviour//游戏入口
    {
        private GameApp m_game = GameApp.Instance;//GameAPP实例化

        void Start()//初始化调用
        {
            InitDataM();
        }

        private void InitDataM()//初始化数据
        {
            //单例获取
            m_game.mainMonoBehaviour = this;
            //防止误删
            DontDestroyOnLoad(gameObject);
            //2.执行StartUp执行lua开关
            AppFacade.Instance.StartUp();   //启动游戏

        }

        void Update()//每帧更新
        {
            m_game.Update();
        }

        void OnApplicationQuit()//退出调用
        {
            m_game.Terminate();
        }

        void OnApplicationFocus()//恢复时调用
        {
            m_game.ApplicationFocus();
        }

        void OnApplicationPause()//暂停时调用
        {
            m_game.ApplicationPause();
        }

        void OnDestroy()//销毁时调用
        {
            Debug.Log("OnDestroy");
            m_game.Terminate();
            RealseCollect();
        }

        void RealseCollect()//释放GC
        {//释放内存
            GC.Collect();
        }

        void OnGUI()//gui绘制||如果需要打LOG显示可以考虑用这个地方
        {
        }

        void OnEnable()//激活对象调用
        {
            m_game.ApplicationEnable();
        }
    }
}