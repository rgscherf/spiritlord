using System.Linq;
using UnityEngine;
using System.Collections;

public class PlayerClassMermaid : PlayerClass {
    // MERMAID
    // mermaid is a powerful hit-and-run attacker
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
    float _baseSpeed = 2300f;
    float _speedMult = 1f;

    public override float PlayerSpeed { get { return _baseSpeed * _speedMult; } }
    public override Sprite ClassSprite {get { return _baseSprite; }}
    public override Color ClassColor {get { return _baseColor; }}
    public override void CallBaseStart() { BaseStart(); }
    public override void CallBaseUpdate() { BaseUpdate(); }

    bool hasReleasedPrimary;

    Timer underwaterTimer;
    bool underwater;
    Sprite underwaterSprite;
    Timer spcRepeatGuard;

    bool droppingWater;

    const float coolDownBonusForEndingDiveEarly = 0.5f;

    // controller enum for secondary fire stage
    // depending on this value, we will either be doing nothing,
    // displaying a fish rangefinder,
    // be waiting for the player to move to fish,
    // or be moving toward the fish.
    enum SecondaryStage {notWinding, windingFish, releasedFish, movingToFish};

    // fish rangefinder flickering controls
    int fishFramesSinceLastChange;
    bool fishRotationBase;

    // fish rangefinder
    bool secondaryWinding;
    float fishDistance;
    GameObject currentFishRangefinder;
    Vector2 currentFishTarget;

    // fish on the ground
    bool fishReleased;
    GameObject currentFishReleased;

    void Awake() {
        entities = GameObject.Find("GameController").GetComponent<Entities>();
        _baseSprite = entities.mermaidSprite;
        _baseColor = ClassColors.mermaidColor;

        primaryCooldownAmt = 1f;
        secondaryCooldownAmt = 2f;
        tertiaryCooldownAmt = 2f;

        underwaterTimer = new Timer(1.5f, true);
        underwaterSprite = entities.mermaidUnderwaterSprite;
    }

    void Start() {
        BlockEnemyPathing(false);
        spcRepeatGuard = new Timer(0.2f);
        droppingWater = true;
        CallBaseStart();
        secondaryFireStage = SecondaryStage.notWinding;
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

        if (droppingWater) { DropWater(gameObject, 1); }

        if (secondaryFireStage == SecondaryStage.windingFish && Input.GetAxisRaw("FireSecondary") == 0f) {
            ReleaseFish();
        }

        // released fish handles its own death timer.
        // in order to reset our cooldown, we have to check whether:
        // 1. the fish has expired on its own
        // 2. we are close enough to explode the fish
        if ((secondaryFireStage == SecondaryStage.movingToFish ||
                secondaryFireStage == SecondaryStage.releasedFish) &&
                currentFishReleased == null) {
            ResetSecondaryCooldown();
        }
        if (secondaryFireStage == SecondaryStage.movingToFish) {
            if (currentFishReleased != null &&
                    Vector3.Distance(transform.position, currentFishReleased.transform.position) < 0.3f) {
                currentFishReleased.GetComponent<PlayerClassMermaidFish>().Explode(true);
                ResetSecondaryCooldown();
            }
        }

        CallBaseUpdate();
    }

    bool NotTouchingWater() {
        var w = Physics2D.OverlapCircleAll(transform.position, 1f)
                .Where( c => c.gameObject.tag == "Mermaidwater")
                .ToArray()
                .Length;
        return w == 0;
    }

    public void DropWater(GameObject obj, int radius) {
        var nearestX = (int) Mathf.Round(obj.transform.position.x);
        var nearestY = (int) Mathf.Round(obj.transform.position.y);
        for (var x = -radius; x <= radius; x++) {
            for (var y = -radius; y <= radius; y++) {
                var nearestIntPos = new Vector3(nearestX + x, nearestY + y, 1f);
                var p = Physics2D.OverlapCircleAll((Vector2) nearestIntPos, 0.1f)
                        .Where( c => c.gameObject.tag == "Mermaidwater")
                        .ToArray()
                        .Length;
                if (p != 0 ) { continue; }
                var w = (GameObject) Instantiate(entities.mermaidWaterSprite, nearestIntPos, Quaternion.identity);
                w.GetComponent<SpriteRenderer>().color = ClassColors.mermaidColor * new Color(1f, 1f, 1f, 0.5f);
                float waterTime = Random.Range(6f, 6.2f);
                Object.Destroy(w, waterTime);
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

    void ReleaseFish() {
        // spawn flying fish object
        Vector2 init = (transform.position + transform.TransformDirection(0f, 1f, 0f));
        currentFishReleased = (GameObject) Instantiate(entities.mermaidFishFlying, init, transform.rotation);
        currentFishTarget = currentFishRangefinder.transform.position;
        currentFishReleased.GetComponent<PlayerClassMermaidFish>().Init(currentFishTarget);

        // destroy fish targeter
        Object.Destroy(currentFishRangefinder);

        // reset cooldowns
        secondaryFireStage = SecondaryStage.releasedFish;

        // flag moving to released mode
        fishReleased = true;
    }

    SecondaryStage secondaryFireStage;

    public override void FireSecondary() {
        // we are cheating the contract by checking for fire release in Update().
        if (!underwater && secondaryCooldown.Check()) {
            switch (secondaryFireStage) {
                case SecondaryStage.notWinding:
                    currentFishRangefinder = (GameObject) Instantiate(entities.mermaidFish, transform.position + transform.TransformDirection(new Vector3(0f, 1f, 0f)), transform.rotation);
                    fishDistance = 1;
                    secondaryFireStage = SecondaryStage.windingFish;
                    FishFlickerInit();
                    break;
                case SecondaryStage.windingFish:
                    // using mouse movement to move fish up/down from player transform
                    var mousey = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 45;
                    fishDistance = Mathf.Clamp(mousey + fishDistance, 1f, 20f);
                    currentFishRangefinder.transform.position = transform.position + transform.TransformDirection(new Vector3(0f, fishDistance, 0f));
                    currentFishRangefinder.transform.rotation = transform.rotation;
                    FishFlickerUpdate();
                    break;
                case SecondaryStage.releasedFish:
                    MoveToFish();
                    break;
            }
        }
    }

    void ResetSecondaryCooldown() {
        secondaryFireStage = SecondaryStage.notWinding;
        secondaryCooldown.Reset();
    }

    void MoveToFish() {
        var mask = LayerMask.GetMask("Geometry", "ChefDrumstick");
        var hit = Physics2D.Raycast(transform.position, currentFishReleased.transform.position - transform.position, Mathf.Infinity, mask);
        if (hit.collider != null && hit.collider.tag == "MermaidFish") {
            var player = GameObject.Find("Player");
            if (currentFishReleased == null) { return; }
            var target = currentFishReleased.transform.position;
            const float maxTime = .5f;
            float fractionalDistance = Vector3.Distance(target, transform.position) / 30;
            float timeToDestination = maxTime * fractionalDistance;
            LeanTween.move(player, target, timeToDestination).setEase(LeanTweenType.easeInQuint);
            secondaryFireStage = SecondaryStage.movingToFish;
        }
    }

    public void FishFlickerInit() {
        fishFramesSinceLastChange = 0;
        fishRotationBase = true;
    }

    void FishFlickerDispose() {
        fishFramesSinceLastChange = 0;
        fishRotationBase = false;
    }

    void FishFlickerUpdate() {
        // we want to rotate colors every 2 frames
        // rather than 1, for a mellower flicker effect
        fishFramesSinceLastChange++;
        if (fishFramesSinceLastChange == 2) {
            fishFramesSinceLastChange = 0;
            fishRotationBase = !fishRotationBase;
            currentFishRangefinder.GetComponent<SpriteRenderer>().color = fishRotationBase ? ClassColors.mermaidColor : Color.black;
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

