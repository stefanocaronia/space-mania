using UnityEngine;
using System.Collections.Generic;
using Shushao;

public class EventManager : SingletonBehaviour<EventManager> {

	private Dictionary<string, List<Component>> Listeners = new Dictionary<string, List<Component>>();

	public void AddListener (string eventName, Component sender) {
		
		if (!Listeners.ContainsKey(eventName))
			Listeners.Add(eventName, new List<Component>());

		Listeners[eventName].Add(sender);
	}

	public void RemoveListener(string eventName, Component sender) {
		if(!Listeners.ContainsKey(eventName))
			return;

		for(int i = Listeners[eventName].Count-1; i>=0; i--) {			
			if(Listeners[eventName][i].GetInstanceID() == sender.GetInstanceID())
				Listeners[eventName].RemoveAt(i); 
		}
	}

	public void ClearListeners() {
		Listeners.Clear();
	}

	public void Post(string eventName, Component sender) {
		if(!Listeners.ContainsKey(eventName))
			return;

		foreach (Component listener in Listeners[eventName]) {		
			listener.SendMessage("On" + eventName, sender, SendMessageOptions.DontRequireReceiver);		
		}
	}
}
