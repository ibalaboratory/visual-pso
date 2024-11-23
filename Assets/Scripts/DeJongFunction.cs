using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DeJong's functions.
/// </summary>
namespace DeJongFunction
{
    public class F1 : IBenchmark
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

        public Vector2 Optimum
        {
            get { return new Vector2(0.0f, 0.0f); }
        }
    }

    public class F2 : IBenchmark
    {
        public Vector2 DomainMin
        {
            get { return new Vector2(-2.048f, -2.048f); }
        }
        public Vector2 DomainMax
        {
            get { return new Vector2(2.048f, 2.048f); }
        }

        public float Evaluate(Vector2 x)
        {
            float a = x[0] * x[0] - x[1];
            float b = 1f - x[0];
            return 100f * a * a + b * b;
        }

        public Vector2 Optimum
        {
            get { return new Vector2(1.0f, 1.0f); }
        }
    }

    public class F3 : IBenchmark
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
            return Mathf.Floor(x[0]) + Mathf.Floor(x[1]);
        }

        public Vector2 Optimum
        {
            get { return new Vector2(-5.12f, -5.12f); }
        }
    }

    public class F4 : IBenchmark
    {
        public Vector2 DomainMin
        {
            get { return new Vector2(-1.28f, -1.28f); }
        }
        public Vector2 DomainMax
        {
            get { return new Vector2(1.28f, 1.28f); }
        }

        public float Evaluate(Vector2 x)
        {
            return Pow4(x[0]) + Pow4(x[1]) * 2 + MathfUtility.GaussianRandom() / 15f;
        }

        public float Display(Vector2 x)
        {
            return Pow4(x[0]) + Pow4(x[1]) * 2;
        }

        public Vector2 Optimum
        {
            get { return new Vector2(0.0f, 0.0f); }
        }

        private float Pow4(float x)
        {
            x *= x;
            return x * x;
        }
    }

    public class F5 : IBenchmark
    {
        public Vector2 DomainMin
        {
            get { return new Vector2(-65.536f, -65.536f); }
        }
        public Vector2 DomainMax
        {
            get { return new Vector2(65.536f, 65.536f); }
        }

        public float Evaluate(Vector2 x)
        {
            // The inverse of the output.
            float outInv = 0.002f;
            for(int j = 0; j < 25; j++)
            {
                outInv += 1f / (j + 1 + Pow6(x[0] - a0[j]) + Pow6(x[1] - a1[j]));
            }
            return 1f / outInv;
        }

        private readonly float[] a0 = new float[]
        {
            -32, -16, 0, 16, 32,
            -32, -16, 0, 16, 32,
            -32, -16, 0, 16, 32,
            -32, -16, 0, 16, 32,
            -32, -16, 0, 16, 32
        };

        private readonly float[] a1 = new float[]
        {
            -32, -32, -32, -32, -32,
            -16, -16, -16, -16, -16,
              0,   0,   0,   0,   0,
             16,  16,  16,  16,  16,
             32,  32,  32,  32,  32
        };

        public Vector2 Optimum
        {
            get { return new Vector2(-32f, -32f); }
        }

        private float Pow6(float x)
        {
            x *= x;
            return x * x * x;
        }
    }

    public class F6 : IBenchmark
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
            float output = 20f;
            output += x[0] * x[0] - 10f * Mathf.Cos(2f * Mathf.PI * x[0]);
            output += x[1] * x[1] - 10f * Mathf.Cos(2f * Mathf.PI * x[1]);
            return output;
        }

        public Vector2 Optimum
        {
            get { return new Vector2(0.0f, 0.0f); }
        }
    }

    public class F7 : IBenchmark
    {
        public Vector2 DomainMin
        {
            get { return new Vector2(-10f, -10f); }
        }
        public Vector2 DomainMax
        {
            get { return new Vector2(10f, 10f); }
        }

        public float Evaluate(Vector2 x)
        {
            float x0 = x[0] - 100f;
            float x1 = x[1] - 100f;
            float sum = x0 * x0 + x1 * x1;
            float product = Mathf.Cos(x0) * Mathf.Cos(x1 * InvSqrt2);
            return 0.00025f * sum - product + 1f;
        }
        
        // The inverse of the square root of 2.
        private readonly float InvSqrt2 = Mathf.Sqrt(2) / 2f;

        public Vector2 Optimum
        {
            get { return new Vector2(8.939f, 6.793f); }
        }
    }
}
