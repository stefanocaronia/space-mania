using UnityEngine;
using System.Collections;
using Shushao;
using UnityEngine.UI;

public class UIItem : MonoBehaviour {

	public CargoContainer container;
	public Image image;
	public Text text;

	public void setContainer(CargoContainer cont) {
		container = cont;
		image.sprite = Shushao.Utility.getItemSprite(cont.contentType);
		text.text = container.contentType.ToString() + " (" + cont.quantity + ")";
	}
}
