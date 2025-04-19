using System;
using UnityEngine;
using Random = UnityEngine.Random;
public class MapGenerator : MonoBehaviour
{
    public MapConfigSO mapConfig;
    public Room roomPrefab;

    private float screenHeight;
    private float screenWidth;
    private float columnWidth;
    private Vector3 GeneratePoint;

    private void Awake()
    {
        screenHeight = Camera.main.orthographicSize * 2;
        screenWidth = screenHeight * Camera.main.aspect;
        
        columnWidth = screenWidth / (mapConfig.roomBlueprints.Count + 1);
    }

    public void CreateMap()
    {
        for (int column = 0; column < mapConfig.roomBlueprints.Count; column++)
        {
            var blueprint = mapConfig.roomBlueprints[column];
            
            var amount = Random.Range(blueprint.min, blueprint.max);


            for (int i = 0; i < amount; i++)
            {
                var room = Instantiate(roomPrefab, transform);
            }
        }
    }
}
