using UnityEngine;

public class Layers {

    public static readonly int Collectable = LayerMask.NameToLayer("Collectable");
    public static readonly int Ground = LayerMask.NameToLayer("Ground");
    public static readonly int Rocket = LayerMask.NameToLayer("Rocket");
    public static readonly int Player = LayerMask.NameToLayer("Player");
    public static readonly int Enemy = LayerMask.NameToLayer("Enemy");

    public static int Mask(int layer) { return (1 << layer); }
}
