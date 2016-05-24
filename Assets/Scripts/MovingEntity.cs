using UnityEngine;

public abstract class MovingEntity : MonoBehaviour {

    public abstract Color BaseColor {get; set;}
    // this is messy but we need to ensure
    // that all derived classes are calling start and update
    public abstract void BaseUpdate();
    public abstract void BaseStart();

    protected ActorHealth healthController;


    public void EntityStart() {
        healthController = gameObject.AddComponent<ActorHealth>();
        flickerTimer = new Timer(0);
    }

    public void EntityUpdate() {
        FlickerUpdate();
    }

    public void ReceiveDamage(int d) {
        healthController.ReceiveDamage(d);
    }

    /////////////
    // FLICKERING
    // sprites flicker after taking damage (and maybe for other reasons??)
    /////////////

    // flicker-related fields
    int framesSinceLastChange;
    bool rotationOnBase;
    bool flickering;
    Timer flickerTimer;

    public void Flicker(float t) {
        if (!flickering) {
            flickering = true;
            framesSinceLastChange = 0;
            rotationOnBase = true;
            flickerTimer = new Timer(t);
        }
    }

    void FlickerUpdate() {
        flickerTimer.Tick(Time.deltaTime);
        if (flickering) {

            // we want to rotate colors every 2 frames
            // rather than 1, for a mellower flicker effect
            framesSinceLastChange++;
            if (framesSinceLastChange == 2) {
                framesSinceLastChange = 0;
                rotationOnBase = !rotationOnBase;
                GetComponent<SpriteRenderer>().color = rotationOnBase ? BaseColor : Color.black;
            }

            if (flickerTimer.Check()) {
                FlickerUnload();
            }
        }
    }

    void FlickerUnload() {
        flickering = false;
        GetComponent<SpriteRenderer>().color = BaseColor;
    }


}
