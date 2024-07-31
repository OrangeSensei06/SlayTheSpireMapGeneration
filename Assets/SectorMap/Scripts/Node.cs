using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OrangeSensei.MapGeneration
{
    //Class For storing the Node Base Data like position, type and is being used or not!
    public class Node
    {
        private Grid<Node> grid;
        private int x; //Floor
        private int y; //Node Location on floor
        private MapNode mapNode;
        private NodeType type;
        public bool IsActivated;

        public Node(Grid<Node> grid, int x, int y){
            this.grid = grid;
            this.x = x;
            this.y = y;
            IsActivated = false;
        }

        public void SetMapNode(MapNode mapNode, NodeType type){
            this.type = type;
            this.mapNode = mapNode;
            //Add the node type to the data!
            mapNode.Initialize(this, type);
        }

        #region Value Returning Functions

        public Vector2Int GetXY(){
            return new Vector2Int(x, y);
        }

        public void ActivateNode(){
            IsActivated = true;
        }

        public void DeactivateNode(){
            IsActivated = false;
            mapNode = null;
        }

        public override string ToString()
        {
            return x + ", " + y;
        }

        public Vector3 GetMapNodePosition(){
            return mapNode.transform.position;
        }

        public MapNode GetMapNode(){
            return mapNode;
        }

        public NodeType GetNodeType(){
            return type;
        }

        #endregion
    }
}
