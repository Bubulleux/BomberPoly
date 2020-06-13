using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameMode
{
    //RoomInfoClass room { get; }
    void Update();
    void PlyDie(int _ply);
}
public class Classic : IGameMode
{

}
