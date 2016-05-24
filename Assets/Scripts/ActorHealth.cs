using UnityEngine;
using System.Collections;

public class ActorHealth : MonoBehaviour {

    public int currentHealth;
    public int maxHealth;

    Timer invulnTimer;
    GameObject go;

    void Start() {
        go = gameObject;
    }

    public void Init(int hp, float invuln = 0.8f) {
        maxHealth = currentHealth = hp;
        invulnTimer = new Timer(invuln, true);
    }

    void Update() {
        invulnTimer.Tick(Time.deltaTime);
    }

    public void ReceiveDamage(int debitamount) {
        if (invulnTimer.Check()) {
            currentHealth -= debitamount;
            invulnTimer.Reset();
            go.GetComponent<Actor>().Flicker(invulnTimer.Cooldown());
        }
        if (currentHealth <= 0) {
            Object.Destroy(go);
        }
    }
}
