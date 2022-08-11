using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.UI
{
    public class ASpecialPanelController : MonoBehaviour, ISpecialPanelController
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
}
