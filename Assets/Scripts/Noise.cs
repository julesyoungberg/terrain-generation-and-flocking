using UnityEngine;

public static class Noise
{

    private static readonly int[] hash = {
        151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
        140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
        247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
        57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
        74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
        60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
        65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
        200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
        52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
        207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
        119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
        129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
        218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
        81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
        184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
        222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180,

        151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
        140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
        247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
        57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
        74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
        60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
        65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
        200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
        52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
        207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
        119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
        129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
        218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
        81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
        184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
        222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
    };

    private const int hashMask = 255;

    private static Vector2[] gradients = {
        new Vector2( 1f, 0f),
        new Vector2(-1f, 0f),
        new Vector2( 0f, 1f),
        new Vector2( 0f,-1f),
        new Vector2( 1f, 1f).normalized,
        new Vector2(-1f, 1f).normalized,
        new Vector2( 1f,-1f).normalized,
        new Vector2(-1f,-1f).normalized
    };

    private const int gradientsMask = 7;

    private static float sqr2 = Mathf.Sqrt(2f);

    // Generates a square noise patch of size res
    public static float[,] GenerateNoise(int res, float frequency, int octaves, float lacunarity, float persistence, int height, Vector3 offset)
    {
        float[,] noise = new float[res, res];

        // setup four corners of grid
        Vector3 point00 = new Vector3(-0.5f, -0.5f) + offset;
        Vector3 point10 = new Vector3(0.5f, -0.5f) + offset;
        Vector3 point01 = new Vector3(-0.5f, 0.5f) + offset;
        Vector3 point11 = new Vector3(0.5f, 0.5f) + offset;

        float stepSize = 1f / res;
        for (int y = 0; y < res; y++)
        {
            // find the start and end of the current row
            Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
            for (int x = 0; x < res; x++)
            {
                // find the current point
                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                // sample perlin noise
                float sample = FractalPerlin(point, frequency, octaves, lacunarity, persistence);
                // adjust the domain of the noise from -1 to 1 to 0 to 1
                sample = sample * 0.5f + 0.5f;
                // scale the noise to the desired height
                sample *= height;
                noise[x, y] = sample;
            }
        }

        return noise;
    }

    // computes a fractal perlin noise sample
    public static float FractalPerlin(Vector3 point, float frequency, int octaves, float lacunarity, float persistence)
    {
        // set initial values
        float sum = Perlin(point, frequency);
        float amplitude = 1f;
        float range = 1f;
        // cycle through octaves collection samples
        for (int o = 1; o < octaves; o++)
        {
            frequency *= lacunarity;
            amplitude *= persistence;
            range += amplitude;
            sum += Perlin(point, frequency) * amplitude;
        }
        return sum / range;
    }

    // computes a perlin noise sample for a given 2D point
    public static float Perlin(Vector3 point, float frequency)
    {
        point *= frequency;

        // calculate the coordinates of cell
        Vector2 p = new Vector2(point.x, point.y);
        int x0 = Mathf.FloorToInt(point.x);
        int y0 = Mathf.FloorToInt(point.y);
        int x1 = x0 + 1;
        int y1 = y0 + 1;
        Vector2 x0y0 = new Vector2(x0, y0);
        Vector2 x0y1 = new Vector2(x0, y1);
        Vector2 x1y0 = new Vector2(x1, y0);
        Vector2 x1y1 = new Vector2(x1, y1);

        // calculate random gradients at cell corners
        int h0 = hash[x0 & hashMask];
        int h1 = hash[x1 & hashMask];
        Vector2 g00 = gradients[hash[h0 + (y0 & hashMask)] & gradientsMask];
        Vector2 g10 = gradients[hash[h1 + (y0 & hashMask)] & gradientsMask];
        Vector2 g01 = gradients[hash[h0 + (y1 & hashMask)] & gradientsMask];
        Vector2 g11 = gradients[hash[h1 + (y1 & hashMask)] & gradientsMask];

        // calculate difference vectors from cell corners to p
        Vector2 a = p - x0y0;
        Vector2 b = p - x1y0;
        Vector2 c = p - x0y1;
        Vector2 d = p - x1y1;

        // dot products to get scalar values for the corners
        float s = Vector2.Dot(g00, a);
        float t = Vector2.Dot(g10, b);
        float u = Vector2.Dot(g01, c);
        float v = Vector2.Dot(g11, d);

        // smooth interpolation
        float sx = Smooth(a.x);
        float sy = Smooth(a.y);
        return Mix(Mix(s, t, sx), Mix(u, v, sx), sy);
    }

    // Linear interpolation function
    private static float Mix(float x, float y, float a)
    {
        return (1 - a) * x + a * y;
    }

    // smooth interpolation function
    private static float Smooth(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }
}