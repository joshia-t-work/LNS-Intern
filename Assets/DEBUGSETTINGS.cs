using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DEBUGSETTINGS : MonoBehaviour
{
    public static DEBUGSETTINGS instance;
    public static int DETAIL => instance ? instance.detail : 0;

    [SerializeField]
    int detail;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }
}
