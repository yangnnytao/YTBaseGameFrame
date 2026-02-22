using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerOfManager : Singleton<ManagerOfManager>
{
    public override void InitDataM()
    {
        TableDataManager.Instance.InitDataM();
        //GameDataManager.Instance.InitDataM();
    }

    public override void DestroyM()
    {
        TableDataManager.Instance.DestroyM();
        //GameDataManager.Instance.DestroyM();
    }
}
