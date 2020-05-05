using MLAPI.Serialization;
using MLAPI.Serialization.Pooled;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[Serializable]
public class Team : IBitWritable
{

    /// <summary>
    /// Id of team used to determine if home/away. Home = 0, Away = 1
    /// </summary>
    public int id;
    /// <summary>
    /// An array of the players in their designated position
    /// </summary>
    public ulong[] playersInPosition;
    /// <summary>
    /// Team points;
    /// </summary>
    public int points = 0;
    /// <summary>
    /// Team fouls
    /// </summary>
    public int fouls = 0;

    public Team(int t_id, int t_size)
    {
        id = t_id;
        playersInPosition = new ulong[t_size];
    }

    public void Read(Stream stream)
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            id = reader.ReadInt32Packed();
            playersInPosition = reader.ReadULongArrayPacked();
            points = reader.ReadInt32Packed();
            fouls = reader.ReadInt32Packed();
        }
    }

    public void Write(Stream stream)
    {
        using (PooledBitWriter writer = PooledBitWriter.Get(stream))
        {
            writer.WriteInt32Packed(id);
            writer.WriteULongArrayPacked(playersInPosition);
            writer.WriteInt32Packed(points);
            writer.WriteInt32Packed(fouls);
        }
    }
}