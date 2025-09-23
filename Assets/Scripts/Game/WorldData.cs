using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New World Data", menuName = "Game Data/World Data")]
public class WorldData : ScriptableObject
{
    public string worldName;

    [Tooltip("La lista ordinata dei settori che compongono questo mondo.")]
    public List<SectorData> sectors;
}