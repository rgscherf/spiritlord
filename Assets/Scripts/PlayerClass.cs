using UnityEngine;

public abstract class PlayerClass : MonoBehaviour {
    public abstract float PlayerSpeed {get;}
    public abstract Sprite ClassSprite {get;}
    public abstract Color ClassColor {get;}
    public abstract void FirePrimary();
    public abstract void FireSecondary();
    public abstract void FireTertiary();
    public abstract void CallBaseStart(); // call this at END of Start()
    public abstract void CallBaseUpdate(); // call this at BEGINNING of Update()
    public abstract void ClassSwitchCleanup();

    protected Timer primaryCooldown;
    protected Timer secondaryCooldown;
    protected Timer tertiaryCooldown;

    protected float primaryCooldownAmt;
    protected float secondaryCooldownAmt;
    protected float tertiaryCooldownAmt;

    public void BaseStart() {
        primaryCooldown = new Timer(primaryCooldownAmt, true);
        secondaryCooldown = new Timer(secondaryCooldownAmt, true);
        tertiaryCooldown = new Timer(tertiaryCooldownAmt, true);
    }

    public void BaseUpdate() {
        primaryCooldown.Tick(Time.deltaTime);
        secondaryCooldown.Tick(Time.deltaTime);
        tertiaryCooldown.Tick(Time.deltaTime);
    }

    protected static void BlockEnemyPathing(bool v) {
        GameObject.Find("Player").GetComponent<PlayerController>().SetEnemyPathfinding(v, true);
    }
}
