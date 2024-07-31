using System.Collections.Generic;
using UnityEngine;
using OrangeSensei.Utils;

namespace OrangeSensei.MapGeneration
{
    //Gameplay Node class that handles all the functions like changing spirte, handling the event triggering and other stuff!
    public class MapNode : MonoBehaviour
    {
        public Node myNodeData;
        public GameObject pathPrefab;
        public Color notInteractableColor = Color.grey;
        

        private bool canSelect = false;
        private readonly List<MapNode> connectingNodes = new List<MapNode>(); //List to get all the connected nodes for movement!
        private NodeType type;
        private EncounterData encounterData; //Data to know which enocunter to trigger!
        private MapGeneration mapGeneration;

        // Activates the node, allowing it to be selected and changing its appearance!    

        public void Initialize(Node myNode, NodeType type)
        {
            this.type = type;
            myNodeData = myNode;
            mapGeneration = MapGeneration.Instance;
            connectingNodes.Clear();
            SetSprite(type, notInteractableColor);
        }

        public void ActivateNode()
        {
            canSelect = true;
            SetSpriteColor(Color.white);
            if(type != NodeType.Boss) GetComponent<Animator>().Play("Activate");
        }

        // Deactives the OnMouseUP() function
        public void DeactivateNode()
        {
            canSelect = false;
            SetSpriteColor(notInteractableColor);
            if(type != NodeType.Boss) GetComponent<Animator>().Play("Idle");
        }

        public void AddEncounter(EncounterData encounterData)
        {
            if(encounterData == null) return;
            this.encounterData = encounterData;
            if(encounterData.encounterSprite != null){
                TryGetComponent<SpriteRenderer>(out SpriteRenderer sr);
                sr.sprite = encounterData.encounterSprite;
            }
        }

        //Handles the interaction of the map node! //! This is where you would want to trigger the encounter event and wait for it to finish and update the MapController!
        private void OnMouseUp()
        {
            if (canSelect)
            {
                //Trigger the encounter based on nodeType
                FindObjectOfType<MapController>().UpdateNodes(connectingNodes);
            }
        }

        // Creates a path to another node and ensures different node types. (Spawns the Prefab asssigned to it!)
        public void MakePath(Node connectingNode)
        {
            if (connectingNodes.Contains(connectingNode.GetMapNode()))
            {
                return;
            }

            LineRenderer line = Instantiate(pathPrefab).GetComponent<LineRenderer>();

            if(GameObject.Find("Paths") == null){
                new GameObject("Paths");
            }

            line.transform.SetParent(GameObject.Find("Paths").transform, true);

            line.SetPosition(0, transform.position);
            line.SetPosition(1, connectingNode.GetMapNodePosition());

            line.gameObject.SetActive(true);
            connectingNodes.Add(connectingNode.GetMapNode());

            EnsureDifferentNodeTypes(connectingNode.GetMapNode());
        }

        /// <summary>
        /// Ensures that the node types of connected nodes are different.
        /// </summary>
        private void EnsureDifferentNodeTypes(MapNode newConnection)
        {
            foreach (var connectedNode in connectingNodes)
            {
                if (connectedNode != newConnection && connectedNode.type == newConnection.type)
                {
                    NodeType newType = GetDifferentNodeType(connectedNode.type);
                    newConnection.SetNodeType(newType);
                }
            }
        }

        /// <summary>
        /// Returns a node type different from the provided type.
        /// </summary>
        private NodeType GetDifferentNodeType(NodeType typeToAvoid)
        {
            foreach (var rule in mapGeneration.data.CustomWeightedFloors)
            {
                if (rule.Floor == myNodeData.GetXY().x + 2 && Random.value <= rule.CustomWeight)
                {
                    return rule.TargetType;
                }
            }

            var availableTypes = new List<NodeType>();
            var availableWeights = new List<float>();

            foreach (var nodeWeight in mapGeneration.data.NodeWeights)
            {
                if (nodeWeight.type != typeToAvoid && mapGeneration.data.IsNodeTypeAllowed(nodeWeight.type, myNodeData.GetXY().x))
                {
                    availableTypes.Add(nodeWeight.type);
                    availableWeights.Add(nodeWeight.weight);
                }
            }

            return OrangeUtils.GetRandomWeightedItem(availableTypes, availableWeights);
        }

        /// <summary>
        /// Sets the node type and updates the sprite.
        /// </summary>
        private void SetNodeType(NodeType newType)
        {
            type = newType;
            SetSprite(newType, notInteractableColor);
        }

        /// <summary>
        /// Sets the sprite based on the node type and color.
        /// </summary>
        private void SetSprite(NodeType type, Color color)
        {
            if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
            {
                sr.sprite = AssetHolder.Instance.GetSprite(type);
                sr.color = color;
            }
        }

        /// <summary>
        /// Sets the sprite color.
        /// </summary>
        private void SetSpriteColor(Color color)
        {
            if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
            {
                sr.color = color;
            }
        }

        private void OnDestroy()
        {
            myNodeData.DeactivateNode();
        }
    }
}
