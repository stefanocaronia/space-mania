using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessBkg : MonoBehaviour {

	struct Boundaries
	{
		public float Left;
		public float Right;
		public float Top;
		public float Bottom;
		public Vector2 size;
	};

	struct StarfieldStruct {
		public GameObject mc;
		public GameObject tc;
		public GameObject bc;
		public GameObject tl;
		public GameObject ml;
		public GameObject bl;
		public GameObject tr;
		public GameObject mr;
		public GameObject br;
	}

	string[] starfieldImages = {
		"starfield_01",
		"starfield_02",
		"starfield_03",
		"starfield_04"
	};

	SpriteRenderer[] sprites;

	Boundaries screenBoundaries;
	Boundaries backgroundBoundaries;

	SpriteRenderer topleft_sr;
	SpriteRenderer topright_sr;
	SpriteRenderer bottomleft_sr;
	SpriteRenderer bottomright_sr;

	GameObject topleft;
	GameObject topright;
	GameObject bottomleft;
	GameObject bottomright;

	StarfieldStruct starfields;

	void Awake() {

		randomizeTextures ();
		calculateScreenBoundaries ();
		cloneStarfields ();
		setSprites ();
	}

	void Update () {
	
		bool change = false;
		
		calculateScreenBoundaries ();
		calculateBackgroundBoundaries ();

		Vector3 newPosition = transform.position;

		if (screenBoundaries.Top >= backgroundBoundaries.Top) {
			newPosition.y += screenBoundaries.size.y;
			change = true;
		} else if (screenBoundaries.Bottom <= backgroundBoundaries.Bottom) {
			newPosition.y -= screenBoundaries.size.y;
			change = true;
		}
		if (screenBoundaries.Right >= backgroundBoundaries.Right) {
			newPosition.x += screenBoundaries.size.x;
			change = true;
		} else if (screenBoundaries.Left <= backgroundBoundaries.Left) {
			newPosition.x -= screenBoundaries.size.x;
			change = true;
		}

		if (change) {
			transform.position = newPosition;
		}
	}


	void calculateScreenBoundaries(){
		screenBoundaries.Left = Camera.main.ViewportToWorldPoint(new Vector2 (0, 0)).x;
		screenBoundaries.Right = Camera.main.ViewportToWorldPoint(new Vector2 (1, 0)).x;
		screenBoundaries.Top = Camera.main.ViewportToWorldPoint(new Vector2 (0, 1)).y;
		screenBoundaries.Bottom = Camera.main.ViewportToWorldPoint(new Vector2 (0, 0)).y;
		screenBoundaries.size.x = Mathf.Abs(screenBoundaries.Right - screenBoundaries.Left);
		screenBoundaries.size.y = Mathf.Abs(screenBoundaries.Top - screenBoundaries.Bottom);
	}

	void calculateBackgroundBoundaries() {
		backgroundBoundaries.Left = topleft_sr.bounds.min.x;
		backgroundBoundaries.Right = topright_sr.bounds.max.x;
		backgroundBoundaries.Top = topleft_sr.bounds.max.y;
		backgroundBoundaries.Bottom = bottomright_sr.bounds.min.y;
		backgroundBoundaries.size.x = Mathf.Abs(backgroundBoundaries.Right - backgroundBoundaries.Left);
		backgroundBoundaries.size.y = Mathf.Abs(backgroundBoundaries.Top - backgroundBoundaries.Bottom);
	}

	void setSprites() {
		topleft = transform.Find("starfield_tl/tl").gameObject;
		topright = transform.Find("starfield_tr/tr").gameObject;
		bottomleft = transform.Find("starfield_bl/bl").gameObject;
		bottomright = transform.Find("starfield_br/br").gameObject;
		topleft_sr = topleft.GetComponent<SpriteRenderer> ();
		topright_sr = topright.GetComponent<SpriteRenderer> ();
		bottomleft_sr = bottomleft.GetComponent<SpriteRenderer> ();
		bottomright_sr = bottomright.GetComponent<SpriteRenderer> ();	
	}

	void cloneStarfields () {
		starfields.mc = transform.Find ("starfield_mc").gameObject;
		starfields.tc = (GameObject)Instantiate(starfields.mc, transform);
		starfields.tc.name = "starfield_tc";
		starfields.tl = (GameObject)Instantiate(starfields.mc, transform);
		starfields.tl.name = "starfield_tl";
		starfields.tr = (GameObject)Instantiate(starfields.mc, transform);
		starfields.tr.name = "starfield_tr";
		starfields.ml = (GameObject)Instantiate(starfields.mc, transform);
		starfields.ml.name = "starfield_ml";
		starfields.mr = (GameObject)Instantiate(starfields.mc, transform);
		starfields.mr.name = "starfield_mr";
		starfields.bl = (GameObject)Instantiate(starfields.mc, transform);
		starfields.bl.name = "starfield_bl";
		starfields.bc = (GameObject)Instantiate(starfields.mc, transform);
		starfields.bc.name = "starfield_bc";
		starfields.br = (GameObject)Instantiate(starfields.mc, transform);
		starfields.br.name = "starfield_br";
		starfields.tc.transform.position = new Vector2 (starfields.mc.transform.position.x, starfields.mc.transform.position.y + screenBoundaries.size.y);
		starfields.tl.transform.position = new Vector2 (starfields.mc.transform.position.x - screenBoundaries.size.x, starfields.mc.transform.position.y + screenBoundaries.size.y);
		starfields.tr.transform.position = new Vector2 (starfields.mc.transform.position.x + screenBoundaries.size.x, starfields.mc.transform.position.y + screenBoundaries.size.y);
		starfields.ml.transform.position = new Vector2 (starfields.mc.transform.position.x - screenBoundaries.size.x, starfields.mc.transform.position.y);
		starfields.mr.transform.position = new Vector2 (starfields.mc.transform.position.x + screenBoundaries.size.x, starfields.mc.transform.position.y);
		starfields.bl.transform.position = new Vector2 (starfields.mc.transform.position.x - screenBoundaries.size.x, starfields.mc.transform.position.y - screenBoundaries.size.y);
		starfields.bc.transform.position = new Vector2 (starfields.mc.transform.position.x, starfields.mc.transform.position.y - screenBoundaries.size.y);
		starfields.br.transform.position = new Vector2 (starfields.mc.transform.position.x + screenBoundaries.size.x, starfields.mc.transform.position.y - screenBoundaries.size.y);
	}

	void randomizeTextures(){
		sprites = GetComponentsInChildren<SpriteRenderer> ();
		foreach (SpriteRenderer s in sprites) {			
			s.sprite = Resources.Load(starfieldImages[Random.Range(0,3)], typeof(Sprite)) as Sprite;
			s.flipX = (Random.value>0.5f?true:false);
			s.flipY = (Random.value>0.5f?true:false);
		}
	}
}
