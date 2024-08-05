using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything to do with gore and gibs...
/// </summary>
public class GoreManager : Singleton<GoreManager>
{
    private const float GRAVITY_MULTIPLIER = 1f;
    private static List<GibInstance> spawnedGibs = new List<GibInstance>();
    private static SFXManager sfxManager;

    private void Awake()
    {
        sfxManager = FindFirstObjectByType<SFXManager>();
    }

    public static void Cleanup()
    {
        foreach(GibInstance gibInstance in spawnedGibs)
        {
            Destroy(gibInstance.GameObject);
        }

        spawnedGibs.Clear();
    }

    public static void SpawnGibs(Vector3 spawnPosition, List<GibInfo> gibList)
    {
        foreach(GibInfo gibInfo in gibList)
        {
            // Create new gib object...
            GameObject gibObject = new GameObject(gibInfo.gibName + " Gib");
            gibObject.transform.position = gibInfo.spawnPoint.position;

            // Add sprite renderer...
            SpriteRenderer spriteRenderer = gibObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = gibInfo.gibSprite;
            spriteRenderer.sortingOrder = 1;

            // Add Rigidbody2D for physics simulation...
            Rigidbody2D rigidbody2D = gibObject.AddComponent<Rigidbody2D>();
            rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Apply initial velocity and rotation...
            Vector2 initialVelocity = new Vector2(Random.Range(-2f, 2f), Random.Range(2f, 5f));
            float initialRotation = Random.Range(-180f, 180f);
            rigidbody2D.velocity = initialVelocity;
            rigidbody2D.angularVelocity = initialRotation;

            // Add to list of spawned gibs for physics update...
            spawnedGibs.Add(new GibInstance(rigidbody2D, spriteRenderer, gibObject, spawnPosition));
        }
    }

    private void FixedUpdate()
    {
        UpdateGibPhysics();
    }

    private void UpdateGibPhysics()
    {
        foreach(GibInstance gibInstance in spawnedGibs)
        {
            Rigidbody2D rigidbody2D = gibInstance.Rigidbody2D;

            // Check if gib has reached or passed the "floor"...
            float floorY = gibInstance.SpawnPosition.y + Random.Range(-1.5f, 0f);

            if(rigidbody2D.position.y <= floorY)
            {
                // Apply damping to velocity and angular velocity...
                rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, Mathf.Clamp(rigidbody2D.velocity.y * -0.5f, -0.5f, 0));
                rigidbody2D.angularVelocity *= 0.5f;

                if(rigidbody2D.velocity.y <= 0.1f && Mathf.Abs(rigidbody2D.velocity.y) < 0.01f && rigidbody2D.constraints != RigidbodyConstraints2D.FreezeAll)
                {
                    rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, 0);
                    rigidbody2D.angularVelocity = 0;
                    rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
                    gibInstance.GFX.color = new Color(0.75f, 0.75f, 0.75f, 1);
                    DecalManager.SpawnDecal("blood", gibInstance.GameObject.transform.position);
                    sfxManager.Play2DSound("Blood Splat");
                }
            }
            else
            {
                // Continue applying gravity if not grounded...
                rigidbody2D.velocity -= Vector2.down * Physics2D.gravity.y * GRAVITY_MULTIPLIER * Time.fixedDeltaTime;
            }
        }

        // Remove gibs that are frozen from the list...
        //spawnedGibs.RemoveAll(gib => gib.Rigidbody2D.constraints == RigidbodyConstraints2D.FreezeAll);
    }

    private class GibInstance
    {
        public Rigidbody2D Rigidbody2D { get; }
        public GameObject GameObject { get; }
        public SpriteRenderer GFX { get; }
        public Vector3 SpawnPosition { get; }

        public GibInstance(Rigidbody2D rigidbody2D, SpriteRenderer gfx, GameObject gameObject, Vector3 spawnPos)
        {
            Rigidbody2D = rigidbody2D;
            GameObject = gameObject;
            GFX = gfx;
            SpawnPosition = spawnPos;
        }
    }

    [System.Serializable]
    public class GibInfo
    {
        public enum GibType
        {
            HEAD,
            TORSO,
            LEFT_LEG,
            RIGHT_LEG,
            LEFT_HAND,
            RIGHT_HAND,
        }

        public string gibName = "head";
        public Sprite gibSprite = null;
        public Transform spawnPoint;
        public GibType gibType = GibType.HEAD;
    }
}
