using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility functions for float-type mathematical operations.
/// </summary>
public static class MathfUtility
{
    // Method List:
    //     MathfUtility.GaussianRandom();
    //


    /// <summary>
    /// Generator that draws samples from a Gaussian distribution (mean 0, variance 1).
    /// </summary>
    private static IEnumerator<float> GaussianRandomGenerator;

    static MathfUtility()
    {
        GaussianRandomGenerator = CreateGaussianRandomGenerator();
    }

    /// <summary>
    /// Draws a random float number from a Gaussian distribution (mean 0, variance 1).
    /// </summary>
    /// <returns>A random float number, drawn from a Gaussian distribution (mean 0, variance 1)</returns>
    public static float GaussianRandom()
    {
        GaussianRandomGenerator.MoveNext();
        return GaussianRandomGenerator.Current;
    }

    /// <summary>
    /// Creates a generator that draws samples from a Gaussian distribution (mean 0, variance 1).
    /// (Box-Muller's method.)
    /// </summary>
    /// <returns>Gaussian random generator</returns>
    private static IEnumerator<float> CreateGaussianRandomGenerator()
    {
        float u, v;
        while(true)
        {
        do u = Random.value; while(u==0);
        v = Random.Range(0f,360f);
        
        float r = Mathf.Sqrt(-2*Mathf.Log(u));
        yield return r * Mathf.Cos(v);
        yield return r * Mathf.Sin(v);
        }
    }
}
