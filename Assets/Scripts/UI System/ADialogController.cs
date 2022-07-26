using UnityEngine;

namespace System.UI
{
    public abstract class ADialogController : MonoBehaviour, IDialogController
    {
        public string screenID { get; set; }
        public bool isVisible { get; set; }

        public virtual void Show()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                gameObject.transform.SetAsLastSibling();
                isVisible = true;
            }
        }

        //Don't call this function inside a dialog controller, make the dialog layer hide it
        public virtual void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                isVisible = false;
            }
        }
    }
}