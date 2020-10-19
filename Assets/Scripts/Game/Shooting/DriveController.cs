using UnityEngine;
using System.Collections;
using MLAPI;

public enum DriveDir
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

public class DriveController : NetworkedBehaviour
{
    /*
     * TODO
     * Some type of "directional" or counter play moves for defender and driver
     * Maybe some type of up, left, right with a extra button?
     * 
     * Players positions relative to you. What the defender doing (stealing, contesting)
     * effects the difficulty of the shot.
     */

    public bool StartDrive()
    {
        return false;
    }

    public bool UpdateDrive()
    {
        return false;
    }

    public bool FinishDrive()
    {
        return false;
    }
}