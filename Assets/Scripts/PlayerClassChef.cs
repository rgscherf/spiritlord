using UnityEngine;

public class PlayerClassChef: PlayerClass {

    // CHEF
    // chef is a slow-moving master of crowd control.
    // primary fire: throw a beer stein, which explodes on its first contact for aoe damage.
    // secondary fire: wind-up a drumstick, which will attract enemies where it lands.
    // tertiary fire: slide out a cauldron of stew, which will explode and push away enemies after a delay.

    Color _baseColor;
    Sprite _baseSprite;

    public GameObject primaryProjectile;
    public GameObject secondaryProjectile;
    public GameObject tertiaryProjectile;

    const float primaryThrowVelocity = 450f;
    const float secondaryDeathTimer = 2.5f;
    const float secondaryThrowVelocity = 200f;

    public override float PlayerSpeed { get { return 1200f; } }
    public override Sprite ClassSprite {get { return _baseSprite; }}
    public override Color ClassColor {get { return _baseColor; }}
    public override void CallBaseStart() { BaseStart(); }
    public override void CallBaseUpdate() { BaseUpdate(); }

    GameObject MakeProjectile(GameObject proj) {
        return (GameObject) Instantiate(proj, transform.position + transform.up, transform.rotation);
    }

    void Awake() {
        var entities = GameObject.Find("GameController").GetComponent<Entities>();
        _baseSprite = entities.chefSprite;

        _baseColor = ClassColors.chefColor;

        primaryProjectile = entities.chefFirePrimary;
        secondaryProjectile = entities.chefFireSecondary;
        tertiaryProjectile = entities.chefFireTertiary;

        primaryCooldownAmt = 0.75f;
        secondaryCooldownAmt = 2f;
        tertiaryCooldownAmt = 2f;
    }

    void Start() {
        CallBaseStart();
    }

    void Update() {
        CallBaseUpdate();
    }

    public override void FirePrimary() {
        if (primaryCooldown.Check()) {
            var p = MakeProjectile(primaryProjectile);
            ChangeSpriteColor(p);
            p.GetComponent<Rigidbody2D>().AddForce(transform.up * primaryThrowVelocity);
            p.GetComponent<Rigidbody2D>().AddTorque(10 * (Random.value > 0.5f ? 1f : -1f));
            primaryCooldown.Reset();
        }
    }

    public override void FireSecondary() {
        if (secondaryCooldown.Check()) {
            var p  = (GameObject) Instantiate(secondaryProjectile, transform.position + transform.up, transform.rotation);
            ChangeSpriteColor(p);
            var rb = p.GetComponent<Rigidbody2D>();
            rb.AddTorque(30f);
            rb.AddForce(transform.up * secondaryThrowVelocity);
            Object.Destroy(p, secondaryDeathTimer);
            secondaryCooldown.Reset();
        }
    }

    public override void FireTertiary() {
        if (tertiaryCooldown.Check()) {
            var p = (GameObject) Instantiate(tertiaryProjectile, gameObject.transform.position, gameObject.transform.rotation);
            ChangeSpriteColor(p);
            tertiaryCooldown.Reset();
        }
    }

    void ChangeSpriteColor(GameObject go) {
        go.GetComponent<SpriteRenderer>().color = _baseColor;
    }

}
