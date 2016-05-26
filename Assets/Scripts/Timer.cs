public class Timer {
    float timer;
    float timerCurrent;

    public Timer(float t, bool startOffCooldown = false) {
        timer = t;
        timerCurrent = 0f;
        if (startOffCooldown) {
            timerCurrent = timer + 10f;
        }
    }

    public void Tick(float dt) {
        timerCurrent += dt;
    }

    public bool Check() {
        return timerCurrent > timer;
    }

    public float TimeUntilGoal() {
        return timer - timerCurrent;
    }

    public void Reset() {
        timerCurrent = 0f;
    }

    public bool TickCheck(float dt) {
        Tick(dt);
        return Check();
    }

    public float Cooldown() {
        return timer;
    }

    public float CooldownCurrent() {
        return timerCurrent;
    }
}
