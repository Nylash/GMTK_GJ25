using UnityEngine;

public class PublicEnableDisable : MonoBehaviour
{
    public void EnableDisableThis()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
