using System.Collections.Generic;
using UnityEngine;

namespace TamborGame.UI 
{
    [CreateAssetMenu]
    public class UISettings : ScriptableObject
    {
        public List<GameObject> screensPrefabs =  new List<GameObject>();
    }
}
