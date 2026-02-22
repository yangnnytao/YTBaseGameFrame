using UnityEngine;
using System.Collections;
namespace YGZFrameWork
{
    public interface ICommand
    {
        void Execute(IMessage message);
    }

}