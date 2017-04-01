using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shushao;

class Radar : MonoBehaviour {

	public List<Collider2D> Detected = new List<Collider2D>();

	public int Capacity = 128;
	public float Radius = 4.0f;
	public float Interval = 1.0f;
	public LayerMask Layers;

	bool _locked = false;

	public bool Active = true;

	public void Enable() {
		Active = true;
	}

	public void Disable() {
		Active = false;
	}

	void Start() {
		Layers = LayerMask.GetMask("Default");
	}

	public void Scan() {
		if (!Active) {
			Detected.Clear();
			return;
		}
		if (_locked) return;
		_locked = true;
		Detected.Clear();
		Collider2D[] res = new Collider2D[Capacity];
		int num = Physics2D.OverlapCircleNonAlloc(transform.position, Radius, res, Layers);
		if (num == 0) return;

		for (int i = 0; i < res.Length; i++) {
			if (!Active) break;
			Collider2D c = null;
			c = res[i];
			if (!Utility.ColliderIsInGame(c)) continue;
			if (c.gameObject != this.gameObject)
				Detected.Add(c);
		}

		_locked = false;
	}

	public void Reset() {
		Detected.Clear();
	}

	public void scanRepeating() {
		if (!Active) return;
		InvokeRepeating("Scan", 0.1f, Interval); 
	}

	public int FindAsteroids(ref List<Collider2D> results) {
		if (!Active) return 0;
		if (_locked) return 0;
		_locked = true;
		results.Clear();
		int count = 0;
		for (int i = 0; i < Detected.Count; i++) {
			if (!Active) break;
			Collider2D c = null;
			c = Detected[i];
			if (!Utility.ColliderIsInGame(c)) continue;
			if (c.GetComponent<AsteroidController>() != null) {
				results.Add(c);
				count++;
			}
		}
		_locked = false;
		return count;
	}

	public int FindShips(ref List<Collider2D> results) {
		if (!Active) return 0;
		if (_locked) return 0;
		_locked = true;
		results.Clear();
		int count = 0;
		for (int i = 0; i < Detected.Count; i++) {
			if (!Active) break;
			Collider2D c = null;
			c = Detected[i];
			if (!Utility.ColliderIsInGame(c)) continue;
			if (c.GetComponent<ShipController>() != null || c.GetComponent<WreckController>() != null) {
				results.Add(c);
				count++;
			}
		}
		_locked = false;
		return count;
	}

	public int FindWithTag(List<string> tags, bool withCargo, ref List<Collider2D> results) {
		if (!Active) return 0;
		if (_locked) return 0;
		_locked = true;
		results.Clear();
		int count = 0;

		for (int i = 0; i < Detected.Count; i++) {
			if (!Active) break;
			Collider2D c = null;
			c = Detected[i];
			if (!Utility.ColliderIsInGame(c)) continue;
			if (withCargo && (c.GetComponent<Cargo>() == null || (c.GetComponent<Cargo>() != null && c.GetComponent<Cargo>().containers.Count == 0))) continue;
			if (tags.Contains(c.tag)) {
				results.Add(c);
				count++;
			}
		}
		_locked = false;
		return count;
	}

	public int FindItems(IList contentTypes, ref List<Collider2D> results) {
		if (!Active) return 0;
		if (_locked) return 0;
		_locked = true;
		results.Clear();
		int count = 0;
		for (int i = 0; i < Detected.Count; i++) {
			if (!Active) break;
			Collider2D c = null;
			c = Detected[i];
			if (!Utility.ColliderIsInGame(c)) continue;
			if (c.GetComponent<Cargo>() != null && contentTypes.Contains(c.GetComponent<Cargo>().ContentType)) {
				results.Add(c);
				count++;
			}
		}
		_locked = false;
		return count;
	}
}
