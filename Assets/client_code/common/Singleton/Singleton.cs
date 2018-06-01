using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> where T : class, new()
{
    private static T m_ins;
    public static T Instance
    {
        get
        {
            if (m_ins == null)
            {
                m_ins = default(T);
            }
            return m_ins;
        }
    }



}
#if UNITY_EDITOR
#endif