using UnityEngine;
using System.Collections.Generic;

namespace Shushao {

	public enum GameStates {
		MENU,
		RUNNING,
		PAUSED,
		GAMEOVER
	}

	public enum ContentType {
		NONE,
		Mixed,
		Garbage,
		Ice,
		Iron,
		Gold,
		Fuel,
		Cell,
		Passengers,
		Parts,
		Uranium
	}

	public enum AsteroidSize {
		NONE,
		HUGE,
		BIG,
		LITTLE
	}

	public enum MonsterSize {
		NONE,
		LITTLE,
		NORMAL,
		BIG,
		HUGE
	}

	public enum Side {
		TOP,
		RIGHT,
		BOTTOM,
		LEFT
	}

	public enum Direction {
		TOP,
		TOPRIGHT,
		RIGHT,
		BOTTOMRIGHT,
		BOTTOM,
		BOTTOMLEFT,
		LEFT,
		TOPLEFT
	}


	public enum EngineState {
		OFF,
		EMDRIVE,
		ON
	}

	[System.Serializable]
	public struct CargoContainer {
		public ContentType contentType;
		public float quantity;

		public CargoContainer(ContentType c, float q) {
			contentType = c;
			quantity = q;
		}
	}

	public enum EntityType {
		Item,
		Asteroid,
		Player,
		Pirate,
		Enemy,
		Miner,
		Transport,
		Hero,
		Station,
		Wreck,
		Ameba
	}

//	public enum ShipType {
//		PLAYER,
//		PIRATE,
//		ENEMY,
//		MINER,
//		TRANSPORT,
//		HERO
//	}

	public class Entity {
		public int ID;
		public string name;
		public EntityType type;
		public ContentType contentType;
		public Vector2 position; // start position
		public GameObject GameObject; // reference
	}

	public class EntityDB : List<Entity> {

		public Entity findByID(int ID) {
			return this.Find(x => x.ID == ID);
		}

		public void removeByID(int ID) {
			this.Remove(this.Find(x => x.ID == ID));
		}

		public int countByType(EntityType type) {
			return this.FindAll(x => x.type == type).Count;
		}

		public Entity findNearestByType(EntityType etype, Vector2 origin) {
			List<Entity> elements = this.FindAll(x => x.type == etype);
			float minDistance = 99999999.0f;
			Entity nearest = new Entity();
			if (elements.Count == 0) return nearest;
			
			foreach (Entity element in elements) {
				
				float distance = (element.position - origin).sqrMagnitude;
				if (distance < minDistance) {
					minDistance = distance;
					nearest = element;
				}
			}
			return nearest;
		}

		public bool ContainObject(GameObject go) {
			return (this.Find(x => x.ID == go.GetInstanceID()) != null);
		}

//		public void rescan() {
//			this.ForEach(x => {
//				if (x.GameObject == null) Remove(x);
//				x.position = x.GameObject.transform.position;
//			});
//		}

	}

	public static class Tables {
	
		public static EntityType[] ShipTypes = new EntityType[] {
			EntityType.Player,
			EntityType.Pirate,
			EntityType.Enemy,
			EntityType.Miner,
			EntityType.Transport,
			EntityType.Hero,
			EntityType.Wreck
		};

		// valore delle merci
		public static Dictionary<ContentType, float> prices = new Dictionary<ContentType, float> {
			{ ContentType.Ice, 0.5f },
			{ ContentType.Iron, 1.0f },
			{ ContentType.Gold, 8.5f },
			{ ContentType.Uranium, 20.0f },
			{ ContentType.Garbage, 0.001f },
			{ ContentType.Parts, 5.0f },
			{ ContentType.Passengers, 18.0f }
		};
	
	}
}
