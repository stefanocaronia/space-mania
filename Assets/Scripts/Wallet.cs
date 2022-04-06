using UnityEngine;

public class Wallet {

	public Component Owner {
		get;
		private set;
	}

	private float _credits;
	public float Credits {
		get {
			return _credits;
		}
		set {
			_credits = (value < 0.0f ? 0.0f : value);
		}
	}

	public Wallet(Component owner) {
		Owner = owner;
	}

	void Awake(){
		Credits = 0.0f;
	}

	public float Receive(float amount) {
		Credits += amount;
		if (Owner is PlayerManager)
			GameManager.Instance.UI.updateCredits();
		return amount;
	}

	public float Pay(float amount) {
		float taken = Mathf.Clamp(amount, 0.0f, Credits);
		Credits -= taken;
		if (Owner is PlayerManager)
			GameManager.Instance.UI.updateCredits();
		return taken;
	}

	public float Tansfer(float amount, Wallet receiver) {
		float taken = Pay(amount);
		if (Owner is PlayerManager)
			GameManager.Instance.UI.updateCredits();
		return receiver.Receive(taken);
	}

	public float Transfer(Wallet receiver) {
		float taken = Pay(Credits);
		if (Owner is PlayerManager)
			GameManager.Instance.UI.updateCredits();
		return receiver.Receive(taken);
	}
}
