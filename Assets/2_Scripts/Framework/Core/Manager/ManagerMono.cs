using Unity.VisualScripting;
using UnityEngine;
using YGZFrameWork;

public class ManagerMono<T> : MonoBehaviour
{
    /// <summary> 内置事件 </summary>
    protected EventDispatcher eventDispatcher;

    public virtual void InitDataM()
    {
        RegisterMsg();
    }

    public virtual void DestroyM()
    {
        ClearAllEvents();
    }

    public virtual void RegisterMsg()
    {

    }

    #region 事件相关

    /// <summary> 添加事件监听 </summary>
    /// <param name="id">事件ID</param>
    /// <param name="handler">事件委托</param>
    /// <param name="save">是否是永久事件</param>
    protected void AddEventListener(int id, EventDispatcher.eventHandler handler, bool save = false)
    {
        eventDispatcher?.AddEvent(id, handler, save);
    }

    /// <summary> 发送事件 </summary>
    /// <param name="id"> 事件ID </param>
    /// <param name="objs"> 相关参数 </param>
    protected void DispatchEvent(int id, params object[] objs)
    {
        eventDispatcher?.DoEvent(id, objs);
    }

    /// <summary> 删除该ID所有事件 </summary>
    /// <param name="id"> 事件ID </param>
    protected void RemoveEventListener(int id)
    {
        eventDispatcher?.RemoveEvent(id);
    }

    /// <summary> 删除指定事件 </summary>
    /// <param name="id"> 事件ID </param>
    /// <param name="handler">  </param>
    protected void RemoveEventListener(int id, EventDispatcher.eventHandler handler)
    {
        eventDispatcher?.RemoveEvent(id, handler);
    }

    /// <summary> 清空所有事件 </summary>
    protected void ClearAllEvents()
    {
        eventDispatcher?.ClearEvent();
    }

    #endregion 事件相关_end
}