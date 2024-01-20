using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Jobs;
using Unity.Collections;

public class Player : MonoBehaviour
{
    private float vertical;
    private float horizontal;
    private float mouseWheel;
    private float mouseClick;

    private Vector3 mouseWorldPos;
    private Vector3 mouseScreenPos;
    private Vector3 currentCell;
    public Transform selectorCube;
    public GameObject placedCube;
    public GameObject _cellPrefab;
    public GameObject currentOverlap;
    public List<GameObject> citizens;
    public List<Vector3> liveCells;
    public Selector selectorScript;
    public Camera _camera;
    //public HashSet<Vector2Int> _checkedCells = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> _deadCellsToCheck = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> _newCitizens = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> _dyingCells = new HashSet<Vector2Int>();
    public HashSet<GameObject> _cellPool = new HashSet<GameObject>();
    public HashSet<Vector2Int> _liveCells = new HashSet<Vector2Int>();

    public Dictionary<Vector2Int, GameObject> _citizens = new Dictionary<Vector2Int, GameObject>();
    public Dictionary<Vector2Int, bool> _checkedCells = new Dictionary<Vector2Int, bool>();

    public bool overlapping = false;
    public bool occupied = false;
    public bool execute = false;

    public float timer = 1f;
    public float zoomSpeed;
    public float gameSpeed;
    public float _cachedMouseY;
    public bool _shiftHeld;

    private void Start()
    {
        citizens = new List<GameObject>();
    }

    void Update()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        mouseClick = Input.GetAxis("Fire1");

        

        transform.Translate(Vector3.up * vertical);
        transform.Translate(Vector3.right * horizontal);
        mouseScreenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);

        mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        _camera.orthographicSize -= mouseWheel * zoomSpeed;

        if (_camera.orthographicSize < 0f)
            _camera.orthographicSize = 0f;

        if (_shiftHeld)
        {
            selectorCube.position = new Vector3(Mathf.Floor(Mathf.Abs(mouseWorldPos.x)) * Mathf.Sign(mouseWorldPos.x), _cachedMouseY, 0);
        }
        else
            selectorCube.position = new Vector3(Mathf.Floor(Mathf.Abs(mouseWorldPos.x)) * Mathf.Sign(mouseWorldPos.x), Mathf.Floor(Mathf.Abs(mouseWorldPos.y)) * Mathf.Sign(mouseWorldPos.y), 0);

        if (Input.GetMouseButton(0))
        {
            if (!_citizens.ContainsKey(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y)))
            {
                _citizens.Add(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y), GetPooledCell(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y)));
                _liveCells.Add(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y));
            }
        }

        if (Input.GetMouseButton(1))
        {
            if (_citizens.ContainsKey(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y)))
            {
                ReturnCellToPool(_citizens[new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y)]);
                _citizens.Remove(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y));
                _liveCells.Remove(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y));
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            execute = true;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _cachedMouseY = selectorCube.position.y;
            _shiftHeld = true;
        }
        else
            _shiftHeld = false;



        if (Input.GetKey(KeyCode.E))
            gameSpeed -= .001f;

        if (gameSpeed < 0f)
            gameSpeed = 0f;

        if (Input.GetKey(KeyCode.Q))
            gameSpeed += .001f;



        if (execute && timer > gameSpeed)
        {
            EvaluateCells();
            timer = 0;
        }

        timer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Escape))
            ResetGame();             
    }

    public void EvaluateCells()
    {
        _deadCellsToCheck.Clear();      
        _newCitizens.Clear();
        _dyingCells.Clear();

        foreach (var cell in _liveCells)
        {
            int neighbors = CheckLiveNeighbors(cell);

            if (!ApplyLiveRules(neighbors))
                _dyingCells.Add(cell);
        }

        foreach (var deadCell in _deadCellsToCheck)
        {
            int neighbors = CheckDeadNeighbors(deadCell);

            if (ApplyDeadRules(neighbors))
                _newCitizens.Add(deadCell);
        }

        UpdateCells();
    }

    public int CheckLiveNeighbors(Vector2Int pos)
    {
        int neighbors = 0;
        for (int x = pos.x - 1; x < pos.x + 2; x++)
        {
            for (int y = pos.y + 1; y > pos.y - 2; y--)
            {
                Vector2Int posToCheck = new(x, y);

                if (posToCheck == pos)
                    continue;
                else if (_deadCellsToCheck.Contains(posToCheck))
                    continue;
                else if (_liveCells.Contains(posToCheck))
                    neighbors++;                
                else
                    _deadCellsToCheck.Add(posToCheck);
            }
        }

        return neighbors;
    }

    public int CheckDeadNeighbors(Vector2Int pos)
    {
        int neighbors = 0;
        for (int x = pos.x - 1; x < pos.x + 2; x++)
        {
            for (int y = pos.y + 1; y > pos.y - 2; y--)
            {
                Vector2Int posToCheck = new(x, y);

                if (posToCheck == pos)
                    continue;
                else if (_deadCellsToCheck.Contains(posToCheck))
                    continue;
                else if (_liveCells.Contains(posToCheck))
                {
                    neighbors++;

                    if (neighbors == 4)
                        return neighbors;
                }
            }
        }

        return neighbors;
    }

    public bool ApplyLiveRules(int neighbors, bool liveCell = true)
    {
        return (neighbors == 3 || neighbors == 2);
    }

    public bool ApplyDeadRules(int neighbors)
    {
        return neighbors == 3;
    }

    public void UpdateCells()
    {
        foreach (var cell in _dyingCells)
        {
            ReturnCellToPool(_citizens[cell]);
            _citizens.Remove(cell);
            _liveCells.Remove(cell);
        }

        foreach (var cell in _newCitizens)
        {
            _citizens.Add(cell, GetPooledCell(new Vector2(cell.x, cell.y)));
            _liveCells.Add(cell);
        }
    }

    public void ResetGame()
    {
        foreach (var citizen in _citizens)
        {
            Destroy(citizen.Value);
        }

        execute = false;
        timer = 0;
        _citizens.Clear();
        _liveCells.Clear();
    }

    public GameObject GetPooledCell(Vector2 pos)
    {
        GameObject cell = null;

        if(_cellPool.Count > 0)
        {
            cell = _cellPool.FirstOrDefault();
            _cellPool.Remove(cell);
            cell.transform.position = pos;
            cell.GetComponent<Renderer>().enabled = true;
        }
        else
        {
            cell = Instantiate(_cellPrefab, pos, Quaternion.identity);
        }

        return cell;
    }

    public void ReturnCellToPool(GameObject cell)
    {
        _cellPool.Add(cell);
        cell.GetComponent<Renderer>().enabled = false;
    }
}
