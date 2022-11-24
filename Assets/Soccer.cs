using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Soccer : MonoBehaviour
{
    public static Soccer instance;
    public static Dictionary<string, int> goals = new Dictionary<string, int>();

    [SerializeField]
    TMP_Text A;
    [SerializeField]
    TMP_Text B;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            goals.Clear();
            goals.Add("A", 0);
            goals.Add("B", 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        A.text = goals["A"].ToString();
        B.text = goals["B"].ToString();
    }
}
