using System.Linq;
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
    bool _blockEnemyPathfinding;
    float _baseSpeed = 2000f;
    float _speedMult = 1f;

    public override float PlayerSpeed { get { return _baseSpeed * _speedMult; } }
    public override Sprite ClassSprite {get { return _baseSprite; }}
    public override Color ClassColor {get { return _baseColor; }}
    public override void CallBaseStart() { BaseStart(); }
    public override void CallBaseUpdate() { BaseUpdate(); }

    Timer underwaterTimer;
    bool underwater;
    Sprite underwaterSprite;
    Timer spcRepeatGuard;

    bool droppingWater;

    const float coolDownBonusForEndingDiveEarly = 0.5f;

    void Awake() {
        entities = GameObject.Find("GameController").GetComponent<Entities>();
        _baseSprite = entities.mermaidSprite;
        _baseColor = ClassColors.mermaidColor;

        primaryCooldownAmt = 0.4f;
        secondaryCooldownAmt = 3f;
        tertiaryCooldownAmt = 2f;

        underwaterTimer = new Timer(1.5f, true);
        underwaterSprite = entities.mermaidUnderwaterSprite;
    }

    void Start() {
        BlockEnemyPathing(false);
        spcRepeatGuard = new Timer(0.2f);
        droppingWater = true;
        CallBaseStart();
    }

    void Update() {
        spcRepeatGuard.Tick(Time.deltaTime);
        underwaterTimer.Tick(Time.deltaTime);
        if (underwater) {
            if (NotTouchingWater() || underwaterTimer.Check()) {
                UnderwaterDisable();
                tertiaryCooldown = new Timer(tertiaryCooldownAmt);
            }
        }

        if (droppingWater) { DropWater(); }

        if (secondaryWinding && Input.GetAxisRaw("FireSecondary") == 0f) {
            ReleaseFish();
        }

        CallBaseUpdate();
    }

    bool NotTouchingWater() {
        var w = Physics2D.OverlapCircleAll(transform.position, 3f)
                .Where( c => c.gameObject.tag == "Mermaidwater")
                .ToArray()
                .Length;
        return w == 0;
    }

    void DropWater() {
        var nearestX = (int) Mathf.Round(transform.position.x);
        var nearestY = (int) Mathf.Round(transform.position.y);
        for (var x = -1; x <= 1; x++) {
            for (var y = -1; y <= 1; y++) {
                var nearestIntPos = new Vector3(nearestX + x, nearestY + y, 1f);
                var p = Physics2D.OverlapCircleAll((Vector2) nearestIntPos, 0.1f)
                        .Where( c => c.gameObject.tag == "Mermaidwater")
                        .ToArray()
                        .Length;
                if (p != 0 ) { continue; }
                var w = (GameObject) Instantiate(entities.mermaidWaterSprite, nearestIntPos, Quaternion.identity);
                w.GetComponent<SpriteRenderer>().color = ClassColors.mermaidColor * new Color(1f, 1f, 1f, 0.5f);
                Object.Destroy(w, 6f);
            }
        }
    }

    public override void FirePrimary() {
        if (!underwater && primaryCooldown.Check()) {
            var pos = transform.position + transform.TransformDirection(new Vector3 (0.5f, 0.2f, 0f));
            var rot = transform.rotation * Quaternion.Euler(new Vector3(0f, 0f, -135f));
            var t = (GameObject) Instantiate(entities.mermaidTrident, pos, rot);
            t.GetComponent<SpriteRenderer>().color = ClassColor;
            t.transform.SetParent(gameObject.transform);
            primaryCooldown.Reset();
        }
    }

    bool secondaryWinding;
    float timeHeld;
    const float maxTimeHeld = 2;
    GameObject currentFish;
    float maxForce = 3500f;

    void ReleaseFish() {
        secondaryWinding = false;
        secondaryCooldown.Reset();

        currentFish.GetComponent<Rigidbody2D>().AddForce(transform.up * (maxForce * (timeHeld / maxTimeHeld)));
    }

    public override void FireSecondary() {
        // we are cheating the contract by checking for fire release in Update().
        if (secondaryCooldown.Check()) {
            if (!secondaryWinding) {
                currentFish = (GameObject) Instantiate(entities.mermaidFish, transform.position + transform.TransformDirection(new Vector3(0f, -1f, 0f)), transform.rotation);
                timeHeld = 0;
                secondaryWinding = true;
            } else {
                timeHeld += Time.deltaTime;
                currentFish.transform.rotation = transform.rotation;
                currentFish.transform.position = transform.position + transform.TransformDirection(new Vector3(0f, -1f, 0f));
                currentFish.GetComponent<SpriteRenderer>().color = Color.Lerp(ClassColors.mermaidColor * new Color(1f, 1f, 1f, 0.2f),
                        ClassColors.mermaidColor,
                        timeHeld / maxTimeHeld);

                if (timeHeld >= maxTimeHeld) {
                    ReleaseFish();
                }
            }
        }
    }

    public override void FireTertiary() {
        if (spcRepeatGuard.Check()) {
            if (underwater) {
                UnderwaterDisable();
                tertiaryCooldown = new Timer(tertiaryCooldownAmt * coolDownBonusForEndingDiveEarly);
            } else {
                if (tertiaryCooldown.Check()) {
                    UnderwaterEnable();
                    tertiaryCooldown = new Timer(tertiaryCooldownAmt);
                }
            }
            spcRepeatGuard.Reset();
        }
    }

    void UnderwaterEnable() {
        underwater = true;
        underwaterTimer.Reset();
        GetComponent<SpriteRenderer>().sprite = underwaterSprite;
        BlockEnemyPathing(true);
        _speedMult = 3f;
        gameObject.layer = LayerMask.NameToLayer("ChefDrumstick");
        droppingWater = false;
    }

    void UnderwaterDisable() {
        underwater = false;
        GetComponent<SpriteRenderer>().sprite = _baseSprite;
        BlockEnemyPathing(false);
        _speedMult = 1f;
        gameObject.layer = LayerMask.NameToLayer("Player");
        droppingWater = true;
    }

    public override void ClassSwitchCleanup() {
        UnderwaterDisable();
        droppingWater = false;
    }

}

