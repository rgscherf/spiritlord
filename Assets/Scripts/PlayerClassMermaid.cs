using UnityEngine;
using System.Collections;

public class PlayerClassMermaid : PlayerClass {
    // MERMAID
    // mermaid is a powerful hit-and-run melee attacker
    // with awesome mobility

    // the fisherman is all about his fishing rod.
    // at first, only primary fire has an effect.
    // hold primary fire: cast line outward. then:
    // primary fire: catch a fish which EXPLODES after delay
    // secondary fire: catch a turnip, which returns to the player and gives health
    // tertiary fire: swiftly reel YOURSELF toward the line, damaging enemies along the way

    // primary
    // a quick lance stab

    // secondary
    // cast a line outward (hold to cast farther)
    // when released, throw a lure which drops into 'water'
    // from this state:
    // press primary to pull up a fish, which shortly explodes
    // press secondary to reel YOURSELF to the lure, causing damage along the way

    // tertiary
    // drop 'water' particles behind yourself at all times.
    // hold tertiary fire to move faster on water
    // (promotes melee hit-and-run)

    Entities entities;

    Color _baseColor;
    Sprite _baseSprite;

    public override float PlayerSpeed { get { return 2000f; } }
    public override Sprite ClassSprite {get { return _baseSprite; }}
    public override Color ClassColor {get { return _baseColor; }}
    public override void CallBaseStart() { BaseStart(); }
    public override void CallBaseUpdate() { BaseUpdate(); }

    void Awake() {
        entities = GameObject.Find("GameController").GetComponent<Entities>();
        _baseSprite = entities.mermaidSprite;
        _baseColor = ClassColors.mermaidColor;

        primaryCooldownAmt = 0.6f;
        secondaryCooldownAmt = 0.5f;
        tertiaryCooldownAmt = 0.5f;
    }

    void Start() {
        CallBaseStart();
    }

    void Update() {
        CallBaseUpdate();
    }

    public override void FirePrimary() {
        if (primaryCooldown.Check()) {
            var pos = transform.position + transform.TransformDirection(new Vector3 (0.5f, 0.5f, 0f));
            var rot = transform.rotation * Quaternion.Euler(new Vector3(0f, 0f, -135f));
            var t = (GameObject) Instantiate(entities.mermaidTrident, pos, rot);
            t.GetComponent<SpriteRenderer>().color = ClassColor;
            t.transform.SetParent(gameObject.transform);
            primaryCooldown.Reset();
        }
    }

    public override void FireSecondary() {}
    public override void FireTertiary() {}

}

