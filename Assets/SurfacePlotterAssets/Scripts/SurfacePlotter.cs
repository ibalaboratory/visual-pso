using System;
using UnityEngine;

// SurfacePlotter plots the surface of a two-variable function, y = f(x, z).
public class SurfacePlotter : MonoBehaviour
{
    // Public members. 
    // (Mostly configurable parameters shown in the Editor > Inspector.)
    // -----------------------------------------------------------------

    // Two-variable function, y = f(x, z), to be plotted.
    public Func<Vector2, float> function;

    // Limits of x and z axes.
    [System.Serializable]
    public class Limits
    {
        public float min = -1f;
        public float max = 1f;
    }
    public Limits xLimits;
    public Limits zLimits;

    // Limits of y axis.
    // Only used for texture color.
    public bool autoCalculateHeightLimits = true;
    public Limits heightLimits;

    // Automatically scales the plot and centers it.
    public bool autoScale = false;

    // Number of points in each axis.
    [System.Serializable]
    public class Resolution
    {
        [Range(2, 1000)] public int x = 10;
        [Range(2, 1000)] public int z = 10;
    }
    public Resolution resolution;

    // Material of the surface mesh.
    public Material material;

    // Attach collider component to mesh.
    // Recommend "false" when you just want to display the surface.
    public bool useCollider = false;

    // See: UnityEngine.Mesh.MarkDynamic
    public bool useDynamicBuffer = true;

    // If "true", normals and tangents of the mesh are automatically calculated.
    // See Also: UnityEngine.Mesh.RecalculateNormals, RecalculateTangents
    public bool autoCalculateNormals = false;

    // -----------------------------------------------------------------


    // Internal cache.
    // --------------
    GameObject surfaceObj;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    private Limits lastXLimits;
    private Limits lastZLimits;
    private Resolution lastResolution;
    private Vector2[] functionInputs;
    // --------------


    // Initialization.
    void Awake()
    {
        CreateEmptySurface();
    }


    // Public methods.
    // --------------------------------------------------------------

    /// <summary>
    /// Plot the surface of a given function, y = f(x, z).
    /// </summary>
    /// <param name="function">Function to be plotted.</param>
    public void Plot(Func<Vector2, float> function)
    {
        this.function = function;

        Mesh mesh = meshFilter.mesh;
        Vector2[] uv = mesh.uv;
        Vector2[] uv2 = mesh.uv2;
        int[] triangles = mesh.triangles;

        // Update uv and triangles only when "resolution" or "x/z limits" has changed.
        if(IsGridChanged())
        {
            UpdateGridInfo();
            // Default mesh index buffer is 16-bit and supports only up to 65535 vertices.
            if(resolution.x * resolution.z > 65535)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            uv = GetUV(resolution);
            functionInputs = GetFunctionInputs(uv, xLimits, zLimits);
            triangles = GetTriangles(resolution);
            mesh.Clear();
        }

        mesh.vertices = GetVertices(function, functionInputs);
        mesh.uv = uv;
        mesh.uv2 = GetHeightMap(mesh.vertices);
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        if(autoCalculateNormals)
        {
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
        }

        meshFilter.mesh = mesh;

        if(useCollider)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }

        if(autoScale)
        {
            ScaleAndCenterPlot();
        }
    }

    /// <summary>
    /// Re-plot last-assigned function.
    /// Useful when you have changed "resolution" or "x/z limits".
    /// </summary>
    [ContextMenu("Update Surface")]
    public void UpdateSurface()
    {
        if(function is null) throw new Exception("Function to be plotted is not set.");

        Plot(function);
    }

    // --------------------------------------------------------------
  

    // Private methods.
    // --------------------------------------------------------------

    // Make a new mesh object.
    private void CreateEmptySurface(string name="Surface")
    {
        surfaceObj = new GameObject(name);
        surfaceObj.transform.SetParent(transform);
        surfaceObj.transform.localScale = Vector3.one;
        meshFilter = surfaceObj.AddComponent<MeshFilter>();
        meshRenderer = surfaceObj.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        if(useCollider)
            meshCollider = surfaceObj.AddComponent<MeshCollider>();
        if(useDynamicBuffer)
            meshFilter.mesh.MarkDynamic();
    }

    // Create a surface mesh from a function.
    // This is NOT used in this script.
    // Only kept for demonstration purpose.
    // Plot() function basically works like this.
    private Mesh CreateMesh(Func<Vector2, float> function)
    {
        Mesh mesh = new Mesh();

        mesh.uv = GetUV(resolution);
        functionInputs = GetFunctionInputs(mesh.uv, xLimits, zLimits);
        mesh.vertices = GetVertices(function, functionInputs);
        mesh.uv2 = GetHeightMap(mesh.vertices);
        mesh.triangles = GetTriangles(resolution);

        return mesh;
    }

    // Get texture coordinates for each vertices.
    // Point (xlim.min, zlim.min) in the Scene coordinates is mapped to
    // Point (0, 0) in the texture coordinates.
    // And, (xlim.max, zlim.max) -> (1, 1).
    private Vector2[] GetUV(Resolution resolution=null)
    {
        if(resolution is null) resolution = this.resolution;

        float[] xLinSpace = LinSpace01(resolution.x);
        float[] zLinSpace = LinSpace01(resolution.z);

        Vector2[] uv = new Vector2[resolution.x * resolution.z];
        int idx = 0;
        for(int i=0; i < resolution.x; i++)
        {
            for(int j=0; j < resolution.z; j++)
            {
                uv[idx] = new Vector2(xLinSpace[i], zLinSpace[j]);
                idx++;
            }
        }

        return uv;
    }

    // Get grid points for inputs to the function.
    private Vector2[] GetFunctionInputs(Limits xlim=null, Limits zlim=null, Resolution resolution=null)
    {
        if(xlim is null) xlim = xLimits;
        if(zlim is null) zlim = zLimits;
        if(resolution is null) resolution = this.resolution;

        float[] xLinSpace = LinSpace(xlim, resolution.x);
        float[] zLinSpace = LinSpace(zlim, resolution.z);

        Vector2[] latticePoints = new Vector2[resolution.x * resolution.z];
        int idx = 0;
        for(int i=0; i < resolution.x; i++)
        {
            for(int j=0; j < resolution.z; j++)
            {
                latticePoints[idx] = new Vector2(xLinSpace[i], zLinSpace[j]);
                idx++;
            }
        }

        return latticePoints;
    }

    // Get grid points for inputs to the function. (use already-computed uv coordinates.)
    private Vector2[] GetFunctionInputs(Vector2[] uv, Limits xlim=null, Limits zlim=null)
    {
        if(xlim is null) xlim = xLimits;
        if(zlim is null) zlim = zLimits;

        Vector2 scale = new Vector2(xlim.max - xlim.min, zlim.max - zlim.min);
        Vector2 offset = new Vector2(xlim.min, zlim.min);

        Vector2[] functionInputs = new Vector2[uv.Length];
        for(int i=0; i < uv.Length; i++)
        {
            functionInputs[i] = uv[i] * scale + offset;
        }

        return functionInputs;
    }

    // Divide a range [limits.min, limit.max] into "num" points.
    // Both ends are included.
    private float[] LinSpace(Limits limits, int num)
    {
        if(num < 2) throw new Exception("Invalid \"num\".");
        float[] result = new float[num];
        float delta = (limits.max - limits.min) / (num - 1);
        for(int i=0; i < num; i++)
            result[i] = delta * i + limits.min;
        result[num - 1] = limits.max;
        return result;
    }

    // Divide a range [0, 1] into "num" points.
    // Both ends are included.
    private float[] LinSpace01(int num)
    {
        if(num < 2) throw new Exception("Invalid \"num\".");
        float[] result = new float[num];
        float delta = 1f / (float)(num - 1);
        for(int i=0; i < num; i++)
            result[i] = delta * i;
        result[num - 1] = 1;
        return result;
    }

    // Compute height for input grid points, and get a list of vertices.
    private Vector3[] GetVertices(Func<Vector2, float> function, Vector2[] inputs)
    {
        Vector3[] vertices = new Vector3[inputs.Length];
        for(int i=0; i < inputs.Length; i++)
        {
            vertices[i] = new Vector3(
                inputs[i].x,
                function.Invoke(inputs[i]),
                inputs[i].y
            );
        }
        return vertices;
    }

    // Get uv2 map for color gradients.
    private Vector2[] GetHeightMap(Vector3[] vertices)
    {
        if(autoCalculateHeightLimits) CalculateHeightLimits(vertices);

        Vector2[] heightMap = new Vector2[vertices.Length];
        float scale = 1f / (heightLimits.max - heightLimits.min);
        for(int i=0; i<vertices.Length; i++)
        {
            float normalizedHeight = Clamp01((vertices[i].y - heightLimits.min) * scale);
            heightMap[i] = new Vector2(normalizedHeight, vertices[i].z);
        }

        return heightMap;
    }

    private void CalculateHeightLimits(Vector3[] vertices)
    {
        heightLimits.min = float.MaxValue;
        heightLimits.max = float.MinValue;
        foreach(Vector3 vertex in vertices)
        {
            heightLimits.min = Mathf.Min(heightLimits.min, vertex.y);
            heightLimits.max = Mathf.Max(heightLimits.max, vertex.y);
        }
    }

    private float Clamp01(float x)
    {
        return Mathf.Max(0f, Mathf.Min(x, 1f));
    }

    // Get triangles for the surface mesh.
    private int[] GetTriangles(Resolution resolution)
    {
        int squareCount = (resolution.x - 1) * (resolution.z - 1);
        int[] triangles = new int[squareCount * 6];

        // 2 triangles in a square.
        int[] trianglesInSquare = new int[6]
        {
            0, 1, resolution.z,
            1, resolution.z + 1, resolution.z
        };

        int idx = 0;
        for(int x=1; x < resolution.x; x++)
        {
            for(int z=1; z < resolution.z; z++)
            {
                for(int i=0; i < 6; i++)
                {
                    triangles[idx] = trianglesInSquare[i];
                    trianglesInSquare[i]++;
                    idx++;
                }
            }

            for(int i=0; i < 6; i++)
            {
                trianglesInSquare[i]++;
            }
        }

        return triangles;
    }

    // Check if input grid points should be changed from the last plot.
    private bool IsGridChanged()
    {
        if(lastXLimits is null || lastZLimits is null || resolution is null)
        {
            InitGridInfo();
            return true;
        }
        if(xLimits.min != lastXLimits.min) return true;
        if(xLimits.max != lastXLimits.max) return true;
        if(zLimits.min != lastZLimits.min) return true;
        if(zLimits.max != lastZLimits.max) return true;
        if(resolution.x != lastResolution.x) return true;
        if(resolution.z != lastResolution.z) return true;
        return false;
    }

    // Record information of the grid.
    private void UpdateGridInfo()
    {
        if(lastXLimits is null || lastZLimits is null || resolution is null)
            InitGridInfo();
        lastXLimits.min = xLimits.min;
        lastXLimits.max = xLimits.max;
        lastZLimits.min = zLimits.min;
        lastZLimits.max = zLimits.max;
        lastResolution.x = resolution.x;
        lastResolution.z = resolution.z;
    }

    // Initialize information of the grid.
    private void InitGridInfo()
    {
        lastXLimits = new Limits();
        lastZLimits = new Limits();
        lastResolution = new Resolution();
    }

    // Scales the plot to the size of (20, 10, 20) and places it in the center of the object.
    private void ScaleAndCenterPlot()
    {
        Vector3 targetScale = new Vector3(20f, 10f, 20f);
        Vector3 targetPosition = new Vector3(0f, 5f, 0f);

        Vector3 scale = new Vector3();
        Vector3 postion = new Vector3();

        scale.x = targetScale.x / (xLimits.max - xLimits.min);
        scale.y = targetScale.y / (heightLimits.max - heightLimits.min);
        scale.z = targetScale.z / (zLimits.max - zLimits.min);

        postion.x = (xLimits.max + xLimits.min) / 2f;
        postion.y = (heightLimits.max + heightLimits.min) / 2f;
        postion.z = (zLimits.max + zLimits.min) / 2f;
        for(int i = 0; i < 3; i++)
        {
            postion[i] = targetPosition[i] - postion[i] * scale[i];
        }

        surfaceObj.transform.localScale = scale;
        surfaceObj.transform.localPosition = postion;
    }

    // --------------------------------------------------------------
}
