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

    int numWaterDropsPerFrame = 1;
    float waterDropLifetime = 4f;

    bool droppingWater;


    const float coolDownBonusForEndingDiveEarly = 0.5f;

    void Awake() {
        entities = GameObject.Find("GameController").GetComponent<Entities>();
        _baseSprite = entities.mermaidSprite;
        _baseColor = ClassColors.mermaidColor;

        primaryCooldownAmt = 0.4f;
        secondaryCooldownAmt = 0.5f;
        tertiaryCooldownAmt = 2f;

        underwaterTimer = new Timer(2f, true);
        underwaterSprite = entities.mermaidUnderwaterSprite;
    }

    void Start() {
        spcRepeatGuard = new Timer(0.2f);
        droppingWater = true;
        CallBaseStart();
    }

    void Update() {
        spcRepeatGuard.Tick(Time.deltaTime);
        underwaterTimer.Tick(Time.deltaTime);
        if (underwaterTimer.Check() && underwater) {
            UnderwaterDisable();
            tertiaryCooldown = new Timer(tertiaryCooldownAmt);
        }
        if (droppingWater) { DropWater(); }


        CallBaseUpdate();
    }

    void DropWater() {
        const float dropRadius = 1.5f;
        const int maxNumDrops = 20;
        int numDropsHere = Physics2D.OverlapCircleAll(transform.position, dropRadius)
                           .Where( c => c.gameObject.tag == "Particle")
                           .Where( c => c.GetComponent<ParticleController>().particleType == ParticleType.mermaidWater)
                           .ToArray()
                           .Length;
        Debug.Log(numDropsHere);
        if (numDropsHere > maxNumDrops) { return; }
        for (var i = 0; i < numWaterDropsPerFrame; i++) {
            var rand = (Vector2) Random.insideUnitCircle * dropRadius;
            var pos = rand + (Vector2) transform.position + (Vector2) transform.TransformDirection(new Vector2(0f, -1f));
            var par = (GameObject) Instantiate(entities.particle, pos, transform.rotation);
            MakeWaterParticle(par);
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

    public override void FireSecondary() {}

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
        // spawn some water particles around me
        SpawnRandomWaterParticles(100, 30, 500);
    }

    void MakeWaterParticle(GameObject par) {
        par.GetComponent<ParticleController>().Init(
            ParticleType.mermaidWater
            , true
            , ClassColors.mermaidColor * new Color(1.5f, 1.5f, 1.5f, 0.4f)
            , waterDropLifetime
            , 4);
    }

    void SpawnRandomWaterParticles(int n, float minForce, float maxForce) {
        for (var i = 0; i < n; i++) {
            var par = (GameObject) Instantiate(entities.particle, transform.position, transform.rotation);
            MakeWaterParticle(par);
            var force = Random.Range(minForce, maxForce);
            par.GetComponent<Rigidbody2D>().drag = 2f;
            par.GetComponent<Rigidbody2D>().AddForce(((Vector3)Random.insideUnitCircle.normalized + transform.up) * force);
        }
    }

    void UnderwaterDisable() {
        underwater = false;
        GetComponent<SpriteRenderer>().sprite = _baseSprite;
        BlockEnemyPathing(false);
        _speedMult = 1f;
        gameObject.layer = LayerMask.NameToLayer("Player");
        // spawn some water particles around me
    }

    public override void ClassSwitchCleanup() {
        droppingWater = false;
        UnderwaterDisable();
    }

}

