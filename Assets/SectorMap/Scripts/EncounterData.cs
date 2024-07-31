using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OrangeSensei.MapGeneration
{
    [CreateAssetMenu(fileName = "EncounterData", menuName = "SectorMap/EncounterData", order = 1)]
    //Class to add custom instructions for each encounter type!
    public class EncounterData : ScriptableObject
    {
        public NodeType type;
        //Some kimd of data, to store thing like scene? for now a sprite
        public Sprite encounterSprite;
        //public List<EnemyData> EnemyToEncounter;
    }
}
