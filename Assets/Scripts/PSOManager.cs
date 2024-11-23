using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Executes PSO algorithm and displays particles on the scene.
/// </summary>
public class PSOManager : MonoBehaviour
{
    public enum BenchmarkName
    {
        F1, F2, F3, F4, F5, F6, F7, Custom
    }

    [SerializeField]
    private BenchmarkName benchmarkName = BenchmarkName.F1;

    [SerializeField]
    private int population = 50;

    [SerializeField]
    private PSO.Parameters psoParameters = new PSO.Parameters();

    [Header("Object Reference")]
    [SerializeField]
    private SurfacePlotter plotter = null;
    [SerializeField]
    private GameObject particleObject = null;
    [SerializeField]
    private Transform optimumTransform = null;
    [SerializeField]
    private Text statusText = null;
    [SerializeField]
    private LineGraph lineGraph = null;

    [Header("Other")]
    [SerializeField]
    private bool updateNoisySurface = false;
    [SerializeField]
    private float secondsPerIteration = 0.5f;
    private const float MinimumSecondsPerIteration = 0.02f;


    private IBenchmark benchmark;
    private PSO pso;
    private List<Transform> particles;
    private List<Vector2> plotData = new List<Vector2>();


    void Start()
    {
        benchmark = GetBenchmark(benchmarkName);
        if(benchmark == null)
        {
            Debug.LogError("Unrecognized benchmark function.");
            this.enabled = false;
            return;
        }

        if(plotter == null)
        {
            Debug.LogWarning("SurfacePlotter not set. Surface plot will not be displayed.");
        }
        else
        {
            DisplayBenchmark();
        }

        pso = new PSO(benchmark, population, psoParameters);

        plotData.Add(new Vector2(pso.Iteration, pso.GlobalBestValue));

        if(particleObject == null)
        {
            Debug.LogWarning("ParticleObject not set. Particles will not be displayed.");
        }
        else
        {
            InitializeParticles();
            TryDisplayParticles();
        }

        TryDisplayOptimum();

        if(lineGraph == null)
        {
            Debug.LogWarning("LineGraph not set. Line graph will not be displayed.");
        }

        UpdateFixedDeltaTime();
    }

    // Executes one step of PSO every Time.fixedDeltaTime seconds.
    void FixedUpdate()
    {
        if(pso == null) return;

        try
        {
            pso.Update();
        }
        catch
        {
            Debug.Log("MaxIteration exceeded.");
            pso = null;
            return;
        }

        plotData.Add(new Vector2(pso.Iteration, pso.GlobalBestValue));

        Debug.Log($"({pso.Iteration}) {pso.GlobalBestPosition}: {pso.GlobalBestValue}");
    }

    void Update()
    {
        TryDisplayParticles();
        TryUpdateNoisySurface();
        TryUpdateText();
        TryDisplayOptimum();
        TryPlotLineGraph();
        UpdateFixedDeltaTime();
    }


    /// <summary>
    /// Gets an instance of a benchmark function.
    /// </summary>
    /// <param name="benchmarkName">The name of a benchmark function</param>
    /// <returns>An instance of the benchmark function</returns>
    private IBenchmark GetBenchmark(BenchmarkName benchmarkName)
    {
        switch(benchmarkName)
        {
            case BenchmarkName.F1:
                return new DeJongFunction.F1();
            case BenchmarkName.F2:
                return new DeJongFunction.F2();
            case BenchmarkName.F3:
                return new DeJongFunction.F3();
            case BenchmarkName.F4:
                return new DeJongFunction.F4();
            case BenchmarkName.F5:
                return new DeJongFunction.F5();
            case BenchmarkName.F6:
                return new DeJongFunction.F6();
            case BenchmarkName.F7:
                return new DeJongFunction.F7();
            case BenchmarkName.Custom:
                return new CustomFunction();
            default:
                return null;
        }
    }

    private void DisplayBenchmark()
    {
        if(benchmark == null)
        {
            throw new System.NullReferenceException("\"benchmark\" is null.");
        }
        if(plotter == null)
        {
            throw new System.NullReferenceException("\"plotter\" is null.");
        }
        
        plotter.xLimits.min = benchmark.DomainMin[0];
        plotter.xLimits.max = benchmark.DomainMax[0];
        plotter.zLimits.min = benchmark.DomainMin[1];
        plotter.zLimits.max = benchmark.DomainMax[1];

        plotter.Plot(benchmark.Display);
    }

    private void InitializeParticles()
    {
        if(particleObject == null)
        {
            throw new System.NullReferenceException("\"particleObject\" is null.");
        }
        if(pso == null)
        {
            throw new System.NullReferenceException("\"pso\" is null.");
        }

        particles = new List<Transform>();

        Transform surface = plotter?.transform?.Find("Surface");
        Vector3 surfacePosition = (surface == null) ? Vector3.zero : surface.position;
        Vector3 surfaceScale = (surface == null) ? Vector3.one : surface.lossyScale;

        GameObject particlesParent = new GameObject("Particles");
        particlesParent.transform.position = surfacePosition;
        particlesParent.transform.localScale = surfaceScale;

        int numParticles = pso.Population;
        for(int i = 0; i < numParticles; i++)
        {
            GameObject particle = Instantiate(particleObject, particlesParent.transform);
            particle.SetActive(true);

            // Keep the lossy (global) scale of particle objects the same.
            Vector3 targetScale = particleObject.transform.localScale;
            Vector3 parentScale = particlesParent.transform.lossyScale;
            particle.transform.localScale = new Vector3(targetScale.x / parentScale.x, targetScale.y / parentScale.y, targetScale.z / parentScale.z);

            particles.Add(particle.transform);
        }

        particleObject.SetActive(false);
    }

    private void TryDisplayParticles()
    {
        if(particles == null || pso == null) return;

        int numParticles = pso.Population;
        for(int i = 0; i < numParticles; i++)
        {
            particles[i].localPosition = pso.Particles[i].Coordinates;
        }
    }

    private void TryDisplayOptimum()
    {
        if(optimumTransform == null || benchmark == null || pso == null) return;

        Vector2 range = benchmark.DomainMax - benchmark.DomainMin;
        float xScale = (range[0] == 0) ? 0 : 1f / range[0];
        float zScale = (range[1] == 0) ? 0 : 1f / range[1];
        float surfaceScale = 20f;
        Vector3 position = new Vector3();
        position.x = pso.GlobalBestPosition[0] * xScale * surfaceScale;
        position.y = optimumTransform.position.y;
        position.z = pso.GlobalBestPosition[1] * zScale * surfaceScale;
        optimumTransform.position = position;
    }

    private void TryUpdateText()
    {
        if(statusText == null || pso == null) return;

        statusText.text = string.Join(
            System.Environment.NewLine,
            $"Iteration: {pso.Iteration}",
            $"Best Position: ({pso.GlobalBestPosition[0]:F2}, {pso.GlobalBestPosition[1]:F2})",
            $"Best Value: {pso.GlobalBestValue:F4}"
        );
    }

    private void TryPlotLineGraph()
    {
        if(lineGraph == null || plotData == null) return;
        lineGraph.Plot(plotData);
    }

    private void UpdateFixedDeltaTime()
    {
        secondsPerIteration = Mathf.Max(secondsPerIteration, MinimumSecondsPerIteration);
        Time.fixedDeltaTime = secondsPerIteration;
    }

    private void TryUpdateNoisySurface()
    {
        if(plotter == null || benchmark == null) return;
        if(updateNoisySurface)
        {
            plotter.autoCalculateHeightLimits = false;
            plotter.Plot(benchmark.Evaluate);
        }
    }
}
