using System.Collections.Generic;
using UnityEngine;
using OrangeSensei.Utils;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace OrangeSensei.MapGeneration
{
    public class MapGeneration : MonoBehaviour
    {
        public static MapGeneration Instance;

        [Header("Map Properties")]
        public GenerationData data;

        [Header("Prefabs")]
        public Transform spawnPosition;
        public MapNode mapNodePrefab;
        public MapNode bossNodePrefab;

        [Header("Randomness Properties")]
        public float positionRandomOffsetRadius = 0.4f;

        private Grid<Node> grid;
        private List<int> chosenStartingPoints = new List<int>();
        private List<MapNode> allNodes = new List<MapNode>();
        Transform mapNodesParent;

        private void Awake() {
            Instance = this;
        }

        void Start()
        {
            mapNodesParent = new GameObject("MapNodes").transform;
            InitializeGrid();
            GenerateMap();
        }

        /// <summary>
        /// Initializes the grid with nodes based on the given generation data.
        /// </summary>
        private void InitializeGrid()
        {
            //Adds extra X axis for the boss node
            grid = new Grid<Node>(data.MapFloors + 1, data.NodesPerFloor, data.CellSize, spawnPosition.position, (Grid<Node> g, int x, int y) => new Node(g, x, y));
        }

        /// <summary>
        /// Generates the map by instantiating nodes and setting their types based on the provided data.
        /// </summary>
        public void GenerateMap()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            //Refreshing the grid and map by removing last generated nodes
            foreach (var item in allNodes)
            {
                Destroy(item.gameObject);
            }

            if(GameObject.Find("Paths") != null){
                foreach (Transform child in GameObject.Find("Paths").transform)
                {
                    Destroy(child.gameObject);
                }
            }

            allNodes.Clear();
            chosenStartingPoints.Clear();

            for (int x = 0; x < data.MapFloors; x++)
            {
                for (int y = 0; y < data.NodesPerFloor; y++)
                {
                    CreateMapNode(x, y);
                }
            }

            Vector3 worldPosition = new Vector2((data.MapFloors + 1) * data.CellSize, data.NodesPerFloor * data.CellSize / 2);
            MapNode bossNode = Instantiate(bossNodePrefab, worldPosition, Quaternion.identity, mapNodesParent);
            grid.GetGridObject(data.MapFloors, data.NodesPerFloor / 2).SetMapNode(bossNode, NodeType.Boss);
            bossNode.myNodeData.ActivateNode();
            allNodes.Add(bossNode);

            for (int i = 0; i < data.StartingPaths; i++)
            {
                CreateCompletePath();   
            }

            DeactivateUnactivatedNodes();
            
            stopWatch.Stop();
            UnityEngine.Debug.Log($"Map generation took {stopWatch.Elapsed.TotalSeconds} seconds.");


            var chosenPointsNodes = chosenStartingPoints.Select(point => grid.GetGridObject(0, point).GetMapNode()).ToList();
            FindObjectOfType<MapController>().UpdateNodes(chosenPointsNodes);
        }

        /// <summary>
        /// Creates a map node at the specified grid coordinates.
        /// </summary>
        private void CreateMapNode(int x, int y)
        {
            Vector3 worldPosition = grid.GetWorldPosition(x, y) + new Vector3(data.CellSize, data.CellSize) * 0.5f;
            Vector2 offset = Random.insideUnitCircle * positionRandomOffsetRadius;
            worldPosition += new Vector3(offset.x, offset.y, 0f);

            MapNode mapNode = Instantiate(mapNodePrefab, worldPosition, Quaternion.identity, mapNodesParent);
            NodeType type = GetNodeType(x + 1);
            grid.GetGridObject(x, y).SetMapNode(mapNode, type);
            mapNode.AddEncounter(data.GetEncounterData(type));
            allNodes.Add(mapNode);
        }

        /// <summary>
        /// Returns a node type based on the floor value and custom weighted rules.
        /// </summary>
        private NodeType GetNodeType(int floorValue){
            foreach (var rule in data.CustomWeightedFloors)
            {
                //Checks for custom weighted floors based on the generation data
                if (rule.Floor == floorValue && Random.value <= rule.CustomWeight)
                {
                    return rule.TargetType;
                }
            }

            List<NodeType> availableTypes = new List<NodeType>();
            List<float> availableWeights = new List<float>();

            for (int i = 0; i < data.NodeWeights.Count; i++)
            {
                NodeType type = data.NodeWeights[i].type;
                if (data.IsNodeTypeAllowed(type, floorValue))
                {
                    availableTypes.Add(type);
                    availableWeights.Add(data.NodeWeights[i].weight);
                }
            }

            return OrangeUtils.GetRandomWeightedItem(availableTypes, availableWeights);
        }
        
        /// <summary>
        /// Creates a complete path from a start point to the end of the grid.
        /// </summary>
        public void CreateCompletePath()
        {
            //if (chosenStartingPoints.Count >= data.NodesPerFloor) return;

            //Checks If we have enough minimum starting points if not we make sure to get a unique one!
            int randomStartIndex = chosenStartingPoints.Count < data.MinStartPoints ? GetRandomStartPoint() : Random.Range(0, data.NodesPerFloor);
            if (chosenStartingPoints.Count >= data.MaxStartPoints)
            {
                randomStartIndex = chosenStartingPoints[Random.Range(0, chosenStartingPoints.Count)];
            }

            chosenStartingPoints.Add(randomStartIndex);
            Node currentNode = grid.GetGridObject(0, randomStartIndex);
            currentNode.ActivateNode();
            List<Node> path = new List<Node> { currentNode };

            while (currentNode.GetXY().x < data.MapFloors - 1)
            {
                Node[] possibleConnections = GetPossibleConnections(currentNode.GetXY().x, currentNode.GetXY().y);
                Node nextNode = possibleConnections[Random.Range(0, possibleConnections.Length)];

                currentNode.GetMapNode().MakePath(nextNode);
                currentNode = nextNode;
                currentNode.ActivateNode();
                path.Add(currentNode);
            }

            currentNode.GetMapNode().MakePath(grid.GetGridObject(data.MapFloors, data.NodesPerFloor / 2));

            ValidateAndFixPath(path);
        }

        /// <summary>
        /// Validates the path to ensure no consecutive node restrictions are violated, and adjusts the path if needed.
        /// </summary>
        private void ValidateAndFixPath(List<Node> path)
        {
            NodeType lastType = NodeType.None;

            //Checks for consecutive node restrictions, if found consecutive, change the node type and we make sure it follows all the other restrictions!
            for (int i = 1; i < path.Count; i++)
            {
                NodeType currentType = path[i].GetNodeType();
                NodeRestriction restriction = data.GetNodeRestriction(currentType);

                if (restriction != null && restriction.AllowConsecutive && currentType == lastType)
                {
                    NodeType newType = GetNonConsecutiveNodeType(path[i].GetXY().x + 1, lastType);
                    path[i].SetMapNode(path[i].GetMapNode() , newType);
                }

                lastType = currentType;
            }
        }

        /// <summary>
        /// Gets a node type that is not the same as the given lastType for the given floor.
        /// </summary>
        private NodeType GetNonConsecutiveNodeType(int floorValue, NodeType lastType)
        {
            List<NodeType> availableTypes = new List<NodeType>();
            List<float> availableWeights = new List<float>();

            for (int i = 0; i < data.NodeWeights.Count; i++)
            {
                NodeType type = data.NodeWeights[i].type;
                if (type != lastType && data.IsNodeTypeAllowed(type, floorValue))
                {
                    availableTypes.Add(type);
                    availableWeights.Add(data.NodeWeights[i].weight);
                }
            }

            return OrangeUtils.GetRandomWeightedItem(availableTypes, availableWeights);
        }

        /// <summary>
        /// Returns a random start point that has not been chosen yet.
        /// </summary>
        private int GetRandomStartPoint()
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, data.NodesPerFloor);
            } while (chosenStartingPoints.Contains(randomIndex));
            return randomIndex;
        }


        /// <summary>
        /// Returns possible connections for the given coordinates in the grid.
        /// </summary>
        private Node[] GetPossibleConnections(int x, int y)
        {
            List<Node> possibleConnections = new List<Node>();

            if (x == data.MapFloors - 1)
            {
                UnityEngine.Debug.Log("This is the final Floor, can't go further");
            }
            else
            {
                int lowerBound = Mathf.Max(0, y - 1);
                int upperBound = Mathf.Min(data.NodesPerFloor - 1, y + 1);

                for (int i = lowerBound; i <= upperBound; i++)
                {
                    possibleConnections.Add(grid.GetGridObject(x + 1, i));
                }
            }

            return possibleConnections.ToArray();
        }

        /// <summary>
        /// Deactivates all nodes that have not been activated.
        /// </summary>
        private void DeactivateUnactivatedNodes()
        {
            foreach (var item in allNodes)
            {
                if (!item.myNodeData.IsActivated)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }
    }
}
