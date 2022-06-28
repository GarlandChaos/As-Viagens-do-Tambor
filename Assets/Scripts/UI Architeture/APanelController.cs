using UnityEngine;

public abstract class APanelController : MonoBehaviour, IPanelController
{
    public string screenID { get; set; }
    public bool isVisible { get; set; }

    public virtual void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            isVisible = true;
        }
    }

    public virtual void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            isVisible = false;
        }
    }
}
