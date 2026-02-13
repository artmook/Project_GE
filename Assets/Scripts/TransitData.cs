using System;
using UnityEngine;

[Serializable]
public struct TransitData
{
    [Tooltip("이동할 씬의 이름")]
    public string sceneToMove;
    
    [Tooltip("해당 씬에서 스폰될 지점의 이름")]
    public string destinationObj;
}
