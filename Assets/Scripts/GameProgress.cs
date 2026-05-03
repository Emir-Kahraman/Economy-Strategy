using UnityEngine;

[CreateAssetMenu(fileName = "GameProgress", menuName = "ScriptableObjects/GameProgress")]
public class GameProgress : ScriptableObject
{
    public int lastCompletedLevel;
}
