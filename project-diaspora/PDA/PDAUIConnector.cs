using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PDAUIConnector : MonoBehaviour
{
    public Button closeButton;

    void Start()
    {

        PDAController pda = FindObjectOfType<PDAController>();
        PDAController controller = FindObjectOfType<PDAController>();

        if (pda != null)
        {
            pda.RegisterFirstTimeUI(this.gameObject);
        }
        else
        {
            Debug.LogWarning("[PDAUIConnector] 씬에서 PDAController를 찾을 수 없습니다!");
        }

        if (controller != null)
        {

            closeButton.onClick.AddListener(controller.CloseFirstTimeUI);
        }
        else
        {
            Debug.LogError("PDAController를 찾을 수 없습니다!");
        }
    }
}
