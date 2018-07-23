using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 
/// </summary>
public class ResourcesIndicator : MonoBehaviour {

	public ResourcesComponent resources;

	public GameObject cellPrefab;

	private List<GameObject> cells = new List<GameObject>();

	private Dictionary<ResourceType, Text> amountTexts = new Dictionary<ResourceType, Text>();

	private QuickListener listener;
	
	// Use this for initialization
	void Start () {
		listener = new QuickListener(ResourceComponentEventCallback );
		resources.events.Add(listener);
	}
	
	// Update is called once per frame
	void Update () {
		if (cells.Count != resources.Count) {
			ValidateCells();
		//} else {
		//	//int i = 0;
		//	foreach (var pair in resources.Quantities) {
		//		//GameObject cell = cells[i];
		//		//Text amountText = cell.transform.Find("AmountText").gameObject.GetComponent<Text>();
		//		amountTexts[pair.Key].text = pair.Value.ToString();

		//		//i++;
		//	}
		}

	}

	private void ValidateCells() {
		foreach( GameObject cell in cells ) {
			Destroy(cell);
		}
		cells.Clear();
		amountTexts.Clear();

		float yDis = 0;

		foreach( var pair in resources.Quantities) {
			GameObject newCell = GameObject.Instantiate(cellPrefab, transform);
			newCell.SetActive(true);
			Text titleText = newCell.transform.Find("TitleText").gameObject.GetComponent<Text>();
			titleText.text = pair.Key.ToString();

			Text amountText = newCell.transform.Find("AmountText").gameObject.GetComponent<Text>();
			amountText.text = pair.Value.ToString();

			RectTransform rect = newCell.GetComponent<RectTransform>();
			rect.position = rect.position + new Vector3( 0 , yDis, 0 );
			yDis += rect.rect.height;

			cells.Add(newCell);
			amountTexts.Add(pair.Key, amountText);
		}
	}

	private void ResourceComponentEventCallback( int eventCode, object data ) {
		ResourceType resourceType = (ResourceType)data;
		if ( !amountTexts.ContainsKey(resourceType)) {
			return;
		}
		switch( (ResourceEvent)eventCode) {
			case ResourceEvent.Add:
				amountTexts[resourceType].text = resources.Quantities[resourceType].ToString();
				break;
			case ResourceEvent.Subtract:
				amountTexts[resourceType].text = resources.Quantities[resourceType].ToString();
				TextAnimations.FlashRed(amountTexts[resourceType], Color.black);
				break;
		}
	}
}
