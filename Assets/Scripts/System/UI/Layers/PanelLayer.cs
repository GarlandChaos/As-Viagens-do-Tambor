using UnityEngine;

namespace TamborGame.UI
{
    public class PanelLayer : ALayer<IPanelController>
    {
        public override void SaySize()
        {
            Debug.Log("Panel layer size is: " + screens.Count);
            base.SaySize();
        }
    }
}
