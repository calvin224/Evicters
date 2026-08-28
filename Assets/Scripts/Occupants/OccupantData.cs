using UnityEngine;

[CreateAssetMenu(
    fileName = "OccupantData",
    menuName = "Game/Occupant Data"
)]
public class OccupantData : ScriptableObject
{
    [Header("Identity")]
    public string occupantName;

    [Header("Detection")]
    public float detectionRange = 5f;

    [Header("Wandering")]
    public float wanderRadius = 5f;
    public float wanderInterval = 4f;

    [Header("Anger")]
    public float angerDuration = 3f;

    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string[] knockDialogue;

    [TextArea(2, 5)]
    public string[] pushDialogue;
}