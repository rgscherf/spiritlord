using UnityEngine;
using System.Collections;

public class PlayerClassFisherman : PlayerClass {
    // the fisherman is all about his fishing rod.
    // at first, only primary fire has an effect.
    // hold primary fire: cast line outward. then:
    // primary fire: catch a fish which EXPLODES after delay
    // secondary fire: catch a turnip, which returns to the player and gives health
    // tertiary fire: swiftly reel YOURSELF toward the line, damaging enemies along the way
    Color _baseColor;
    Sprite _baseSprite;

    public override float PlayerSpeed { get { return 2000f; } }
    public override Sprite ClassSprite {get { return _baseSprite; }}
    public override Color ClassColor {get { return _baseColor; }}
    public override void CallBaseStart() { BaseStart(); }
    public override void CallBaseUpdate() { BaseUpdate(); }

    void Awake() {
        var entities = GameObject.Find("GameController").GetComponent<Entities>();
        _baseSprite = entities.fishSprite;
        _baseColor = ClassColors.fishColor;

        primaryCooldownAmt = 0.5f;
        secondaryCooldownAmt = 0.5f;
        tertiaryCooldownAmt = 0.5f;
    }

    public override void FirePrimary() {}
    public override void FireSecondary() {}
    public override void FireTertiary() {}

}

