using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OrangeSensei.MapGeneration
{   
    [CustomEditor(typeof(MapGeneration))]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            MapGeneration mapGen = (MapGeneration)target;

            DrawDefaultInspector();

            if(GUILayout.Button("Generate")){
                mapGen.GenerateMap();
            }
        }
    }
}
