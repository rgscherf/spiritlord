using UnityEngine;
using System.Collections;

public class PlayerClassMermaidTrident : MonoBehaviour {

    Transform par;

    const float tweenTime = 0.2f;

    // Use this for initialization
    void Start () {
        par = transform.parent;
        LeanTween.value(gameObject, ThrustIncrement, 0.6f, 1.5f, tweenTime).setEase(LeanTweenType.easeOutQuart);
        Object.Destroy(gameObject, tweenTime + 0.3f);
    }

    void ThrustIncrement(float y) {
        transform.position = par.position + par.TransformDirection(new Vector3(0.5f, y, 0f));
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Enemy") {
            other.gameObject.GetComponent<Actor>().ReceiveDamage(2);
        }
    }
}
