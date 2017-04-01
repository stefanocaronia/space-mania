using UnityEngine;
using System.Collections;
using Shushao;

public class ScriptComponent : MonoBehaviour
{
    [HideInInspector]
    public ShipController SHIP;
    [HideInInspector]
    public ItemController ITEM;
    [HideInInspector]
    public SpriteRenderer Sprite;
    [HideInInspector]
    public Damageable DAMAGEABLE;
    [HideInInspector]
    public Cargo CARGO;
    [HideInInspector]
    public Animator Animator;
    [HideInInspector]
    public Animation anim;
    [HideInInspector]
    public AudioSource[] Sounds;
    [HideInInspector]
    public Rigidbody2D RIGIDBODY;
    [HideInInspector]
    public AIController AI;

    protected bool isShip;
    [HideInInspector]
    public bool isPlayerShip;
    protected bool isAsteroid;
    protected bool isItem;
    protected bool isAI;
    protected bool isWreck;
    protected bool isDamageable;
    protected bool hasCargo;

    protected void InitScriptComponent()
    {
        AI = GetComponent<AIController>();
        RIGIDBODY = GetComponent<Rigidbody2D>();
        SHIP = GetComponent<ShipController>();
        ITEM = GetComponent<ItemController>();
        DAMAGEABLE = GetComponent<Damageable>();
        CARGO = GetComponent<Cargo>();

        Animator = GetComponent<Animator>();
        Sounds = GetComponents<AudioSource>();
        Sprite = GetComponent<SpriteRenderer>();

        isAI = (AI != null);
        isAsteroid = (GetComponent<AsteroidController>() != null);
        isShip = (SHIP != null);
        isPlayerShip = isShip && CompareTag("Player");
        isItem = (ITEM != null);
        isDamageable = (DAMAGEABLE != null);
        isWreck = (GetComponent<WreckController>() != null);
        hasCargo = (CARGO != null);
    }

    public void RegenerateCollider()
    {
        if (GetComponent<PolygonCollider2D>() != null)
            Destroy(GetComponent<PolygonCollider2D>());

        PolygonCollider2D polygonCollider = this.gameObject.AddComponent<PolygonCollider2D>();
        polygonCollider.sharedMaterial = Resources.Load("Materials/Rock") as PhysicsMaterial2D;
    }

    // destroy gameobject (dopo tot secondi)
    public void Die(float delay)
    {
        if (ToDestroy())
            Destroy(this.gameObject, delay);
    }

    // destroy gameobject
    public void Die()
    {
        if (ToDestroy())
            Destroy(this.gameObject);
    }

    private bool ToDestroy()
    {
        if (isItem || isAsteroid)
            WorldController.Instance.unregisterEntity(this.gameObject);

        if (isAsteroid) {
            WorldController.Instance.AsteroidsPool.Put(this.gameObject);
            return false;
        }

		if (isShip && !isPlayerShip) {
			Pool pool = WorldController.Instance.GetPool(SHIP.Type);
			if (pool != null) pool.Put(this.gameObject);
			return false;					
		}

        

        if (GetComponent<Fire>() != null)  {
            GetComponent<Fire>().Source.Put(this.gameObject);
            return false;
        }

        return true;
    }

}