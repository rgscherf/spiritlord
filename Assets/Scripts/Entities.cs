using UnityEngine;
using System.Collections;

public class Entities : MonoBehaviour {
    public GameObject particle;

    public Sprite sprite1;
    public Sprite sprite2;
    public Sprite sprite4;
    public Sprite sprite8;
    public Sprite sprite16;
    public Sprite sprite32;

    public Sprite chefSprite;
    public GameObject chefFirePrimary;
    public GameObject chefFireSecondary;
    public GameObject chefFireTertiary;

    public Sprite mermaidSprite;
    public GameObject mermaidTrident;

    public static Vector2 OutwardExplosionVector(Vector2 explosionSource, Vector2 explodedEntityPosition, float blastForce) {
        var dir = (explodedEntityPosition - explosionSource).normalized;
        return blastForce * dir;
    }

}
