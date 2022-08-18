using UnityEngine;
using MLAPI;

namespace TamborGame.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Pawn : NetworkBehaviour
    {
        public string namePawn;
        public Sprite spritePawn;
    }
}