using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Slightly modified version of this: https://discussions.unity.com/t/detecting-terrain-texture-at-position/448802/2
public class TerrainTexDetector : MonoBehaviour
{
    [SerializeField] Terrain terrain;
    TerrainData terrainData;
    int alphamapWidth;
    int alphamapHeight;
    float[,,] splatmapData;
    int numTextures;

    void Start()
    {
        //terrainData = Terrain.activeTerrain.terrainData;
        terrainData = terrain.terrainData;
        alphamapWidth = terrainData.alphamapWidth;
        alphamapHeight = terrainData.alphamapHeight;
        splatmapData = terrainData.GetAlphamaps(0, 0, alphamapWidth, alphamapHeight);
        numTextures = splatmapData.Length / (alphamapWidth * alphamapHeight);

    }

    public int GetActiveTexture(Vector3 worldPos) {
        Vector3 terrainPos = worldPos - terrain.transform.position;

        int mapX = Mathf.RoundToInt((terrainPos.x / terrainData.size.x) * terrainData.alphamapWidth);
        int mapZ = Mathf.RoundToInt((terrainPos.z / terrainData.size.z) * terrainData.alphamapHeight);

        float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

        float maxMix = 0;
        int maxIndex = 0;

        for (int i = 0; i < splatmapData.GetLength(2); i++)
        {
            if (splatmapData[0, 0, i] > maxMix)
            {
                maxMix = splatmapData[0, 0, i];
                maxIndex = i;
            }
        }

        return maxIndex;
    }
    /*
    Vector3 ConvertPositionToMapCoordinate(Vector3 playerPosition) {
        Vector3 splatPosition = new Vector3();
        Terrain ter = Terrain.activeTerrain;
        Vector3 terPosition = ter.transform.position;
        splatPosition.x = (playerPosition.x - terPosition.x / ter.terrainData.size.x) * ter.terrainData.alphamapWidth;
        splatPosition.z = (playerPosition.z - terPosition.z / ter.terrainData.size.z) * ter.terrainData.alphamapHeight;
        return splatPosition;
    }
    
    public int GetActiveTerrainTextureIdx(Vector3 position) {
        Vector3 terrainCord = ConvertPositionToMapCoordinate(position);
        int activeTerrainIndex = 0;
        float largestOpacity = 0f;
        for (int i = 0; i < numTextures; i++) {
            if (largestOpacity < splatmapData[(int)terrainCord.z, (int)terrainCord.x, i]) {
                activeTerrainIndex = i;
                //largestOpacity = splatmapData[(int)terrainCord.z, (int)terrainCord.x, i];
            }
        }
        Debug.Log(activeTerrainIndex);
        return activeTerrainIndex;
    }
    */
}