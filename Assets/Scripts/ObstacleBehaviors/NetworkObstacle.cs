﻿using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObstacle : NetworkBehaviour
{
    [Header("Bindings")]
    [SerializeField] private GameScreen screen;

    [SerializeField] private GameManager manager;
    private GameObject sweeper;

    [Header("Generator Settings")]
    [SerializeField] private short reduceHoleBy;

    [SerializeField, Range(1.0f, 2.0f), ContextMenuItem("Randomize", "RandomTolerance")] private float holeTolerance;

    [Header("Runtime Variables")]
    [SerializeField] private float speed;

    [SerializeField] private float speedMultiplier;
    [SerializeField] private float maxSpeed;

    [Header("Synchronized Variables")]
    [SerializeField] private Direction direction;

    [SyncVar(hook = nameof(SetDirection))] private int dirNum;

#if UNITY_EDITOR

    [Header("GUI Options")]
    [SerializeField] private bool showGUI;

    [SerializeField] private Vector2 guiOffset;
#endif
    private readonly SyncList<float> holes = new SyncList<float>();

    /// <summary>
    /// Direction enum used mostly for indicating the direction of the sweepers.
    /// </summary>
    public enum Direction
    {
        Up, Left, Right
    }

    /// <summary>
    /// This function will generate new float points as hole points and new sweeper direction.
    /// </summary>
    /// <returns>True, if the function runs properly</returns>
    [Server]
    private bool GenerateObstacle()
    {
        if (!manager)
        {
            manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
        }
        if (!screen)
        {
            screen = GameObject.FindGameObjectWithTag("screen").GetComponent<GameScreen>();
        }
        holes.Clear();
        //RNG: determine where obstacle will originate
        {
            var num = Random.Range(0f, 1f);
            var holeCount = Mathf.Clamp(Random.Range(manager.PlayerCount - reduceHoleBy, manager.PlayerCount), 1, manager.PlayerCount);
            var num2 = screen.ScreenHeight_inWorldUnits / holeCount;
            //Chance 25% right
            if (num < 0.25f)
            {
                direction = Direction.Right;
                SetDirection(dirNum, 2);
            }
            //Chance 25% left
            else if (num < 0.5f)
            {
                direction = Direction.Left;
                SetDirection(dirNum, 1);
            }
            //Chance 50% up
            else
            {
                num2 = screen.ScreenWidth_inWorldUnits / holeCount;
                direction = Direction.Up;
                SetDirection(dirNum, 0);
            }
            //Generate a list of holes position
            {
                var temp = new List<float>();
                for (short i = 0; i < holeCount; i++)
                {
                    if (direction != Direction.Up)
                    {
                        temp.Add(Random.Range(i * num2, (i + 1) * num2) - screen.ScreenHeight_inWorldUnits / 2);
                    }
                    else
                    {
                        temp.Add(Random.Range(i * num2, (i + 1) * num2) - screen.ScreenWidth_inWorldUnits / 2);
                    }
                }
                temp.Sort();
                for (short i = 0; i < holeCount; i++)
                {
                    holes.Add(temp[i]);
                }
            }
        }
        return true;
    }

    /// <summary>
    /// This function will instantiate a set of sweepers with conditions generated by the <c>GenerateObstacle()</c> function.
    /// </summary>
    /// <returns>True, if the function runs properly</returns>
    [Server]
    private bool CreateObstacle()
    {
        if (!sweeper)
        {
            sweeper = GameObject.FindGameObjectWithTag("networkManager").GetComponent<MyNetworkManager>().spawnPrefabs[1];
        }
        var size = sweeper.GetComponent<SpriteRenderer>().bounds.size;
        Vector2 vel = Vector2.zero;
        //First obstacle
        {
            var instance = Instantiate(sweeper);
            instance.transform.localScale = new Vector3(1, (screen.Corner_TopRight.y - (holes[holes.Count - 1] + holeTolerance)) / size.y, 1);
            switch (direction)
            {
                case Direction.Left:
                    instance.transform.localPosition = new Vector3(screen.Corner_TopRight.x + 1f, holes[holes.Count - 1] + holeTolerance);
                    vel = new Vector2(-speed, 0);
                    break;

                case Direction.Right:
                    instance.transform.localPosition = new Vector3(screen.Corner_BottomLeft.x - 1f, holes[holes.Count - 1] + holeTolerance);
                    vel = new Vector2(speed, 0);
                    break;

                case Direction.Up:
                    instance.transform.localPosition = new Vector3(holes[holes.Count - 1] + holeTolerance, screen.Corner_BottomLeft.y - 1f);
                    instance.transform.localScale = new Vector3((screen.Corner_TopRight.x - (holes[holes.Count - 1] + holeTolerance)) / size.x, 1, 1);
                    vel = new Vector2(0, speed);
                    break;
            }
            instance.GetComponent<ObstacleScript>().SetVelocity(vel);
            instance.transform.parent = transform;
            NetworkServer.Spawn(instance);
        }

        //first+1 -> holeCount-1 obstacle
        if (holes.Count > 1)
        {
            for (int i = holes.Count - 2; i > -1; i--)
            {
                var instance = Instantiate(sweeper);
                instance.transform.localScale = new Vector3(1, (holes[i + 1] - holeTolerance - (holes[i] + holeTolerance)) / size.y, 1);
                switch (direction)
                {
                    case Direction.Left:
                        instance.transform.localPosition = new Vector3(screen.Corner_TopRight.x + 1f, holes[i] + holeTolerance);
                        vel = new Vector2(-speed, 0);
                        break;

                    case Direction.Right:
                        instance.transform.localPosition = new Vector3(screen.Corner_BottomLeft.x - 1f, holes[i] + holeTolerance);
                        vel = new Vector2(speed, 0);
                        break;

                    case Direction.Up:
                        instance.transform.localPosition = new Vector3(holes[i] + holeTolerance, screen.Corner_BottomLeft.y - 1f);
                        instance.transform.localScale = new Vector3((holes[i + 1] - holeTolerance - (holes[i] + holeTolerance)) / size.x, 1, 1);
                        vel = new Vector2(0, speed);
                        break;
                }
                instance.GetComponent<ObstacleScript>().SetVelocity(vel);
                instance.transform.parent = transform;
                NetworkServer.Spawn(instance);
            }
        }

        //Last obstacle
        {
            var instance = Instantiate(sweeper);
            instance.transform.localScale = new Vector3(1, (holes[0] - holeTolerance - screen.Corner_BottomLeft.y) / size.y, 1);
            switch (direction)
            {
                case Direction.Left:
                    instance.transform.localPosition = new Vector3(screen.Corner_TopRight.x + 1f, screen.Corner_BottomLeft.y);
                    vel = new Vector2(-speed, 0);
                    break;

                case Direction.Right:
                    instance.transform.localPosition = new Vector3(screen.Corner_BottomLeft.x - 1f, screen.Corner_BottomLeft.y);
                    vel = new Vector2(speed, 0);
                    break;

                case Direction.Up: //Basically, down
                    instance.transform.localPosition = new Vector3(screen.Corner_BottomLeft.x, screen.Corner_BottomLeft.y - 1f);
                    instance.transform.localScale = new Vector3((holes[0] - holeTolerance - screen.Corner_BottomLeft.x) / size.x, 1, 1);
                    vel = new Vector2(0, speed);
                    break;
            }
            instance.GetComponent<ObstacleScript>().SetVelocity(vel);
            instance.transform.parent = transform;
            NetworkServer.Spawn(instance);
        }
        return true;
    }

    /// <summary>
    /// Creates a platform for the players. All connected client, including <c>localPlayer</c> should ran this function once.
    /// </summary>
    public void CreatePlatform()
    {
        var instance = Instantiate(GameObject.FindGameObjectWithTag("networkManager").GetComponent<MyNetworkManager>().spawnPrefabs[0]);
        instance.transform.position = new Vector3(0f, screen.Corner_BottomLeft.y - 0.5f, 0f);
        instance.transform.localScale = new Vector3(2f, 3f, 1f);
    }

    public override void OnStartClient()
    {
        CreatePlatform();
        base.OnStartClient();
    }

    public override void OnStopClient()
    {
        var temp = GameObject.FindGameObjectsWithTag("Ground");
        foreach (GameObject val in temp)
        {
            Destroy(val);
        }
        base.OnStopClient();
    }

    private void Awake()
    {
        if (!manager)
        {
            try
            {
                manager = GameObject.FindGameObjectWithTag("Manager").GetComponent<GameManager>();
            }
            catch
            {
                Debug.LogError("Manager for Obstacle not found");
            }
        }
        if (!sweeper)
        {
            sweeper = GameObject.FindGameObjectWithTag("networkManager").GetComponent<MyNetworkManager>().spawnPrefabs[1];
        }
    }

    private void Start()
    {
        if (isServer) GenerateObstacle();
    }

    private void Update()
    {
        if (manager.Running)
        {
            if (transform.childCount < 1)
            {
                if (isServer)
                {
                    GenerateObstacle();
                    CreateObstacle();
                }
            }
            if (speed < maxSpeed)
            {
                speed += speedMultiplier / 100 * Time.deltaTime;
            }
        }
    }

    private void OnGUI()
    {
        if (!showGUI) return;
        GUILayout.BeginArea(new Rect(10 + guiOffset.x, 40 + guiOffset.y, 215, 9999));
        if (GUILayout.Button("Generate Obstacle"))
        {
            Debug.Log("Obstacle Generated? = " + GenerateObstacle().ToString());
        }
        if (GUILayout.Button("Create Obstacle"))
        {
            Debug.Log("Obstacle Created? = " + CreateObstacle().ToString());
        }
        GUILayout.Label("Number of holes = " + holes.Count);
        GUILayout.EndArea();
    }

    private void OnDrawGizmos()
    {
        foreach (float item in holes)
        {
            if (direction != Direction.Up)
            {
                Gizmos.DrawSphere(new Vector3(0, item), 0.1f);
            }
            else
            {
                Gizmos.DrawSphere(new Vector3(item, 0), 0.1f);
            }
        }
    }

    /// <summary>
    /// Hook function for <c>dirNum</c>.
    /// </summary>
    /// <param name="old">Old value</param>
    /// <param name="_new">New value</param>
    private void SetDirection(int old, int _new)
    {
        dirNum = _new;
        switch (dirNum)
        {
            case 0:
                direction = Direction.Up;
                break;

            case 1:
                direction = Direction.Left;
                break;

            case 2:
                direction = Direction.Right;
                break;
        }
    }
}