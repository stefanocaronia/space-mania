using System.Collections.Generic;
using Shushao;

namespace Shushao {

	public enum AIAction {
		STILL,
		PATROL,
		MINE,
		GATHER,
		DOCK,
		ATTACK,
		ESCAPE
	}

	public enum AIBehaviour {
		PASSIVE,
		DEFENSIVE,
		AGGRESSIVE
	}

	public enum AIAttack {
		RANGED,
		CONTACT
	}

	public enum AIGoal {
		PATROL,
		SELL,
		KILL,
		ESCAPE,
		EAT
	}

	public struct AITemplate {
		public AIBehaviour Behaviour;
		public AIAction StartAction;
		public EntityType[] Targets;
		public ContentType[] Desires;
		public AIAttack Attack;
		public AIGoal Goal;
		public string[] Avoid;

		public List<string> DesiresTags {
			get {
				var tags = new List<string>();
				foreach (ContentType ct in Desires) {
					tags.Add(ct.ToString());
				}
				return tags;
			}
		}

		public List<string> TargetsTags {
			get {
				var tags = new List<string>();
				foreach (EntityType ct in Targets) {
					tags.Add(ct.ToString());
				}
				return tags;
			}
		}
	}

	static public class AIConfiguration {

		static public Dictionary<EntityType, AITemplate> AITemplates = new Dictionary<EntityType, AITemplate> {
			{ 
				EntityType.Miner, new AITemplate {
					Behaviour = AIBehaviour.PASSIVE,
					StartAction = AIAction.PATROL,
					Attack = AIAttack.RANGED,
					Targets = new [] { 
						EntityType.Asteroid 
					},
					Desires = new [] {
						ContentType.Fuel,
						ContentType.Cell,
						ContentType.Gold,
						ContentType.Ice,
						ContentType.Iron,
						ContentType.Uranium
					},
					Goal = AIGoal.SELL,
				}
			},
			{ 
				EntityType.Pirate, new AITemplate {
					Behaviour = AIBehaviour.DEFENSIVE,
					StartAction = AIAction.PATROL,
					Attack = AIAttack.RANGED,
					Targets = new [] { 
						EntityType.Player, 
						EntityType.Transport,
						EntityType.Wreck,
						EntityType.Miner
					},
					Desires = new [] {
						ContentType.Fuel,
						ContentType.Cell,
						ContentType.Gold,
						ContentType.Uranium,
						ContentType.Passengers
					},
					Goal = AIGoal.SELL,
				}
			},
			{ 
				EntityType.Ameba, new AITemplate {
					Behaviour = AIBehaviour.AGGRESSIVE,
					StartAction = AIAction.PATROL,
					Attack = AIAttack.CONTACT,
					Avoid = new [] {
						"Ateroid",
						"Station",
						"Ameba"
					},
					Targets = new [] { 
						EntityType.Player, 
						EntityType.Transport,
						EntityType.Wreck,
						EntityType.Miner,
						EntityType.Enemy,
						EntityType.Pirate,
					},
					Desires = new [] {
						ContentType.Fuel,
						ContentType.Cell,
						ContentType.Passengers
					},
					Goal = AIGoal.EAT,
				}
			},
			{ 
				EntityType.Transport, new AITemplate {
					Behaviour = AIBehaviour.PASSIVE,
					StartAction = AIAction.PATROL,
					Targets = new EntityType[] {},
					Desires = new ContentType[] {}
				}
			}

		};
	}
}

