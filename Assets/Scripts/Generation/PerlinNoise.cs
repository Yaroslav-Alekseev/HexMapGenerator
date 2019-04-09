using UnityEngine;
using System.Collections;

public static class PerlinNoise
{

    public static float[,] GenerateNoiseMap(int HexCountX, int HexCountY, int seed, float scale, int octavesCount, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[HexCountX, HexCountY];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octavesCount];
        for (int i = 0; i < octavesCount; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
            scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y < HexCountY; y++)
        {
            for (int x = 0; x < HexCountX; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octavesCount; i++)
                {
                    float sampleX = x / scale * frequency + octaveOffsets[i].x;
                    float sampleY = y / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;
                
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < HexCountY; y++)
            for (int x = 0; x < HexCountX; x++)
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            

        return noiseMap;
    }

}