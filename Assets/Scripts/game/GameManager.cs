using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    /**
     * Define normal moving speed. Actual speed = Speed in entity stats * normal moving speed * other multiplier. 
     */
    public float normalMovingSpeed = 6.25f;
    /**
     * Define base gravity. Might be useful if we are going to implement this game in 3d.
     */
    public float baseGravity = 0.245f;
    public Vector2 xBorder { get; private set; }
    public Vector2 zBorder { get; private set; }
    
    // Start is called before the first frame update
    void Start()
    {
        SetBorder(new Vector2(-5, 5), new Vector2(5, -5));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**
     * Set global border of the map by two points. Camera position can only be in this range.
     */
    public void SetBorder(Vector2 pointA, Vector2 pointB)
    {
        float minX = Mathf.Min(pointA.x, pointB.x);
        float minZ = Mathf.Min(pointA.y, pointB.y);
        float maxX = Mathf.Max(pointA.x, pointB.x);
        float maxZ = Mathf.Max(pointA.y, pointB.y);
        xBorder = new Vector2(minX, maxX);
        zBorder = new Vector2(minZ, maxZ);
        print("Border set to x: " + xBorder + " z: " + zBorder);
    }

    /**
     * Clamp camera position within this range.
     */
    public Vector2 ClampBorder(float x, float y)
    {
        float clampX = Mathf.Clamp(x, xBorder.x, xBorder.y);
        float clampY = Mathf.Clamp(y, zBorder.x, zBorder.y);
        return new Vector2(clampX, clampY);
    }
}
