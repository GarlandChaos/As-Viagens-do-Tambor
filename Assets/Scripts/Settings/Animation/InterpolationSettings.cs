using UnityEngine;

namespace TamborGame.Settings
{
    [CreateAssetMenu]
    public class InterpolationSettings : ScriptableObject
    {
        [SerializeField]
        AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField]
        float duration = 1f;

        public AnimationCurve _Curve { get => curve; }
        public float _Duration { get => duration; }

    }
}