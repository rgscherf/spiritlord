﻿using System.Linq;
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

    Timer underwaterTimer;
    bool underwater;
    Sprite underwaterSprite;
    Timer spcRepeatGuard;

    bool droppingWater;

    const float coolDownBonusForEndingDiveEarly = 0.5f;

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

        if (droppingWater) { DropWater(gameObject, 1); }

        if (secondaryFireStage == SecondaryStage.windingFish && Input.GetAxisRaw("FireSecondary") == 0f) {
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
                float waterTime = Random.Range(5.8f, 6.2f);
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


    enum SecondaryStage {notWinding, windingFish, releasedFish};

    void ReleaseFish() {
        // spawn flying fish object
        Vector2 init = (transform.position + transform.TransformDirection(0f, 1f, 0f));
        var currentFishReleased = (GameObject) Instantiate(entities.mermaidFishFlying, init, transform.rotation);
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
        if (secondaryCooldown.Check()) {
            switch (secondaryFireStage) {
                case SecondaryStage.notWinding:
                    currentFishRangefinder = (GameObject) Instantiate(entities.mermaidFish, transform.position + transform.TransformDirection(new Vector3(0f, 1f, 0f)), transform.rotation);
                    fishDistance = 1;
                    secondaryFireStage = SecondaryStage.windingFish;
                    FishFlickerInit();
                    break;
                case SecondaryStage.windingFish:
                    currentFishRangefinder = (GameObject) Instantiate(entities.mermaidFish, transform.position + transform.TransformDirection(new Vector3(0f, 1f, 0f)), transform.rotation);
                    fishDistance = 1;
                    FishFlickerInit();
                    break;
                case SecondaryStage.releasedFish:
                    MoveToFish();
                    secondaryCooldown.Reset();
                    break;
            } else {
                secondaryFireStage = SecondaryStage.notWinding;
            }
            // if (!secondaryWinding) {
            //     currentFishRangefinder = (GameObject) Instantiate(entities.mermaidFish, transform.position + transform.TransformDirection(new Vector3(0f, 1f, 0f)), transform.rotation);
            //     fishDistance = 1;
            //     secondaryWinding = true;
            //     FishFlickerInit();
            // } else {
            //     // using mouse movement to move fish up/down from player transform
            //     var mousey = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * 45;
            //     fishDistance = Mathf.Clamp(mousey + fishDistance, 1f, 20f);
            //     currentFishRangefinder.transform.position = transform.position + transform.TransformDirection(new Vector3(0f, fishDistance, 0f));
            //     currentFishRangefinder.transform.rotation = transform.rotation;
            //     FishFlickerUpdate();
            // }
        }
    }

    void MoveToFish()

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

