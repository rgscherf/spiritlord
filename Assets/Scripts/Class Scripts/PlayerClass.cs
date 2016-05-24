using UnityEngine;
using System.Collections;

public abstract class PlayerClass : MonoBehaviour {
    public abstract float PlayerSpeed {get;}
    public abstract Sprite ClassSprite {get;}
    public abstract void FirePrimary();
    public abstract void FireSecondary();
    public abstract void FireTertiary();
}
