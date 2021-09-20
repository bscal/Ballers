using MLAPI.Serialization;
using UnityEngine;

public struct PlayerProperties : INetworkSerializable
{
    public int slot;
    public int teamID;
    public bool isAI;
    public ulong steamID;
    public Vector3 target;
    public bool isRightHanded;
    public bool isMoving;
    public bool isSprinting;
    public bool isScreening;
    public bool isHardScreening;
    public bool isShooting;
    public bool isHelping;
    public bool isBallInLeftHand;
    public bool isCtrlDown;
    public bool isAltDown;
    public bool movingFoward;
    public bool movingBack;
    public bool movingLeft;
    public bool movingRight;
    public bool isContesting;
    public bool isBlocking;
    public bool isStealing;
    public bool isDribbling;
    public bool isInsideThree;
    public bool isInbounds;
    public bool isPostShot;
    public bool isPostMove;

    public void NetworkSerialize(NetworkSerializer serializer)
    {
        serializer.Serialize(ref slot);
        serializer.Serialize(ref steamID);
        serializer.Serialize(ref teamID);
        serializer.Serialize(ref isAI);
        serializer.Serialize(ref target);
        serializer.Serialize(ref isRightHanded);
        //serializer.Serialize(ref isMoving);
        serializer.Serialize(ref isSprinting);
        serializer.Serialize(ref isScreening);
        serializer.Serialize(ref isHardScreening);
        serializer.Serialize(ref isShooting);
        serializer.Serialize(ref isHelping);
        serializer.Serialize(ref isBallInLeftHand);
        serializer.Serialize(ref isCtrlDown);
        serializer.Serialize(ref isAltDown);
        serializer.Serialize(ref movingFoward);
        serializer.Serialize(ref movingBack);
        serializer.Serialize(ref movingLeft);
        serializer.Serialize(ref movingRight);
        serializer.Serialize(ref isContesting);
        serializer.Serialize(ref isBlocking);
        serializer.Serialize(ref isStealing);
        serializer.Serialize(ref isDribbling);
        serializer.Serialize(ref isInsideThree);
        serializer.Serialize(ref isInbounds);
        serializer.Serialize(ref isPostShot);
        serializer.Serialize(ref isPostMove);
    }
}
