using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OrangeSensei.Utils;
using UnityEngine;

namespace OrangeSensei.MapGeneration
{
    public enum NodeType{
        None,
        NormalEncounter,
        EliteEncounter,
        Chest,
        RestSpot,
        Event,
        Merchant,
        Boss
    }

    [CreateAssetMenu(fileName = "GenerationData", menuName = "SectorMap/GenerationData", order = 0)]
    public class GenerationData : ScriptableObject
    {

        [Header("Map Data")]
        public int MapFloors = 10; //X axis 
        public int NodesPerFloor = 5; //Y axis
        public float CellSize = 25f; //Size of the one grid cell;

        [Space]
        public int MinStartPoints, MaxStartPoints;

        [Space]
        public int StartingPaths;

        [Header("Node Placement Data")]
        public List<NodeWeightData> NodeWeights;

        [Space(10)]
        public List<CustomWeightedFloor> CustomWeightedFloors;

        [Space(10)]
        public List<NodeRestriction> NodeRestrictions;

        [Header("Encounters Data")]
        public List<EncounterWeights> NormalEncounterChances;
        
        [Space(10)]
        public List<EncounterWeights> EliteEncounterChances;

        [Space(10)]
        public List<EncounterWeights> ChestEncounterChances;

        [Space(10)]
        public List<EncounterWeights> EventEncounterChances;
        
        public List<NodeType> GetNodeTypesList(){
            List<NodeType> nodeTypesList = new List<NodeType>();
            foreach (var item in NodeWeights)
            {
                nodeTypesList.Add(item.type);
            }
            return nodeTypesList;
        }

        public List<float> GetWeightList(){
            List<float> weightList = new List<float>();
            foreach (var item in NodeWeights)
            {
                weightList.Add(item.weight);
            }
            return weightList;
        }

        public NodeRestriction GetNodeRestriction(NodeType type)
        {
            foreach (var restriction in NodeRestrictions)
            {
                if (restriction.TargetType == type)
                {
                    return restriction;
                }
            }
            return null;
        }

        public EncounterData GetEncounterData(NodeType type){

            switch (type){
                case NodeType.NormalEncounter:
                    return GetEncounterData(NormalEncounterChances);

                case NodeType.EliteEncounter:
                    return GetEncounterData(EliteEncounterChances);

                case NodeType.Chest:
                    return GetEncounterData(ChestEncounterChances);

                case NodeType.Event:
                    return GetEncounterData(EventEncounterChances);

                default:
                    return default;
            }
        }

        private EncounterData GetEncounterData(List<EncounterWeights> encounterChances) { 
            return OrangeUtils.GetRandomWeightedItem(encounterChances.Select(x => x.EncounterData).ToList(), encounterChances.Select(x => x.Weight).ToList());
        }

        /// <summary>
        /// Checks if a node type is allowed on the given floor based on restrictions.
        /// </summary>
        public bool IsNodeTypeAllowed(NodeType type, int floorValue)
        {
            NodeRestriction restriction = GetNodeRestriction(type);

            if (restriction != null)
            {
                if (floorValue < restriction.MinFloor)
                {
                    return false;
                }

                if (restriction.DontAppearOnFloor != -1 && floorValue == restriction.DontAppearOnFloor)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [System.Serializable]
    public struct NodeWeightData{
        public NodeType type;
        public float weight;
    }

    [System.Serializable]
    public struct CustomWeightedFloor{
        public int Floor;
        public NodeType TargetType;
        [Range(0, 1)] public float CustomWeight;
    }

    [System.Serializable]
    public class NodeRestriction{
        public NodeType TargetType;
        public int MinFloor = 0; //Minimum floor value to start appearing the nodetype
        public int DontAppearOnFloor = -1; // -1 means no restriction on any specific floor
        public bool AllowConsecutive = false;
    }

    [System.Serializable]
    public class EncounterWeights{
        public EncounterData EncounterData;
        public float Weight;
    }
}
