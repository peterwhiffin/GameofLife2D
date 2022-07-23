using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


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
    public GameObject currentOverlap;
    public List<GameObject> citizens;
    public List<Vector3> liveCells;
    public Selector selectorScript;
    public Camera camera;

    public bool overlapping = false;
    public bool occupied = false;
    public bool execute = false;

    public float timer = 1f;
    public float zoomSpeed;
    public float gameSpeed;
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

        camera.orthographicSize -= mouseWheel * zoomSpeed;

        if (camera.orthographicSize < 0f)
            camera.orthographicSize = 0f;

        selectorCube.position = new Vector3(Mathf.Floor(Mathf.Abs(mouseWorldPos.x)) * Mathf.Sign(mouseWorldPos.x), Mathf.Floor(Mathf.Abs(mouseWorldPos.y)) * Mathf.Sign(mouseWorldPos.y), 0);

        if (Input.GetMouseButton(0))
        {
            overlapping = selectorScript.CheckOverlap();

            if (!overlapping)
            {
                Debug.Log(overlapping);
                GameObject prefab;

                prefab = Instantiate(placedCube, selectorCube.position, Quaternion.identity);                             
                prefab.tag = "Alive";
                prefab.name = "NewBirth";
                
                citizens.Add(prefab);
                overlapping = true;
            }
        }

        if (Input.GetMouseButton(1))
        {
            overlapping = selectorScript.CheckOverlap();

            if (overlapping)
            {
                citizens.Remove(currentOverlap);
                Destroy(currentOverlap);
                overlapping = false;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
            execute = true;
        

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
        foreach (GameObject citizen in citizens)
        {
            Collider2D[] hitColliders;

            hitColliders = Physics2D.OverlapBoxAll(new Vector2(citizen.transform.position.x, citizen.transform.position.y), new Vector2(1.2f, 1.2f), 0f);
            if (hitColliders.Length > 4)
            {
                citizen.tag = "Dead";
            }
            else if (hitColliders.Length < 3)
            {
                citizen.tag = "Dead";
            }

            hitColliders = Physics2D.OverlapBoxAll(new Vector2(citizen.transform.position.x + 1, citizen.transform.position.y), new Vector2(1.2f, 1.2f), 0f);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.transform.position == new Vector3(citizen.transform.position.x + 1, citizen.transform.position.y, citizen.transform.position.z))
                {
                    occupied = true;
                }
            }
            if (hitColliders.Length == 3 && !occupied)
            {
                liveCells.Add(new Vector3(citizen.transform.position.x + 1, citizen.transform.position.y, citizen.transform.position.z));
            }
            else
                occupied = false;

            hitColliders = Physics2D.OverlapBoxAll(new Vector2(citizen.transform.position.x - 1, citizen.transform.position.y), new Vector2(1.2f, 1.2f), 0f);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.transform.position == new Vector3(citizen.transform.position.x - 1, citizen.transform.position.y, citizen.transform.position.z))
                {
                    occupied = true;
                }
            }
            if (hitColliders.Length == 3 && !occupied)
            {
                liveCells.Add(new Vector3(citizen.transform.position.x - 1, citizen.transform.position.y, citizen.transform.position.z));
            }
            else
                occupied = false;

            hitColliders = Physics2D.OverlapBoxAll(new Vector2(citizen.transform.position.x, citizen.transform.position.y + 1), new Vector2(1.2f, 1.2f), 0f);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.transform.position == new Vector3(citizen.transform.position.x, citizen.transform.position.y + 1, citizen.transform.position.z))
                {
                    occupied = true;
                }
            }
            if (hitColliders.Length == 3 && !occupied)
            {
                liveCells.Add(new Vector3(citizen.transform.position.x, citizen.transform.position.y + 1, citizen.transform.position.z));
            }
            else
                occupied = false;

            hitColliders = Physics2D.OverlapBoxAll(new Vector2(citizen.transform.position.x, citizen.transform.position.y - 1), new Vector2(1.2f, 1.2f), 0f);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.transform.position == new Vector3(citizen.transform.position.x, citizen.transform.position.y - 1, citizen.transform.position.z))
                {
                    occupied = true;
                }
            }
            if (hitColliders.Length == 3 && !occupied)
            {
                liveCells.Add(new Vector3(citizen.transform.position.x, citizen.transform.position.y - 1, citizen.transform.position.z));
            }
            else
                occupied = false;

            hitColliders = Physics2D.OverlapBoxAll(new Vector2(citizen.transform.position.x + 1, citizen.transform.position.y + 1), new Vector2(1.2f, 1.2f), 0f);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.transform.position == new Vector3(citizen.transform.position.x + 1, citizen.transform.position.y + 1, citizen.transform.position.z))
                {
                    occupied = true;
                }
            }
            if (hitColliders.Length == 3 && !occupied)
            {
                liveCells.Add(new Vector3(citizen.transform.position.x + 1, citizen.transform.position.y + 1, citizen.transform.position.z));
            }
            else
                occupied = false;

            hitColliders = Physics2D.OverlapBoxAll(new Vector2(citizen.transform.position.x - 1, citizen.transform.position.y - 1), new Vector2(1.2f, 1.2f), 0f);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.transform.position == new Vector3(citizen.transform.position.x - 1, citizen.transform.position.y - 1, citizen.transform.position.z))
                {
                    occupied = true;
                }
            }
            if (hitColliders.Length == 3 && !occupied)
            {
                liveCells.Add(new Vector3(citizen.transform.position.x - 1, citizen.transform.position.y - 1, citizen.transform.position.z));
            }
            else
                occupied = false;

            hitColliders = Physics2D.OverlapBoxAll(new Vector2(citizen.transform.position.x + 1, citizen.transform.position.y - 1), new Vector2(1.2f, 1.2f), 0f);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.transform.position == new Vector3(citizen.transform.position.x + 1, citizen.transform.position.y - 1, citizen.transform.position.z))
                {
                    occupied = true;
                }
            }
            if (hitColliders.Length == 3 && !occupied)
            {
                liveCells.Add(new Vector3(citizen.transform.position.x + 1, citizen.transform.position.y - 1, citizen.transform.position.z));
            }
            else
                occupied = false;

            hitColliders = Physics2D.OverlapBoxAll(new Vector2(citizen.transform.position.x - 1, citizen.transform.position.y + 1), new Vector2(1.2f, 1.2f), 0f);
            foreach (Collider2D collider in hitColliders)
            {
                if (collider.transform.position == new Vector3(citizen.transform.position.x - 1, citizen.transform.position.y + 1, citizen.transform.position.z))
                {
                    occupied = true;
                }
            }
            if (hitColliders.Length == 3 && !occupied)
            {
                liveCells.Add(new Vector3(citizen.transform.position.x - 1, citizen.transform.position.y + 1, citizen.transform.position.z));
            }
            else
                occupied = false;
        }

        UpdateCells();
    }

    public void UpdateCells()
    {
        foreach (GameObject citizen in citizens.ToList())
        {
            if (citizen.CompareTag("Dead"))
            {
                citizens.Remove(citizen);
                Destroy(citizen);
            }
        }

        liveCells = liveCells.Distinct().ToList();

        foreach (Vector3 liveCell in liveCells)
        {
            GameObject prefab;
            prefab = Instantiate(placedCube, liveCell, Quaternion.identity);
            prefab.name = "NewBirth";
            citizens.Add(prefab);
        }

        liveCells.Clear();
    }

    public void ResetGame()
    {
        foreach (GameObject citizen in citizens.ToList())
        {
            citizens.Remove(citizen);
            Destroy(citizen);
        }

        execute = false;
        timer = 0;
        liveCells.Clear();
    }
}
