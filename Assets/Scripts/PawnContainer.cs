using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PawnContainer : ScriptableObject
{
    [SerializeField]
    List<Pawn> pawns = new List<Pawn>();

    public List<Pawn> _Pawns { get => pawns; }
}
