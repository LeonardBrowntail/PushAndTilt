﻿using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class NetPlayerScript : NetworkBehaviour
{
    private MyNetworkManager netManager;
    private GameScreen screen;
    private TextMesh nameLabel;
    private Rigidbody2D rb;
    private float charRadius;
    [SerializeField] private TextMesh readyLabel;
    [SerializeField] private TextMesh playerNameLabel;

    #region Jumping Fields

    [Header("Player jump charge parameters")]
    [SerializeField]
    private GameObject jumpHeightIndicator;

    [SerializeField]
    private float chargeHeight;

    [SerializeField]
    private float chargeLimit;

    [SerializeField]
    private float chargeMultiplier;

    [SerializeField]
    private float charge;

    [SerializeField]
    private bool isCharging;

    #endregion Jumping Fields

    #region Movement Fields

    [Header("Player speed parameters")]
    [SerializeField]
    private float speed;

    [SerializeField]
    private float acceleration;

    [SerializeField]
    private float speedLimit;

    [SerializeField, Range(0.01f, 1f)]
    private float restitution;

    #endregion Movement Fields

    #region Network Managed Fields

    [Header("Network Managed Fields")]
    [SerializeField, SyncVar(hook = nameof(SetPlayerName))] private string playerName;

    [SerializeField, SyncVar(hook = nameof(SetReadyState))] private bool ready;
    [SerializeField, SyncVar(hook = nameof(SetPlayerAliveStatus))] private bool active;

    #endregion Network Managed Fields

    #region Properties

    public bool isAlive => active;
    public bool isReady => ready;

    #endregion Properties

#if !UNITY_EDITOR
    private float dirX, dirY;
#endif

    private void Awake()
    {
        if (!(netManager = FindObjectOfType<MyNetworkManager>()))
        {
            Debug.LogError("NetPlayerScript.cs/Awake(): netManager is missing!");
        }
        if (!(screen = GameObject.FindGameObjectWithTag("screen").GetComponent<GameScreen>()))
        {
            Debug.LogError("NetPlayerScript.cs/Awake(): screen is missing!");
        }
        if (!(jumpHeightIndicator = transform.GetChild(0).gameObject))
        {
            Debug.LogError("NetPlayerScript.cs/Awake(): jumpHeightIndicator is missing!");
        }
        if (!(rb = GetComponent<Rigidbody2D>()))
        {
            Debug.LogError("NetPlayerScript.cs/Awake(): rb is missing!");
        }
        if (!(readyLabel = transform.GetChild(0).GetComponent<TextMesh>()))
        {
            Debug.LogError("NetPlayerScript.cs/Awake(): readySprite is missing!");
        }
        if (!(nameLabel = transform.GetChild(1).GetComponent<TextMesh>()))
        {
            Debug.LogError("NetPlayerScript.cs/Awake(): nameLabel is missing!");
        }
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            chargeLimit = CalculateMaxVelocity();
            isCharging = false;
            SetPlayerAliveStatus(false, false);
            SetReadyVisibility(false);
            SetPlayerName(string.Empty, netManager.PlayerName);
            SetReadyState(false, false);
            charRadius = GetComponent<SpriteRenderer>().bounds.size.x / 2;
        }
    }

    private void FixedUpdate()
    {
        if (active)
        {
            TouchHandler();
            HandleMovements();
        }
    }

    /// <summary>
    /// Calculates the maximum height of player's jump.
    /// </summary>
    /// <returns>float value containing the max jump speed</returns>
    private float CalculateMaxVelocity()
    {
        return Mathf.Sqrt(2 * (Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale) * (screen.Corner_TopRight.y - transform.position.y));
    }

    /// <summary>
    /// Handles the player movement
    /// </summary>
    private void HandleMovements()
    {
        if (isLocalPlayer)
        {
            //Horizontal Movements
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.D) ^ Input.GetKey(KeyCode.A))
            {
                if (Input.GetKey(KeyCode.D))
                {
                    if (speed < speedLimit) speed += Time.deltaTime * acceleration;
                }
                if (Input.GetKey(KeyCode.A))
                {
                    if (speed > -speedLimit) speed -= Time.deltaTime * acceleration;
                }
            }
            else
            {
                speed = 0;
            }
            rb.velocity = new Vector2(rb.velocity.x + speed, rb.velocity.y);
            //X pos clamping
            {
                if (transform.position.x + charRadius > screen.Corner_TopRight.x)
                {
                    var deep = transform.position.x + charRadius - screen.Corner_TopRight.x;
                    rb.velocity = new Vector2(-(Mathf.Abs(rb.velocity.x) + deep) * restitution, rb.velocity.y);
                }
                if (transform.position.x - charRadius < screen.Corner_BottomLeft.x)
                {
                    var deep = transform.position.x - charRadius + screen.Corner_TopRight.x;
                    rb.velocity = new Vector2(Mathf.Abs(rb.velocity.x + deep) * restitution, rb.velocity.y);
                }
            }
#else
            dirX = Input.acceleration.x * speed;
            transform.position = new Vector2(Mathf.Clamp(transform.position.x, -8.5f, 8.5f), transform.position.y);
            rb.velocity = new Vector2(dirX, rb.velocity.y);
#endif
            //Vertical Movements
            if (rb.velocity.y == 0)
            {
                if (isCharging == true)
                {
                    if (charge <= chargeLimit)
                    {
                        charge += Time.fixedDeltaTime * chargeMultiplier * 10;
                        if (transform.localScale.y > 0.2f)
                        {
                            transform.localScale = new Vector3(transform.localScale.x + Time.fixedDeltaTime, transform.localScale.y - Time.fixedDeltaTime, transform.localScale.z);
                        }
                    }
                    //CalculateChargeVisualization();
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + charge);
                    transform.localScale = Vector3.one;
                    charge = 0;
                }
            }
        }
    }

    /// <summary>
    /// Handles the player's screen touch.
    /// </summary>
    private void TouchHandler()
    {
        isCharging = Input.GetMouseButton(0);
    }

    /// <summary>
    /// Toggles player's ready state
    /// </summary>
    public void SetReadyLabel(bool state)
    {
        if (isLocalPlayer)
        {
            if (state)
            {
                readyLabel.text = "Ready";
                readyLabel.color = new Color(100f, 255f, 100f, 255f);
            }
            else
            {
                readyLabel.text = "";
            }
        }
    }

    private void SetReadyVisibility(bool state)
    {
        if (isLocalPlayer)
        {
            if (state)
            {
                readyLabel.text = "Not Ready";
                readyLabel.color = new Color(255f, 100f, 100f, 1f);
            }
            else
            {
                readyLabel.text = "";
            }
        }
    }

    [ClientRpc]
    public void Arise()
    {
        SetPlayerAliveStatus(false, true);
        SetReadyVisibility(false);
    }

    #region HookFunction

    /// <summary>
    /// Hook funtion to set a player's active state
    /// </summary>
    /// <remarks>This function is called to set the player's ability to move.</remarks>
    /// <param name="_old">[unused]</param>
    /// <param name="_new">new state</param>
    public void SetPlayerAliveStatus(bool _old, bool _new)
    {
        active = _new;
        Debug.Log("Hook fired: SetPlayerAliveStatus() on NetPlayerScript.cs with new value of " + _new);
    }

    public void SetPlayerName(string _old, string _new)
    {
        playerName = _new;
        playerNameLabel.text = _new;
        Debug.Log("Hook fired: SetPlayerName() on NetPlayerScript.cs with new value of " + _new);
    }

    /// <summary>
    /// Hook function to set player's ready state
    /// </summary>
    /// <remarks>
    /// This function is used when a player is connected, but the game hasn't started yet.
    /// </remarks>
    /// <param name="_old">[unused]</param>
    /// <param name="_new">new state</param>
    public void SetReadyState(bool _old, bool _new)
    {
        ready = _new;
        Debug.Log("Hook fired: SetReadyState() on NetPlayerScript.cs with new value of " + _new);
    }

    #endregion HookFunction

    private void OnValidate()
    {
        if (restitution < 0.01f) restitution = 0.01f;
        if (restitution > 1f) restitution = 1f;
        if (chargeMultiplier < 1) chargeMultiplier = 1;
    }
}