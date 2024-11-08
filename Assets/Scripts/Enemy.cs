using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FSM States for the enemy
public enum EnemyState { STATIC, CHASE, REST, MOVING, DEFAULT };

public enum EnemyBehavior { EnemyBehavior1, EnemyBehavior2, EnemyBehavior3 };

public class Enemy : MonoBehaviour
{
    //pathfinding
    protected PathFinder pathFinder;
    public GenerateMap mapGenerator;
    protected Queue<Tile> path;
    protected GameObject playerGameObject;

    public Tile currentTile;
    protected Tile targetTile;
    public Vector3 velocity;

    //properties
    public float speed = 1.0f;
    public float visionDistance = 5;
    public int maxCounter = 5;
    protected int playerCloseCounter;

    protected EnemyState state = EnemyState.DEFAULT;
    protected Material material;

    public EnemyBehavior behavior = EnemyBehavior.EnemyBehavior1;

    // Start is called before the first frame update
    void Start()
    {
        path = new Queue<Tile>();
        pathFinder = new PathFinder();
        playerGameObject = GameObject.FindWithTag("Player");
        playerCloseCounter = maxCounter;
        material = GetComponent<MeshRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        // Stop Moving the enemy if the player has reached the goal
        if (playerGameObject.GetComponent<Player>().IsGoalReached() || playerGameObject.GetComponent<Player>().IsPlayerDead())
        {
            //Debug.Log("Enemy stopped since the player has reached the goal or the player is dead");
            return;
        }

        switch (behavior)
        {
            case EnemyBehavior.EnemyBehavior1:
                HandleEnemyBehavior1();
                break;
            case EnemyBehavior.EnemyBehavior2:
                HandleEnemyBehavior2();
                break;
            case EnemyBehavior.EnemyBehavior3:
                HandleEnemyBehavior3();
                break;
            default:
                break;
        }

    }

    public void Reset()
    {
        Debug.Log("enemy reset");
        path.Clear();
        state = EnemyState.DEFAULT;
        currentTile = FindWalkableTile();
        transform.position = currentTile.transform.position;
    }

    Tile FindWalkableTile()
    {
        Tile newTarget = null;
        int randomIndex = 0;
        while (newTarget == null || !newTarget.mapTile.Walkable)
        {
            randomIndex = (int)(Random.value * mapGenerator.width * mapGenerator.height - 1);
            newTarget = GameObject.Find("MapGenerator").transform.GetChild(randomIndex).GetComponent<Tile>();
        }
        return newTarget;
    }

    // Dumb Enemy: Keeps Walking in Random direction, Will not chase player
    private void HandleEnemyBehavior1()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 

                //Changed the color to white to differentiate from other enemies
                material.color = Color.white;

                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // Enemy2: Chases the player when it is nearby
    private void HandleEnemyBehavior2()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerGameObject.transform.position);

        if (distanceToPlayer <= visionDistance)
        {
            // Player is within vision, chase the player
            Tile playerTile = FindClosestTile(playerGameObject.transform.position);
            path = pathFinder.FindPathAStar(currentTile, playerTile);

            if (path.Count > 0)
            {
                targetTile = path.Dequeue();
                state = EnemyState.MOVING;
            }
        }
        else
        {
            HandleEnemyBehavior1(); // Random walking behavior if the player is not in vision
        }

        if (state == EnemyState.MOVING)
        {
            velocity = targetTile.gameObject.transform.position - transform.position;
            transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
            {
                currentTile = targetTile;
                state = EnemyState.DEFAULT;
            }
        }
    }

    // Enemy3: Targets a tile that is a few tiles away from the player
    private void HandleEnemyBehavior3()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerGameObject.transform.position);

        if (distanceToPlayer <= visionDistance)
        {
            // Player is within vision, target a tile near the player
            Tile playerTile = FindClosestTile(playerGameObject.transform.position);
            Tile targetTile = GetTileTwoStepsAway(playerTile);
            path = pathFinder.FindPathAStar(currentTile, targetTile);

            if (path.Count > 0)
            {
                this.targetTile = path.Dequeue();
                state = EnemyState.MOVING;
            }
        }
        else
        {
            HandleEnemyBehavior1(); // Random walking behavior if the player is not in vision
        }

        if (state == EnemyState.MOVING)
        {
            velocity = targetTile.gameObject.transform.position - transform.position;
            transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

            if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
            {
                currentTile = targetTile;
                state = EnemyState.DEFAULT;
            }
        }
    }

    // Helper method to get all tiles in the map
    private IEnumerable<Tile> GetAllTiles()
    {
        List<Tile> allTiles = new List<Tile>();
        for (int i = 0; i < mapGenerator.width * mapGenerator.height; i++)
        {
            Tile tile = GameObject.Find("MapGenerator").transform.GetChild(i).GetComponent<Tile>();
            allTiles.Add(tile);
        }
        return allTiles;
    }

    // Helper method to find the closest tile to a given position
    private Tile FindClosestTile(Vector3 position)
    {
        Tile closestTile = null;
        float closestDistance = float.MaxValue;

        foreach (Tile tile in GetAllTiles())
        {
            float distance = Vector3.Distance(tile.transform.position, position);
            if (distance < closestDistance)
            {
                closestTile = tile;
                closestDistance = distance;
            }
        }
        return closestTile;
    }

    // Helper method to get a tile that is two steps away from the player's current position
    private Tile GetTileTwoStepsAway(Tile playerTile)
    {
        List<Tile> adjacents = playerTile.Adjacents;
        if (adjacents.Count > 0)
        {
            return adjacents[UnityEngine.Random.Range(0, adjacents.Count)];
        }
        return playerTile; // Default to the player tile if no suitable tile is found
    }
}
