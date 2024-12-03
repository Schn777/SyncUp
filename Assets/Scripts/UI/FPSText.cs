using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSText : MonoBehaviour
{

    private FPSManager fpsManager;
    private TextMeshProUGUI textMeshProUGUI;

    void Start()
    {
        fpsManager = FPSManager.Instance;
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        //textMeshProUGUI.text = "" + (int)fpsManager.FrameRate;
    }
}
