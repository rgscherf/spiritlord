using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public Transform playerTransform;
    // Use this for initialization
    void Start () {

    }

    // Update is called once per frame
    void Update () {
        transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, -10f);
        transform.rotation = playerTransform.rotation;

    }
}
