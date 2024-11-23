using UnityEngine;

/// <summary>
/// Wrapper for 2D benchmark functions (y = f(x0, x1)).
/// </summary>
public interface IBenchmark
{
    /// <summary>
    /// The lower limit of the domain of the function.
    /// </summary>
    /// <value>(MinX0, MinX1)</value>
    Vector2 DomainMin { get; }

    /// <summary>
    /// The upper limit of the domain of the function.
    /// </summary>
    /// <value>(MaxX0, MaxX1)</value>
    Vector2 DomainMax { get; }

    /// <summary>
    /// Evaluates the benchmark function: f(x0, x1).
    /// </summary>
    /// <param name="input">Input variables: (x0, x1)</param>
    /// <returns>Output variable: y = f(x0, x1)</returns>
    float Evaluate(Vector2 input);

    /// <summary>
    /// Evaluates the benchmark function for display,
    /// e.g., evaluating without random noise.
    /// </summary>
    /// <param name="input">Input variables: (x0, x1)</param>
    /// <returns>Representation of y = f(x0, x1) for display</returns>
    float Display(Vector2 input)
    {
        return Evaluate(input);
    }

    /// <summary>
    /// The coordinates of the optimum of the benchmark function.
    /// </summary>
    Vector2 Optimum { get {throw new System.NotImplementedException(); } }

    /// <summary>
    /// The optimum value of the benchmark function.
    /// </summary>
    float OptimumValue { get { return Evaluate(Optimum); } }
}
