using UnityEngine;
using MLAPI;

[RequireComponent(typeof(SpriteRenderer))]
public class Pawn : NetworkBehaviour
{
    public string namePawn;
    public Sprite spritePawn;
}
