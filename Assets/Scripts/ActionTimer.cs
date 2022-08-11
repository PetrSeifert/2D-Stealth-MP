using System;

public class ActionTimer
{
    private Action timerCallback;
    private float timer;
    private bool finished;

    public ActionTimer(Action timerCallback, float timer)
    {
        this.timer = timer;
        this.timerCallback = timerCallback;
    }

    public void Tick(float deltaTime)
    {
        timer -= deltaTime;
        if (!(timer <= 0) || finished) return;
        timerCallback();
        finished = true;
    }
}
