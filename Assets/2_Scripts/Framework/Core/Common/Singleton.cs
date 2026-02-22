/// <summary>
/// 单例(非Mono)
/// </summary>
public class Singleton<T> where T : class, new()
{
    /// <summary>  Static Fields </summary>
    protected static T m_Instance;

    /// <summary> Static Properties </summary>
    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = new T();
            }
            return m_Instance;
        }
    }

    public virtual void InitDataM() { }
    public virtual void DestroyM() { }

    /// <summary> Static Methods </summary>
    public static T GetInstance()
    {
        return Instance;
    }

}
