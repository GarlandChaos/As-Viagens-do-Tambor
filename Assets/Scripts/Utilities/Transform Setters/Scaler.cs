using UnityEngine;

namespace TamborGame.Utilities
{
    public class Scaler : MonoBehaviour
    {
        void Start()
        {
            transform.localScale = Vector3.one * Camera.main.orthographicSize * 1.8f;
        }
    }
}