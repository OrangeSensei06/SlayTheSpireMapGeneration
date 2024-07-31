using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OrangeSensei.MapGeneration
{
    public class Grid<TGridObject>
    {
        public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
        public class OnGridObjectChangedEventArgs : EventArgs{
            public int x;
            public int y;
        }

        private int width;
        private int height;
        private float cellSize;
        private TGridObject[,] gridArray;
        private TextMesh[,] debugTextArray;
        private Vector3 originPosition;

        public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject){
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            this.originPosition = originPosition;

            gridArray = new TGridObject[width, height];
            for(int x = 0; x < gridArray.GetLength(0); x++){
                for (int y = 0; y < gridArray.GetLength(1); y++){
                    gridArray[x,y] = createGridObject(this, x, y);
                 }
            }
            
            //Some Debugs to visualize the grid
            bool showDebug = true;
            if(showDebug){
                debugTextArray = new TextMesh[width, height];
                
                for(int x = 0; x < gridArray.GetLength(0); x++){
                    for (int y = 0; y < gridArray.GetLength(1); y++)
                    {
                        debugTextArray[x,y] = CreateWorldText(gridArray[x,y]?.ToString(), null, GetWorldPosition(x,y) + new Vector3(cellSize, cellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);
                        Debug.DrawLine(GetWorldPosition(x,y), GetWorldPosition(x, y +1), Color.white, 100f);
                        Debug.DrawLine(GetWorldPosition(x,y), GetWorldPosition(x+ 1, y), Color.white, 100f);
                    }
                }

                Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

                OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                    debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
                };
            }
        }

        public Vector3 GetWorldPosition(int x, int y){
            return new Vector3(x, y) * cellSize + originPosition;
        }

        public void GetXY(Vector3 worldPosition, out int x, out int y){
            x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
            y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);
        }

        #region Getting and Setting the Values
        public void SetGridObject(int x, int y, TGridObject value){
            if(x >= 0 && y >= 0 && x < width && y < height){
                gridArray[x,y] = value;
                OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
            }
        }

        public void TriggerGridObjectChanged(int x, int y){
            OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }

        public void SetGridObject(Vector3 worldPosition, TGridObject value){
            GetXY(worldPosition, out int x, out int y);
            SetGridObject(x, y, value);
        }

        public TGridObject GetGridObject(int x, int y){
            if(x >= 0 && y >= 0 && x < width && y < height){
                return gridArray[x, y];
            }else{
                return default;
            }
        }

        public TGridObject GetGridObject(Vector3 worldPosition){
            GetXY(worldPosition, out int x, out int y);
            return GetGridObject(x, y);
        }
        #endregion

        public bool CheckXYValid(int x, int y){
            bool isvalid = x >= 0 && y >= 0 && x < width && y < height;
            Debug.Log(isvalid);
            return isvalid;
        }

        private TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default, int fontSize = 40, Color color = default(Color), TextAnchor textAnchor = TextAnchor.LowerLeft){
            if(color == null) color = Color.white;

            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.text = text;
            textMesh.anchor = textAnchor;
            textMesh.fontSize = fontSize;
            return textMesh;
        }
    }
}
