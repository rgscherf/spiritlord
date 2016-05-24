using UnityEngine;
using System.Collections;
using System;

public class PlayerController : Actor {

    public override Color BaseColor {get; set;}
    public Entities entities;
    PlayerClass currentClass;
    float playerSpeed;
    float movingV;
    float movingH;

    void Awake() {
        entities = GameObject.Find("GameController").GetComponent<Entities>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }

    void Start() {
        PlayerClass startingClass = gameObject.AddComponent<PlayerClassChef>();
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
        GetComponent<Rigidbody2D>().AddForce(combinedMove * playerSpeed * Time.deltaTime);
    }

    void SwapClass(PlayerClass newClass) {
        currentClass = newClass;
        playerSpeed = currentClass.PlayerSpeed;
        GetComponent<SpriteRenderer>().sprite = currentClass.ClassSprite;
        BaseColor = currentClass.ClassColor;
        GetComponent<SpriteRenderer>().color = currentClass.ClassColor;
    }

    public override void BaseUpdate() {
        EntityUpdate();
    }
    public override void BaseStart() {
        EntityStart();
        healthController.Init(6);
    }
}
