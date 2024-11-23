using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Particle swarm optimization algorithm for minimization of a 2D function.
/// </summary>
public class PSO
{
    public IBenchmark Function { get; private set; }

    public int Iteration { get; private set; } = 0;

    public enum OptimizationType
    {
        Minimization = 0,
        Maximization
    }

    // input arguments: (currentValue, bestSoFar)
    // currentValue < bestSoFar for Minimization.
    // currentValue > bestSoFar for Maximization.
    private System.Func<float, float, bool> optimizationComparator;

    [System.Serializable]
    public class Parameters
    {
        public OptimizationType Optimization = OptimizationType.Minimization;
        public float ConstrictionFactor = 1.0f;
        public float InertiaWeight = 0.9f;
        public bool DecayInertiaWeight = true;
        public float TargetInertiaWeight = 0.4f;
        public float AcceralationConstantPersonal = 2.0f;
        public float AcceralationConstantGlobal = 2.0f;
        public float VMax = -1;
        public int MaxIteration = 500;

        /// <summary>
        /// Sets VMax to be equal to the range of the domain of the function.
        /// </summary>
        /// <param name="function">The function to be optimized.</param>
        public void TrySetDefaultVMax(IBenchmark function)
        {
            if(function == null) return;
            this.VMax = function.DomainMax.x - function.DomainMin.x;
        }
    }

    private Parameters parameters = new Parameters();
    public Parameters GetParameters()
    {
        return parameters;
    }
    public void SetParameters(Parameters parameters)
    {
        if(parameters == null) throw new System.ArgumentNullException();
        this.parameters = parameters;
        if(this.parameters.VMax < 0) this.parameters.TrySetDefaultVMax(Function);
    }

    public class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float CurrentValue { get; set; }

        public float PersonalBestValue { get; set; }
        public Vector2 PersonalBestPosition { get; set; }

        public Vector3 Coordinates
        {
            get
            {
                return new Vector3(Position[0], CurrentValue, Position[1]);
            }
        }
    }

    public List<Particle> Particles { get; private set; }

    public float GlobalBestValue { get; private set; }
    public Vector2 GlobalBestPosition { get; private set; }

    public int Population
    {
        get
        {
            return (Particles == null) ? 0 : Particles.Count;
        }
    }

    // Alias of parameters.
    public float ConstrictionFactor { get { return parameters.ConstrictionFactor; } }
    public float InertiaWeight
    {
        get { return parameters.InertiaWeight; }
        private set { parameters.InertiaWeight = value; }
    }
    public bool DecayInertiaWeight { get { return parameters.DecayInertiaWeight; } }
    public float TargetInertiaWeight { get { return parameters.TargetInertiaWeight; } }
    public float AcceralationConstantPersonal { get { return parameters.AcceralationConstantPersonal; } }
    public float AcceralationConstantGlobal { get { return parameters.AcceralationConstantGlobal; } }
    public float VMax { get { return parameters.VMax; } }
    public int MaxIteration { get { return parameters.MaxIteration; } }


    public PSO(IBenchmark function, int population = 50, Parameters parameters = null)
    {
        if(function == null) throw new System.ArgumentNullException();
        Function = function;
        if(parameters != null) SetParameters(parameters);
        else if(this.parameters.VMax < 0) this.parameters.TrySetDefaultVMax(Function);
        Initialize(population);
    }

    /// <summary>
    /// Updates the particle's position & velocity.
    /// </summary>
    public void Update()
    {
        Iteration++;

        if(Iteration > MaxIteration)
        {
            throw new System.Exception("MaxIteration exceeded.");
        }

        foreach(Particle p in Particles)
        {
            Vector2 rPersonal = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            Vector2 rGlobal = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            Vector2 newVelocity = ConstrictionFactor * (
                InertiaWeight * p.Velocity
                + AcceralationConstantPersonal * rPersonal * (p.PersonalBestPosition - p.Position)
                + AcceralationConstantGlobal * rGlobal * (GlobalBestPosition - p.Position));

            newVelocity[0] = Mathf.Clamp(newVelocity[0], -VMax, VMax);
            newVelocity[1] = Mathf.Clamp(newVelocity[1], -VMax, VMax);

            Vector2 newPosition = p.Position + newVelocity;
            newPosition[0] = Mathf.Clamp(newPosition[0], Function.DomainMin[0], Function.DomainMax[0]);
            newPosition[1] = Mathf.Clamp(newPosition[1], Function.DomainMin[1], Function.DomainMax[1]);

            p.Position = newPosition;
            p.Velocity = newVelocity;

            p.CurrentValue = Function.Evaluate(newPosition);
            if(optimizationComparator(p.CurrentValue, p.PersonalBestValue))
            {
                p.PersonalBestValue = p.CurrentValue;
                p.PersonalBestPosition = new Vector2(newPosition[0], newPosition[1]);

                if(optimizationComparator(p.CurrentValue, GlobalBestValue))
                {
                    GlobalBestValue = p.CurrentValue;
                    GlobalBestPosition = new Vector2(newPosition[0], newPosition[1]);
                }
            }
        }

        if(DecayInertiaWeight && Iteration != MaxIteration)
        {
            InertiaWeight -= (InertiaWeight - TargetInertiaWeight) / (MaxIteration - Iteration);
        }
    }


    /// <summary>
    /// Randomly initializes all the particles.
    /// </summary>
    /// <param name="population">Number of particles</param>
    private void Initialize(int population)
    {
        Iteration = 0;

        Particles = new List<Particle>();

        switch (parameters.Optimization)
        {
            case OptimizationType.Minimization:
                GlobalBestValue = float.MaxValue;
                optimizationComparator = (x, y) => x < y;
                break;
            case OptimizationType.Maximization:
                GlobalBestValue = float.MinValue;
                optimizationComparator = (x, y) => x > y;
                break;
            default:
                throw new System.ArgumentException($"Unknown OptimizationType {parameters.Optimization}.");
        }

        for(int i = 0; i < population; i++)
        {
            Particle particle = new Particle();

            float x0 = Random.Range(Function.DomainMin[0], Function.DomainMax[0]);
            float x1 = Random.Range(Function.DomainMin[1], Function.DomainMax[1]);
            particle.Position = new Vector2(x0, x1);

            float v0 = Random.Range(-VMax, VMax);
            float v1 = Random.Range(-VMax, VMax);
            particle.Velocity = new Vector2(v0, v1);

            particle.CurrentValue = Function.Evaluate(particle.Position);
            particle.PersonalBestValue = particle.CurrentValue;
            particle.PersonalBestPosition = new Vector2(particle.Position[0], particle.Position[1]);

            if(optimizationComparator(particle.PersonalBestValue, GlobalBestValue))
            {
                GlobalBestValue = particle.PersonalBestValue;
                GlobalBestPosition = new Vector2(particle.Position[0], particle.Position[1]);
            }

            Particles.Add(particle);
        }
    }
}
