using UnityEngine;

public class PlayerClassChef: PlayerClass {

    // THE CHEF
    // the chef is a slow-moving master of crowd control.
    // primary fire: throw a beer stein, which explodes on its first contact for aoe damage.
    // secondary fire: wind-up a drumstick, which will attract enemies where it lands.
    // tertiary fire: slide out a cauldron of stew, which will explode and push away enemies after a delay.

    public Sprite classSprite;

    public GameObject primaryProjectile;
    public GameObject secondaryProjectile;
    public GameObject tertiaryProjectile;

    Timer primaryCooldown;
    Timer secondaryCooldown;
    Timer tertiaryCooldown;

    const float primaryCooldownAmt = 0.5f;
    const float secondaryCooldownAmt = 2f;
    const float tertiaryCooldownAmt = 2f;

    const float primaryThrowVelocity = 500f;
    const float secondaryDeathTimer = 3.5f;
    const float secondaryThrowVelocity = 200f;


    public float _speed = 1500f;
    public override float PlayerSpeed { get { return _speed; } }


    GameObject MakeProjectile(GameObject proj) {
        return (GameObject) Instantiate(proj, transform.position + transform.up, transform.rotation);
    }

    public void Awake() {
        var entities = GameObject.Find("GameController").GetComponent<Entities>();

        classSprite = entities.chefSprite;

        primaryProjectile = entities.chefFirePrimary;
        secondaryProjectile = entities.chefFireSecondary;
        tertiaryProjectile = entities.chefFireTertiary;

        primaryCooldown = new Timer(primaryCooldownAmt, true);
        secondaryCooldown = new Timer(secondaryCooldownAmt, true);
        tertiaryCooldown = new Timer(tertiaryCooldownAmt, true);
    }

    public void Update() {
        primaryCooldown.Tick(Time.deltaTime);
        secondaryCooldown.Tick(Time.deltaTime);
        tertiaryCooldown.Tick(Time.deltaTime);
    }

    public override void FirePrimary() {
        if (primaryCooldown.Check()) {
            var p = MakeProjectile(primaryProjectile);
            p.GetComponent<Rigidbody2D>().AddForce(transform.up * primaryThrowVelocity);
            p.GetComponent<Rigidbody2D>().AddTorque(20 * (Random.value > 0.5f ? 1f : -1f));
            primaryCooldown.Reset();
        }
    }

    public override void FireSecondary() {
        if (secondaryCooldown.Check()) {
            var p  = (GameObject) Instantiate(secondaryProjectile, transform.position + transform.up, transform.rotation);
            var rb = p.GetComponent<Rigidbody2D>();
            rb.AddTorque(30f);
            rb.AddForce(transform.up * secondaryThrowVelocity);
            Object.Destroy(p, secondaryDeathTimer);
            secondaryCooldown.Reset();
        }
    }

    public override void FireTertiary() {
        if (tertiaryCooldown.Check()) {
            // Instantiate(tertiaryProjectile, gameObject.transform.position + gameObject.transform.up, gameObject.transform.rotation);
            Instantiate(tertiaryProjectile, gameObject.transform.position, gameObject.transform.rotation);
            tertiaryCooldown.Reset();
        }
    }

    public override Sprite ClassSprite {
        get { return classSprite; }
    }

}
