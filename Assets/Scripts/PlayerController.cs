using UnityEngine;
using System.Collections;
using System;

public class PlayerController : Actor {

    public override Color BaseColor {get; set;}
    public Entities entities;
    public PlayerClass currentClass;
    float movingV;
    float movingH;

    public bool BlockEnemyPathfinding {get; set;}
    bool enablePathfindingOnClassSwitch;

    void Awake() {
        entities = GameObject.Find("GameController").GetComponent<Entities>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Start() {
        // PlayerClass startingClass = gameObject.AddComponent<Mermaid>();
        PlayerClass startingClass = gameObject.AddComponent<PlayerClassChef>();
        currentClass = startingClass;
        SwapClass(startingClass);

        BaseColor = Color.white;
        BaseStart();
    }


    void Update() {
        if (Input.GetAxisRaw("FirePrimary") == 1) {
            currentClass.FirePrimary();
        }

        if (Input.GetAxisRaw("FireSecondary") == 1) {
            currentClass.FireSecondary();
        }

        if (Input.GetAxisRaw("FireTertiary") == 1) {
            currentClass.FireTertiary();
        }

        if (Input.GetAxisRaw("SelectClass1") == 1) {
            var c = gameObject.AddComponent<PlayerClassChef>();
            SwapClass(c);
        }

        if (Input.GetAxisRaw("SelectClass2") == 1) {
            var c = gameObject.AddComponent<PlayerClassMermaid>();
            SwapClass(c);
        }

        movingV = Input.GetAxisRaw("Vertical") ;
        movingH = Input.GetAxisRaw("Horizontal");

        BaseUpdate();
    }

    void FixedUpdate() {
        Move(movingH, movingV);
    }

    void Move(float h, float v) {
        var vmove = transform.up * v;
        var hmove = transform.right * h;
        var combinedMove = vmove.normalized + hmove.normalized;
        GetComponent<Rigidbody2D>().AddForce(combinedMove * currentClass.PlayerSpeed * Time.deltaTime);
    }

    void SwapClass(PlayerClass newClass) {
        currentClass.ClassSwitchCleanup();
        currentClass = newClass;
        GetComponent<SpriteRenderer>().sprite = currentClass.ClassSprite;
        BaseColor = currentClass.ClassColor;
        GetComponent<SpriteRenderer>().color = currentClass.ClassColor;
        if (BlockEnemyPathfinding && enablePathfindingOnClassSwitch) {
            BlockEnemyPathfinding = false;
        }
    }

    public override void BaseUpdate() {
        EntityUpdate();
    }
    public override void BaseStart() {
        EntityStart();
        healthController.Init(6);
    }

    public void SetEnemyPathfinding(bool pathfindingState, bool enableOnClassSwitch) {
        BlockEnemyPathfinding = pathfindingState;
        enablePathfindingOnClassSwitch = enableOnClassSwitch;
    }
}
