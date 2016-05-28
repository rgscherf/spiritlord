using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerClassMermaidFish : MonoBehaviour {

    Vector3 target;
    bool flying;
    bool landed;
    Entities entities;

    Rigidbody2D rigid;


    void Awake() {
        GetComponent<SpriteRenderer>().color = ClassColors.mermaidColor;
        rigid = GetComponent<Rigidbody2D>();
        entities = GameObject.Find("GameController").GetComponent<Entities>();
    }

    void Start() {

    }

    void Update() {
        flying = !AtDestination();
        if (flying) {
            rigid.drag = Vector2.Distance(target, transform.position) < 2f ? 10f : 0f ;
        }
        if (!landed && AtDestination()) {
            Land();
        }
    }

    bool AtDestination() {
        return Vector2.Distance(target, transform.position) < 0.3f;
    }

    void Land() {
        rigid.velocity = new Vector3(0f, 0f, 0f);
        flying = false;
        landed = true;
        Explode();
    }

    public void Explode() {
        DropWater(gameObject, 4, true);
    }

    public void Init(Vector3 targ) {
        flying = true;
        landed = false;
        target = targ;
        rigid.drag = 0f;
        rigid.angularDrag = 0f;
        rigid.AddForce((target - transform.position).normalized * 1500);
        rigid.AddTorque(10f);
        Object.Destroy(gameObject, 6f);
    }

    void DropWater(GameObject obj, int radius, bool tweenOutward) {
        var nearestX = (int) Mathf.Round(obj.transform.position.x);
        var nearestY = (int) Mathf.Round(obj.transform.position.y);
        float maxDistance = Vector3.Distance(transform.position, new Vector3(nearestX + radius, nearestY + radius));
        for (var x = -radius; x <= radius; x++) {
            for (var y = -radius; y <= radius; y++) {
                var nearestIntPos = new Vector3(nearestX + x, nearestY + y, 1f);
                var numAdjacentParticles = Physics2D.OverlapCircleAll((Vector2) nearestIntPos, 0.1f)
                                           .Where( c => c.gameObject.tag == "Mermaidwater")
                                           .ToArray()
                                           .Length;
                if ((x != 0 && y != 0) && numAdjacentParticles != 0 ) { continue; }

                GameObject w;
                if (tweenOutward) {
                    w = (GameObject) Instantiate(entities.mermaidWaterSprite, transform.position, Quaternion.identity);
                    const float maxTime = .5f;
                    float fractionalDistance = Vector3.Distance(nearestIntPos, transform.position) / maxDistance;
                    float timeToDestination = maxTime * fractionalDistance;
                    LeanTween.move(w, nearestIntPos, timeToDestination);
                } else {
                    w = (GameObject) Instantiate(entities.mermaidWaterSprite, nearestIntPos, Quaternion.identity);
                }

                w.GetComponent<SpriteRenderer>().color = ClassColors.mermaidColor * new Color(1f, 1f, 1f, 0.5f);
                float waterTime = Random.Range(5.8f, 6.2f);
                Object.Destroy(w, waterTime);
            }
        }
    }



}
