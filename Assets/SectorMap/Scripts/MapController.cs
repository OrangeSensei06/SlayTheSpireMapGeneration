using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OrangeSensei.MapGeneration
{
    public class MapController : MonoBehaviour
    {
        public List<MapNode> availableNodes = new List<MapNode>();
        
        public void ActivateNodes(){
            foreach(MapNode node in availableNodes){
                node.ActivateNode();
            }
        }

        public void UpdateNodes(List<MapNode> nodes){
            foreach (var node in availableNodes)
            {
                node.DeactivateNode();
            }

            availableNodes = nodes;

            ActivateNodes();
        }
    }
}
