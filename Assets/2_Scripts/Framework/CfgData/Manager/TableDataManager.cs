using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using YGZFrameWork;

public class TableDataManager : ManagerBase<TableDataManager>,IManagerInterface
{
    public static Dictionary<int, HeroBaseCfgData> ItemDict { get; private set; }

    public static string mCfgDataPath = "F:\\004_Projects\\YTBaseGameFrameWork\\YTBaseGameFrameWork/Assets/2_Scripts/Config";

    public override void InitDataM()
    {
        base.InitDataM();
        LoadAll();
        RegisterMsg();
    }

    public override void DestroyM()
    {
    }

    public void RegisterMsg()
    {
    }

    public void ClearData()
    {
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadAll()
    {
        ItemDict = Load<HeroBaseCfgData>("HeroBaseCfgData.json", 0);   // 0 表示主键列索引
        foreach (var the in ItemDict)
        { 
            Debug.Log("YT--key:" + the.Key + "--id:" + the.Value.id + "--Job:" + the.Value.job);
        }
    }

    private static Dictionary<int, T> Load<T>(string fileName, int keyIndex) where T : struct
    {
#if UNITY_EDITOR
        var path = Path.Combine(mCfgDataPath, fileName);
#else
        var path = Path.Combine(Application.streamingAssetsPath, fileName);
#endif

        if (!File.Exists(path))
        {
            Debug.LogError($"配置文件不存在: {path}");
            return new Dictionary<int, T>();
        }

        try
        {
            var jsonText = File.ReadAllText(path);
            var dict = new Dictionary<int, T>();

            // 通用JSON解析：假设JSON是对象数组，使用反射映射字段
            var jsonList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonText);
            if (jsonList != null)
            {
                foreach (var jsonObj in jsonList)
                {
                    var obj = ParseJsonObject<T>(jsonObj);
                    var keyField = typeof(T).GetFields()[keyIndex];
                    int key = ConvertToInt(keyField.GetValue(obj));
                    dict[key] = obj;
                }
            }

            return dict;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"加载配置文件失败 {fileName}: {e.Message}");
            return new Dictionary<int, T>();
        }
    }

    /// <summary>
    /// 将JSON对象转换为结构体
    /// </summary>
    private static T ParseJsonObject<T>(Dictionary<string, object> jsonObj) where T : struct
    {
        var t = new T();
        var fields = typeof(T).GetFields();

        foreach (var field in fields)
        {
            // 尝试匹配字段名（不区分大小写）
            string fieldName = field.Name;
            object value = null;

            // 先尝试精确匹配
            if (jsonObj.ContainsKey(fieldName))
            {
                value = jsonObj[fieldName];
            }
            else
            {
                // 尝试首字母大写的匹配（如 id -> Id）
                string capitalizedName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);
                if (jsonObj.ContainsKey(capitalizedName))
                {
                    value = jsonObj[capitalizedName];
                }
            }

            if (value != null)
            {
                try
                {
                    if (field.FieldType == typeof(int))
                    {
                        field.SetValueDirect(__makeref(t), ConvertToInt(value));
                    }
                    else if (field.FieldType == typeof(float))
                    {
                        field.SetValueDirect(__makeref(t), ConvertToFloat(value));
                    }
                    else if (field.FieldType == typeof(string))
                    {
                        field.SetValueDirect(__makeref(t), value.ToString());
                    }
                    else if (field.FieldType == typeof(bool))
                    {
                        field.SetValueDirect(__makeref(t), ConvertToBool(value));
                    }
                    // 可继续扩展其他类型
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"字段 {fieldName} 类型转换失败: {e.Message}");
                }
            }
        }

        return t;
    }

    /// <summary>
    /// 转换为int
    /// </summary>
    private static int ConvertToInt(object value)
    {
        if (value == null) return 0;
        if (value is int) return (int)value;
        if (value is double) return (int)(double)value;
        if (value is float) return (int)(float)value;
        if (int.TryParse(value.ToString(), out int result)) return result;
        return 0;
    }

    /// <summary>
    /// 转换为float
    /// </summary>
    private static float ConvertToFloat(object value)
    {
        if (value == null) return 0f;
        if (value is float) return (float)value;
        if (value is double) return (float)(double)value;
        if (value is int) return (float)(int)value;
        if (float.TryParse(value.ToString(), out float result)) return result;
        return 0f;
    }

    /// <summary>
    /// 转换为bool
    /// </summary>
    private static bool ConvertToBool(object value)
    {
        if (value == null) return false;
        if (value is bool) return (bool)value;
        if (bool.TryParse(value.ToString(), out bool result)) return result;
        return false;
    }
}
