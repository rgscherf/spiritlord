using UnityEngine;
using System.Collections;
using System.Linq;

public class EnemyBasicController : Actor {

    public override Color BaseColor {get; set;}

    PolyNavAgent agent;

    float detectionDistance = 30f;
    float drumstickLureDistance = 7f;

    bool pathfinding;
    public Transform targetTransform;

    Timer attackTimer;
    bool attacking;
    Timer attackCooldown;

    void Start () {
        targetTransform = GameObject.Find("Player").transform;

        BaseColor = Color.white;

        BaseStart();
        agent = GetComponent<PolyNavAgent>();
        agent.OnDestinationReached += Attack;
        attackCooldown = new Timer(5f, true);
    }

    void Update ( ) {
        transform.rotation = targetTransform.rotation;

        BaseUpdate();
    }

    void FixedUpdate() {
        if (!pathfinding) {
            CheckPathfindingActivation();
            return;
        }

        if (attacking) {
            agent.Stop();
            attackCooldown.Check();
            AttackStep();
        } else {
            attackCooldown.Tick(Time.deltaTime);
        }

        if (targetTransform.gameObject.GetComponent<PlayerController>().BlockEnemyPathfinding) {
            agent.Stop();
            return;
        }
        var targetPos = GetTarget();
        agent.SetDestination(targetPos);

    }

    void DeterminePathfindingAction(PathfindingAction act) {
        // pathfinding actions are cues to AI objects.
        // Blockpathfinding -> don't move
        // Normal -> seek target
        // Lure -> seek drumstick
    }

    Vector2 GetTarget() {
        // this is where we mess with enemy pathfinding.
        // will definitely have to expand these statements as we add weapon effects.
        var ds = GameObject.FindGameObjectsWithTag("PlayerDrumstick")
                 .Where( d => Vector2.Distance(d.transform.position, transform.position) < drumstickLureDistance)
                 .OrderBy( d => Vector2.Distance(d.transform.position, transform.position)).ToArray();
        return ds.Length == 0 ? targetTransform.position : ds[0].transform.position;
    }

    void CheckPathfindingActivation() {
        if (!pathfinding) {
            var dist = Vector2.Distance(targetTransform.position, transform.position);
            if (dist < detectionDistance) {
                // raycast blocks on other enemies, so watch line of sight during placement.
                // could not figure out layermasks. try later! (2016-05-23)
                var mask = LayerMask.GetMask("Geometry", "Player");
                var hit = Physics2D.Raycast(transform.position, targetTransform.position - transform.position, Mathf.Infinity, mask);
                if (hit.collider != null && hit.collider.tag == "Player") {
                    pathfinding = true;
                }
            }
        }
    }

    void AttackStep() {
        attackTimer.Tick(Time.deltaTime);
        var check = attackTimer.Check();
        if (check) {
            attacking = false;
            GetComponent<Rigidbody2D>().AddForce((targetTransform.position - transform.position).normalized * 5000f);
            attackCooldown.Reset();
        } else {
            transform.position += ((Vector3)Random.insideUnitCircle.normalized * 0.15f);
        }

    }

    void Attack() {
        if (!attacking && attackCooldown.Check()) {
            attacking = true;
            attackTimer = new Timer (1.5f);
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
