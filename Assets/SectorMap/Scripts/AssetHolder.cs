using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OrangeSensei.MapGeneration
{
    public class AssetHolder : MonoBehaviour
    {
        public static AssetHolder Instance;

        [System.Serializable]
        public struct NodeTypeSprites{
            public NodeType type;
            public Sprite sprite;
        }

        public List<NodeTypeSprites> nodeTypeSprites;

        private Dictionary<NodeType, Sprite> nodeTypeSpriteDictionary;

        private void Awake() {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            nodeTypeSpriteDictionary = new Dictionary<NodeType, Sprite>();
            foreach (var item in nodeTypeSprites)
            {
                nodeTypeSpriteDictionary.Add(item.type, item.sprite);
            }
        }

        public Sprite GetSprite(NodeType type){
            return nodeTypeSpriteDictionary[type];
        }
    }
}
