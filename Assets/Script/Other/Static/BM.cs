using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BM : MonoBehaviour
{
    public static Vector2Int Vec3To2int(Vector3 _vec)
    {
        return new Vector2Int(Mathf.FloorToInt(_vec.x), Mathf.FloorToInt(_vec.z));
    }
    public static Vector2 Vec3To2(Vector3 _vec)
    {
        return new Vector2(Mathf.FloorToInt(_vec.x), Mathf.FloorToInt(_vec.z));
    }
}
