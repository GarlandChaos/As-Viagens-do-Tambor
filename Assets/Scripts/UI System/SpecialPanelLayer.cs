using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.UI
{
    public class SpecialPanelLayer : ALayer<ISpecialPanelController>
    {
        public override void SaySize()
        {
            Debug.Log("Panel layer size is: " + screens.Count);
            base.SaySize();
        }
    }
}
