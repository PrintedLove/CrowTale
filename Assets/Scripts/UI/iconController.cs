using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconController : MonoBehaviour
{
    public string descripton;

    public void OnPointerEnter()
    {
        GameManager.Instance.UpdateIconDescription(descripton);
    }

    public void OnPointerExit()
    {
        GameManager.Instance.UpdateIconDescription("");
    }
}
