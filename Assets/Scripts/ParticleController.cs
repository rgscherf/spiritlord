using UnityEngine;
using System.Collections;

public enum ParticleType {
    effects,
    playerWeapon,
    enemyWeapon,
    mermaidWater
};

public class ParticleController : MonoBehaviour {

    // possible types of particles
    // trying to keep it general.
    // potion lord had convoluted particle types.

    public ParticleType particleType;

    SpriteRenderer spr;
    Entities entities;

    // particle coloring and flicker values
    Color color;
    Color[] flickerColorPalette;
    bool flicker;

    void Awake() {
        entities = GameObject.Find("GameController").GetComponent<Entities>();
        spr = GetComponent<SpriteRenderer>();
    }
    void Start() {
    }

    void Update() {
        spr.color = flicker ? flickerColorPalette[Random.Range(0, flickerColorPalette.Length)] : color;
    }

    public void Init(ParticleType pType, bool doesFlicker, Color col, float dTimer, int size) {
        particleType = pType;
        MountSprite(size);
        color = col;
        flicker = doesFlicker;
        if (flicker) {
            flickerColorPalette = new Color[] { col, col * new Color(1, 1, 1, 0.6f), col * new Color(1, 1, 1, 0.8f)};
        }
        CallDeathTimer(dTimer);
    }

    void MountSprite(int size) {
        Sprite ret;
        switch (size) {
            case 1:
                ret = entities.sprite1;
                break;
            case 2:
                ret = entities.sprite2;
                break;
            case 4:
                ret = entities.sprite4;
                break;
            case 8:
                ret = entities.sprite8;
                break;
            case 16:
                ret = entities.sprite16;
                break;
            case 32:
                ret = entities.sprite32;
                break;
            default:
                throw new System.NotImplementedException("Tried to set illegal particle size");
        }
        spr.sprite = ret;
        float collidersize = size / 16f;
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(collidersize, collidersize);
    }

    void CallDeathTimer(float t) {
        Object.Destroy(gameObject, t);
    }

    public void AddForce(Vector2 direction) {
        gameObject.GetComponent<Rigidbody2D>().AddForce(direction);
    }

    public void AddTorque(float force) {
        gameObject.GetComponent<Rigidbody2D>().AddTorque(force);
    }
}
