using UnityEngine;
using System.Collections.Generic;
namespace YGZFrameWork
{
    public class AppView : View
    {
        private string message;

        ///<summary> 监听的消息 </summary>
        List<string> MessageList
        {
            get
            {
                return new List<string>()
            {
                NotiConst.UPDATE_MESSAGE,
                NotiConst.UPDATE_EXTRACT,
                NotiConst.UPDATE_DOWNLOAD,
                NotiConst.UPDATE_PROGRESS,
            };
            }
        }

        void Awake()
        {
            RemoveMessage(this, MessageList);
            RegisterMessage(this, MessageList);
        }

        /// <summary> 处理View消息 </summary>
        public override void OnMessage(IMessage message)
        {
            string name = message.Name;
            object body = message.Body;
            switch (name)
            {
                case NotiConst.UPDATE_MESSAGE:
                    UpdateMessage(body.ToString());//更新消息
                    break;
                case NotiConst.UPDATE_EXTRACT:
                    UpdateExtract(body.ToString());//更新解压
                    break;
                case NotiConst.UPDATE_DOWNLOAD:
                    UpdateDownload(body.ToString());//更新下载
                    break;
                case NotiConst.UPDATE_PROGRESS:
                    UpdateProgress(body.ToString());//更新下载进度
                    break;
            }
        }

        /// <summary> 更新消息 </summary>
        public void UpdateMessage(string data)
        {
            this.message = data;
        }

        /// <summary> 更新提取 </summary>
        public void UpdateExtract(string data)
        {
            this.message = data;
        }

        /// <summary> 更新下载 </summary>
        public void UpdateDownload(string data)
        {
            this.message = data;
        }

        /// <summary> 更新进程 </summary>
        public void UpdateProgress(string data)
        {
            this.message = data;
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 120, 960, 50), message);

            GUI.Label(new Rect(10, 0, 500, 50), "(1) 单击 \"Lua/Gen Lua Wrap Files\"。");
            GUI.Label(new Rect(10, 20, 500, 50), "(2) 运行Unity游戏");
            GUI.Label(new Rect(10, 40, 500, 50), "PS: 清除缓存，单击\"Lua/Clear LuaBinder File + Wrap Files\"。");
            GUI.Label(new Rect(10, 60, 900, 50), "PS: 若运行到真机，请设置Const.DebugMode=false，本地调试请设置Const.DebugMode=true");
            GUI.Label(new Rect(10, 80, 500, 50), "PS: 加Unity+ulua技术讨论群：>>341746602");
        }
    }
}