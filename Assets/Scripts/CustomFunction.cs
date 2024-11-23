using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomFunction : IBenchmark
{
    public Vector2 DomainMin
    {
        get { return new Vector2(-5.12f, -5.12f); }
    }
    public Vector2 DomainMax
    {
        get { return new Vector2(5.12f, 5.12f); }
    }

    public float Evaluate(Vector2 x)
    {
        return x[0] * x[0] + x[1] * x[1];
    }
}
