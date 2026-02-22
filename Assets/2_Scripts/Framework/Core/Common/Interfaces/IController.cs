using System;
using System.Collections.Generic;

/// <summary>
/// UI控制器事件接口
/// 负责UI控制器事件的注册、移除、执行
/// </summary>
public interface IController
{
    /// <summary> 注册命令 </summary>
    void RegisterCommand(string messageName, Type commandType);

    /// <summary> 注册查看命令 </summary>
    void RegisterViewCommand(IView view, string[] commandNames);

    /// <summary> 执行命令 </summary>
    void ExecuteCommand(IMessage message);

    /// <summary> 删除命令 </summary>
	void RemoveCommand(string messageName);

    /// <summary> 删除查看命令 </summary>
    void RemoveViewCommand(IView view, string[] commandNames);

    /// <summary> 拥有命令 </summary>
	bool HasCommand(string messageName);
}
