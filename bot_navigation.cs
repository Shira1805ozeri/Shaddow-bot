using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SimpleHeatmap : MonoBehaviour
{
    public int rows = 20;
    public int cols = 20;
    public float cellSize = 1f;
    public int numViewPoints = 1000;
    public float viewMinHeight = 7f;
    public float viewMaxHeight = 17f;
    public float sampleHeightOffset = 0.1f;
    public Transform robot;
    public Vector2Int targetCell = new Vector2Int(15, 15);
    public GameObject cellPrefab;

    private float[,] heatmap;
    private List<Vector3> viewPoints;
    private GameObject[,] gridCells;
    private Camera robotCamera;
    private List<GameObject> blockers;

    void Start()
    {
        heatmap = new float[rows, cols];
        viewPoints = new List<Vector3>();
        gridCells = new GameObject[rows, cols];
        blockers = new List<GameObject>();
        robotCamera = Camera.main;

        GenerateBlockers();
        GenerateViewPoints();
        CreateGrid();
        StartCoroutine(UpdateHeatmapAndNavigate());
    }

    void GenerateBlockers()
    {
        foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
        {
            if (obj.name.Contains("desk") || obj.name.Contains("chair"))
            {
                if (obj.GetComponent<Collider>() == null)
                {
                    var mc = obj.AddComponent<MeshCollider>();
                    mc.convex = true;
                }
                blockers.Add(obj);
            }
        }
    }

    void GenerateViewPoints()
    {
        int attempts = 0;
        while (viewPoints.Count < numViewPoints && attempts < numViewPoints * 10)
        {
            attempts++;
            float x = Random.Range(-rows * cellSize / 2f, rows * cellSize / 2f);
            float y = Random.Range(viewMinHeight, viewMaxHeight);
            float z = Random.Range(-cols * cellSize / 2f, cols * cellSize / 2f);
            Vector3 point = new Vector3(x, y, z);
            if (IsVisibleToCamera(point)) viewPoints.Add(point);
        }
    }

    void CreateGrid()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                Vector3 pos = GridToWorld(x, z);
                GameObject cell = cellPrefab != null ?
                    Instantiate(cellPrefab, pos, Quaternion.identity, transform) :
                    GameObject.CreatePrimitive(PrimitiveType.Cube);

                cell.transform.position = pos;
                cell.transform.localScale = new Vector3(cellSize, 0.01f, cellSize);
                cell.transform.parent = transform;

                Destroy(cell.GetComponent<Collider>());
                gridCells[x, z] = cell;
            }
        }
    }

    IEnumerator UpdateHeatmapAndNavigate()
    {
        while (true)
        {
            ComputeHeatmap();

            var path = FindPath(WorldToGrid(robot.position), targetCell);
            if (path != null && path.Count > 1)
            {
                for (int i = 1; i < path.Count; i++)
                {
                    Vector2Int cell = path[i];
                    Vector3 startPos = robot.position;
                    Vector3 endPos = GridToWorld(cell.x, cell.y) + Vector3.up * 0.5f;
                    float elapsed = 0f;
                    float moveDuration = 0.2f;

                    while (elapsed < moveDuration)
                    {
                        robot.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                    robot.position = endPos;

                    // צביעה בכחול רק אם המשבצת נראתה (heatmap > 0)
                    if (cell == targetCell || heatmap[cell.x, cell.y] > 0f)
                    {
                        gridCells[cell.x, cell.y].GetComponent<Renderer>().material.color = Color.blue;
                    }

                    if (cell == targetCell)
                        yield break;
                }
            }
            yield return null;
        }
    }

    void ComputeHeatmap()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int z = 0; z < cols; z++)
            {
                Vector3 pos = GridToWorld(x, z) + Vector3.up * sampleHeightOffset;
                if (!IsVisibleToCamera(pos)) continue;

                int visible = 0;
                foreach (var vp in viewPoints)
                {
                    Vector3 dir = pos - vp;
                    if (Physics.Raycast(vp, dir.normalized, out RaycastHit hit, dir.magnitude))
                    {
                        if (hit.collider.gameObject == gridCells[x, z])
                            visible++;
                    }
                    else
                        visible++;
                }

                float score = (float)visible / viewPoints.Count;
                heatmap[x, z] = score;
                Color color = Color.Lerp(Color.green, Color.red, score);
                if (new Vector2Int(x, z) == targetCell)
                    color = Color.blue;
                gridCells[x, z].GetComponent<Renderer>().material.color = color;
            }
        }
    }

    Vector3 GridToWorld(int x, int z)
    {
        return new Vector3((x - rows / 2f) * cellSize, 0, (z - cols / 2f) * cellSize);
    }

    Vector2Int WorldToGrid(Vector3 pos)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(pos.x / cellSize + rows / 2f), 0, rows - 1);
        int z = Mathf.Clamp(Mathf.RoundToInt(pos.z / cellSize + cols / 2f), 0, cols - 1);
        return new Vector2Int(x, z);
    }

    bool IsVisibleToCamera(Vector3 point)
    {
        if (robotCamera == null) return false;
        Vector3 vp = robotCamera.WorldToViewportPoint(point);
        return vp.z > 0 && vp.x >= 0 && vp.x <= 1 && vp.y >= 0 && vp.y <= 1;
    }

    List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        var open = new PriorityQueue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();
        var fScore = new Dictionary<Vector2Int, float>();

        open.Enqueue(start, 0);
        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);

        while (open.Count > 0)
        {
            Vector2Int current = open.Dequeue();

            if (current == goal)
            {
                var path = new List<Vector2Int> { current };
                while (cameFrom.ContainsKey(current))
                {
                    current = cameFrom[current];
                    path.Add(current);
                }
                path.Reverse();
                return path;
            }

            foreach (Vector2Int dir in new[] {
                Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int neighbor = current + dir;
                if (neighbor.x < 0 || neighbor.x >= rows || neighbor.y < 0 || neighbor.y >= cols)
                    continue;

                float tentative = gScore[current] + heatmap[neighbor.x, neighbor.y];
                if (!gScore.ContainsKey(neighbor) || tentative < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentative;
                    fScore[neighbor] = tentative + Heuristic(neighbor, goal);
                    open.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }
        return null;
    }

    float Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    public class PriorityQueue<T>
    {
        private List<(T item, float priority)> elements = new();

        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            elements.Add((item, priority));
        }

        public T Dequeue()
        {
            int bestIndex = 0;
            for (int i = 1; i < elements.Count; i++)
                if (elements[i].priority < elements[bestIndex].priority)
                    bestIndex = i;

            T bestItem = elements[bestIndex].item;
            elements.RemoveAt(bestIndex);
            return bestItem;
        }
    }
}
