using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class iconController : MonoBehaviour
{
    public string descripton;

    public void OnPointerEnter()
    {
        GameManager.Instance.UpdateIconDescription(descripton);
        Debug.Log(descripton);
    }

    public void OnPointerExit()
    {
        GameManager.Instance.UpdateIconDescription("");
        Debug.Log("clear");
    }
}
