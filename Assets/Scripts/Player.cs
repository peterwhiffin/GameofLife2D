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

    public Dictionary<Vector2Int, GameObject> _citizens = new Dictionary<Vector2Int, GameObject>();


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
                _citizens.Add(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y), GetPooledCell(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y)));        
        }

        if (Input.GetMouseButton(1))
        {
            if (_citizens.ContainsKey(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y)))
            {
                ReturnCellToPool(_citizens[new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y)]);
                _citizens.Remove(new Vector2Int((int)selectorCube.position.x, (int)selectorCube.position.y));
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

        foreach (var cell in _citizens)
        {
            int neighbors = CheckNeighbors(cell.Key);

            if (!ApplyLiveRules(neighbors))
                _dyingCells.Add(cell.Key);
        }

        foreach (var deadCell in _deadCellsToCheck)
        {
            int neighbors = CheckNeighbors(deadCell, 4, true);

            if (ApplyDeadRules(neighbors))
                _newCitizens.Add(deadCell);
        }

        UpdateCells();
    }

  
    public int CheckNeighbors(Vector2Int pos, int limit = 4, bool dead = false)
    {
        int neighbors = 0;
        for (int x = pos.x - 1; x < pos.x + 2; x++)
        {
            for (int y = pos.y + 1; y > pos.y - 2; y--)
            {
                Vector2Int posToCheck = new(x, y);

                if (posToCheck != pos)
                {
                    if (_citizens.ContainsKey(posToCheck))
                        neighbors++;
                    else if(!_deadCellsToCheck.Contains(posToCheck) && !dead)
                        _deadCellsToCheck.Add(posToCheck);
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
        }

        foreach(var cell in _newCitizens)
            _citizens.Add(cell, GetPooledCell(new Vector2(cell.x, cell.y)));      
    }

    //public void UpdateCells()
    //{
    //    foreach (GameObject citizen in citizens.ToList())
    //    {
    //        if (citizen.CompareTag("Dead"))
    //        {
    //            citizens.Remove(citizen);
    //            Destroy(citizen);
    //        }
    //    }

    //    liveCells = liveCells.Distinct().ToList();

    //    foreach (Vector3 liveCell in liveCells)
    //    {
    //        GameObject prefab;
    //        prefab = Instantiate(placedCube, liveCell, Quaternion.identity);
    //        prefab.name = "NewBirth";
    //        citizens.Add(prefab);
    //    }

    //    liveCells.Clear();
    //}


    public void ResetGame()
    {
        foreach (var citizen in _citizens)
        {
            Destroy(citizen.Value);
        }

        execute = false;
        timer = 0;
        _citizens.Clear();
    }

    public GameObject GetPooledCell(Vector2 pos)
    {
        GameObject cell = null;

        if(_cellPool.Count > 0)
        {
            cell = _cellPool.FirstOrDefault();
            _cellPool.Remove(cell);
            cell.transform.position = pos;
            cell.SetActive(true);
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
        cell.SetActive(false);
    }
}
