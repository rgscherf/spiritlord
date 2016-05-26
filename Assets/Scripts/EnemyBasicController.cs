using UnityEngine;
using System.Collections;
using System.Linq;

public class EnemyBasicController : Actor {

    public override Color BaseColor {get; set;}

    float detectionDistance = 30f;
    float drumstickLureDistance = 7f;

    bool pathfinding;
    public Transform playerTransform;

    void Start () {
        playerTransform = GameObject.Find("Player").transform;

        BaseColor = Color.white;

        BaseStart();
    }

    void Update ( ) {
        transform.rotation = playerTransform.rotation;

        BaseUpdate();
    }

    void FixedUpdate() {
        if (!pathfinding) {
            CheckPathfindingActivation();
            return;
        }
        if (playerTransform.gameObject.GetComponent<PlayerController>().BlockEnemyPathfinding) {
            GetComponent<PolyNavAgent>().Stop();
            return;
        }
        var targetPos = GetTarget();
        GetComponent<PolyNavAgent>().SetDestination(targetPos);
    }

    Vector2 GetTarget() {
        // this is where we mess with enemy pathfinding.
        // will definitely have to expand these statements as we add weapon effects.
        var ds = GameObject.FindGameObjectsWithTag("PlayerDrumstick")
                 .Where( d => Vector2.Distance(d.transform.position, transform.position) < drumstickLureDistance)
                 .OrderBy( d => Vector2.Distance(d.transform.position, transform.position)).ToArray();
        return ds.Length == 0 ? playerTransform.position : ds[0].transform.position;
    }

    void CheckPathfindingActivation() {
        if (!pathfinding) {
            var dist = Vector2.Distance(playerTransform.position, transform.position);
            if (dist < detectionDistance) {
                // raycast blocks on other enemies, so watch line of sight during placement.
                // could not figure out layermasks. try later! (2016-05-23)
                var mask = LayerMask.GetMask("Geometry", "Player");
                var hit = Physics2D.Raycast(transform.position, playerTransform.position - transform.position, Mathf.Infinity, mask);
                if (hit.collider != null && hit.collider.tag == "Player") {
                    pathfinding = true;
                }
            }
        }
    }

    public override void BaseUpdate() {
        base.EntityUpdate();
    }

    public override void BaseStart() {
        base.EntityStart();
        base.healthController.Init(3);
    }
}
