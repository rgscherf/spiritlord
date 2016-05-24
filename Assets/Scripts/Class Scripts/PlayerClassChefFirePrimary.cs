using UnityEngine;
using System.Collections;

public class PlayerClassChefFirePrimary : MonoBehaviour {

    Entities entities;

    // float coneLength = 5f;
    // float coneWidth = 3f;
    const float puddleDiameter = 1.5f;

    void Start() {
        entities = GameObject.Find("GameController").GetComponent<Entities>();
    }

    void Explode(Vector2 other) {
        DamageEnemies();
        const int numParticles = 200;
        for (int i = 0; i < numParticles; i++) {
            Vector3 rigid = GetComponent<Rigidbody2D>().velocity.normalized;
            var r = (GameObject) Instantiate(entities.particle, transform.position + rigid, Quaternion.identity);
            r.transform.position += (Vector3) Random.insideUnitCircle * puddleDiameter;
            var rp = r.GetComponent<ParticleController>();
            rp.Init( ParticleController.ParticleType.effects
                     , true
                     , Color.white
                     , Random.Range(0.12f, 0.4f)
                     , (Random.value > 0.5f ? 1 : 2) );
        }
        Object.Destroy(gameObject);
    }

    void DamageEnemies() {
        var coll = Physics2D.OverlapCircleAll(transform.position, puddleDiameter);
        foreach (var c in coll) {
            if (c.gameObject.tag == "Enemy") {
                c.gameObject.GetComponent<MovingEntity>().ReceiveDamage(1);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Player") { return; }
        Explode(other.contacts[0].point);
    }
}
