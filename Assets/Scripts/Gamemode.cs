using System;
public class Gamemode
{

    public bool gameover = false;
    public byte quarter = 1;
    public float quarterLength;
    public float time;

    public Gamemode(float t_quaterLength)
    {
        quarterLength = t_quaterLength;
        time = t_quaterLength;
    }

    internal void IncrementTime(float deltaTime)
    {
        time -= deltaTime;

        if (time < 0)
        {
            time = quarterLength;
            quarter++;
            if (quarter > 3)
            {
                gameover = true;
            }
        }
    }
}