using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Shushao {
	
	public static class Utility {

		public static Dictionary<Direction, Vector2> DirectionVectors = new Dictionary<Direction, Vector2> {
			{ Direction.TOP, new Vector2(0.0f, 1.0f) },
			{ Direction.TOPRIGHT, new Vector2(1.0f, 1.0f) },
			{ Direction.RIGHT, new Vector2(1.0f, 0.0f) },
			{ Direction.BOTTOMRIGHT, new Vector2(1.0f, -1.0f) },
			{ Direction.BOTTOM, new Vector2(0.0f, -1.0f) },
			{ Direction.BOTTOMLEFT, new Vector2(-1.0f, -1.0f) },
			{ Direction.LEFT, new Vector2(-1.0f, 0.0f) },
			{ Direction.TOPLEFT, new Vector2(-1.0f, 1.0f) }
		};

		public static Sprite getItemSprite(ContentType type) {
			Sprite[] textures = Resources.LoadAll<Sprite>("Sprites/Materials");
			int index = 0;
			switch (type) {
				case ContentType.Ice:
					index = 0;
					break;
				case ContentType.Gold:
					index = 1;
					break;
				case ContentType.Iron:
					index = 2;
					break;
				case ContentType.Uranium:
					index = 3;
					break;
				case ContentType.Garbage:
					index = 4;
					break;
				case ContentType.Parts:
					index = 5;
					break;
				case ContentType.Passengers:
					index = 6;
					break;

			}
			return textures[index];
		}

		public static Sprite getRandomSpriteInMultiple(string resource, int numSprites) {
			Sprite[] textures = Resources.LoadAll<Sprite>(resource);
			int random = Random.Range(0, numSprites - 1);
			return textures[random];
		}

		public static float joy2objRot(float joya) {
			return (joya > 0 ? joya - 90.0f : joya + 270.0f);
		}

		// classe per prendere a caso
		public class Picker<T> {

			public Dictionary<T,int> probabilityTable { get; set; }

			readonly List<T> _pickList = new List<T>();

			Dictionary<T,int> _probabilities;
			public Dictionary<T,int> Probabilities {
				get { return _probabilities; }
				set {
					_probabilities = value;
					Init(value);
				}
			}

			public Picker(Dictionary<T,int> table) {
				Init(table);
			}

			public void Add(T element, int weight) {
				Probabilities.Add(element, weight);
				Init(Probabilities);
			}

			public void Remove(T key) {
				Probabilities.Remove(key);
				Init(Probabilities);
			}

			public void Init(Dictionary<T,int> table) {
				_pickList.Clear();
				probabilityTable = table;
				foreach (var entry in probabilityTable) {
					for (int n = 0; n < entry.Value; n++) {
						_pickList.Add(entry.Key);
					}
				}
			}

			public T Pick() {
				return _pickList[Random.Range(0, _pickList.Count - 1)];
			}
		}

		public static Quaternion getLookRotation(Transform origin, Transform target) {
			Vector2 direction = target.position - origin.position;
			//Debug.DrawRay(origin.position, (Vector3)direction, Color.red, 10.0f);
			var angle = -Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			Quaternion destRotation = Quaternion.AngleAxis(angle, Vector3.forward);
			return destRotation;
		}

		public static Vector3 angle2Direction(float angle) {
			float rad = Mathf.Deg2Rad * angle;
			return new Vector3(Mathf.Sin(rad), Mathf.Cos(rad));
		}

		public static Quaternion getRotationFromAngle2D(float angle) {
			return Quaternion.Euler(new Vector3(0, 0, angle));
		}

		public static bool IsDestroyed(GameObject obj) {
			return obj == null && !ReferenceEquals(obj, null);
		}

		public static float ClampAngle(float angle) {
			return angle % 360.0f;
		}

		public static int ClampAngle(int angle) {
			return (int)(angle % 360.0f);
		}

		public static bool ColliderIsInGame(Collider2D c) {
			if (c == null) return false;
			if (!WorldController.Instance.ENTITIES.ContainObject(c.gameObject)) return false;
			if (!c.isActiveAndEnabled) return false;
			if (!c.gameObject.activeSelf) return false; 
			if (c.gameObject.GetComponent<SpriteRenderer>() == null) return false;
			if (!c.gameObject.GetComponent<SpriteRenderer>().enabled) return false;
			return true;
		}

		public static bool IsInGame(GameObject c) {
			if (c == null) return false;
			if (!WorldController.Instance.ENTITIES.ContainObject(c.gameObject)) return false;
			if (!c.gameObject.activeSelf) return false; 
			if (c.gameObject.GetComponent<SpriteRenderer>() == null) return false;
			if (!c.gameObject.GetComponent<SpriteRenderer>().enabled) return false;
			return true;
		}

        public static bool AnimatorHasParameter(Animator animator, string param) {
            bool found = false;
            foreach (AnimatorControllerParameter pa in animator.parameters) {
                if (pa.name == param) {
                    found = true;
                    break;
                }
            }
            return found;
        }
	}
}

