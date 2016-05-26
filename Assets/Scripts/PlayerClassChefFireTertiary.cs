using UnityEngine;
using System.Collections;

public class PlayerClassChefFireTertiary : MonoBehaviour {

    Entities entities;

    Vector2 originalPos;
    Timer deathTimer;

    SpriteRenderer spr;
    SpriteRenderer rangeIndicatorSpr;

    float explodeForce = 6000f;
    float explodeRadius = 3.25f;
    int explosionParticles = 20;

    float deathTimerValue = 0.75f;

    bool hasPushed;

    void Start () {
        entities = GameObject.Find("GameController").GetComponent<Entities>();

        // float the projectile in front of player
        // var playerpos = GameObject.Find("Player").transform.position;
        // var floatDir = transform.position - playerpos;
        // gameObject.GetComponent<Rigidbody2D>().AddForce(floatDir.normalized * 100f);

        // add some rotation for visual flair
        var rotationDir = Random.value > 0.5f ? -1 : 1;
        gameObject.GetComponent<Rigidbody2D>().AddTorque(40f * rotationDir);

        // set a death timer;
        deathTimer = new Timer(deathTimerValue);

        spr = GetComponent<SpriteRenderer>();
        rangeIndicatorSpr = transform.Find("Range Indicator").GetComponent<SpriteRenderer>();
        rangeIndicatorSpr.color = spr.color * new Color(1, 1, 1, 0.3f);
    }

    void Update () {
        deathTimer.Tick(Time.deltaTime);

        if (deathTimer.Check()) {
            Explode();
            if (deathTimer.TimeUntilGoal() <= -0.1f) {
                Object.Destroy(gameObject);
            }
        }
    }

    void Explode() {
        if (!hasPushed) {
            // to prevent once-per-explosion things from happening multiple times
            var colls = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
            foreach (var c in colls) {
                if (c.tag == "Enemy" || c.tag == "Player") {
                    var xplvector = Entities.OutwardExplosionVector(transform.position, c.transform.position, explodeForce);
                    c.gameObject.GetComponent<Rigidbody2D>().AddForce(xplvector);
                }
            }
            spr.enabled = false;
            hasPushed = true;
        }

        // by mistake, I had this run every frame until the timer.TimeFromGoal was exceeded.
        // the "fireworks" effects was nice, so I kept it.
        // again this will run EVERY FRAME until the object is destroyed.
        for (int i = 0; i < explosionParticles; i++) {
            var r = (GameObject) Instantiate(entities.particle, transform.position, Quaternion.identity);
            var rp = r.GetComponent<ParticleController>();
            rp.Init( ParticleType.effects
                     , true
                     , spr.color
                     , 0.5f
                     , 2 );
            var xplvector = Entities.OutwardExplosionVector(transform.position, (Random.insideUnitCircle * 5) + (Vector2) transform.position, 450f);
            rp.AddForce(xplvector);
            rp.AddTorque(100f);
        }
    }
}
