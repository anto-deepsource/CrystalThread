using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Buildable : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {
	
	private float hideBuildIndicatorsDelay = 0;

	static float BI_HIDE_DELAY = 0.2f;

	public List<ResourceTypeValuePair> costs = new List<ResourceTypeValuePair>();

	private bool built = false;
	private bool set = false;
	private bool placing = false;

	private Transform realMesh;

	private Transform buildMesh;
	private Collider buildCollider;
	private List<GameObject> buildIndicators = new List<GameObject>();

	private Transform setGoodMesh;

	private Transform playerCursor;

	private ObjectQuery query;

	// Use this for initialization
	void Awake () {
		realMesh = transform.Find("RealMesh");

		buildMesh = transform.Find("BuildMesh");
		Transform bi = buildMesh.Find("buildIndicators");
		for (int i = 0; i < bi.childCount; i++) {
			GameObject indicator = bi.GetChild(i).gameObject;
			buildIndicators.Add(indicator);
			indicator.GetComponent<SpriteRenderer>().enabled = false;
			//indicator.SetActive(false);
		}

		buildCollider = buildMesh.GetComponentInChildren<Collider>();

		setGoodMesh = transform.Find("SetGoodMesh");

		query = new ObjectQuery() {
			gameObject = gameObject,
			isStaticObstacle = true,
		};
		QueryManager.RegisterObject(query);
	}

	public void OnDestroy() {
		QueryManager.UnregisterObject(query);
	}


	// Update is called once per frame
	void Update () {
		if ( set && !built ) {
			if (hideBuildIndicatorsDelay > 0) {
				hideBuildIndicatorsDelay -= Time.deltaTime;
				if (hideBuildIndicatorsDelay <= 0) {
					HideBuildIndicators();
				}
			}
		} else if ( placing ) {
			transform.position = playerCursor.position;
		}
		
	}

	public void OnPlayerLookAt( GameObject player ) {
		if ( !built ) {
			UpdateBuildIndicators(player);
		}
		
	}

	public void OnPlayerInteract(GameObject player) {
		if (set && !built) {
			ResourcesComponent resources = Utilities.Resources(player);

			if (resources.CanAfford(costs)) {
				resources.Subtract(costs);

				Build();
			}
		}
		
	}

	private void UpdateBuildIndicators(GameObject player) {
		float closestD = 9999999;
		GameObject closestIndicator = null;
		foreach (GameObject indicator in buildIndicators) {
			//indicator.SetActive(false);
			indicator.GetComponent<SpriteRenderer>().enabled = false;
			float d = CommonUtils.DistanceSquared(indicator.transform.position, player.transform.position);
			if (closestIndicator == null || d < closestD) {
				closestD = d;
				closestIndicator = indicator;
			}
		}
		
		closestIndicator.GetComponent<SpriteRenderer>().enabled = true;

		hideBuildIndicatorsDelay = BI_HIDE_DELAY;
	}

	private void HideBuildIndicators() {
		foreach (GameObject indicator in buildIndicators) {
			indicator.GetComponent<SpriteRenderer>().enabled = false;
		}
	}

	private void Build() {
		buildMesh.gameObject.SetActive(false);
		HideBuildIndicators();
		
		realMesh.gameObject.SetActive(true);
	}

	public void StartSetting( Transform cursor ) {
		built = false;
		set = false;
		placing = true;
		buildMesh.gameObject.SetActive(false);
		realMesh.gameObject.SetActive(false);
		setGoodMesh.gameObject.SetActive(true);

		playerCursor = cursor;
	}

	public void FinishSetting() {
		built = false;
		set = true;
		placing = false;
		buildMesh.gameObject.SetActive(true);
		realMesh.gameObject.SetActive(false);
		setGoodMesh.gameObject.SetActive(false);
	}
	
	public void OnPointerUp(PointerEventData eventData) {
		Debug.Log("Pointer up");
	}

	public void OnPointerClick(PointerEventData eventData) {
		Debug.Log("Pointer click");
	}

	public void OnPointerDown(PointerEventData eventData) {
		Debug.Log("Pointer down");
	}

	public void OnMouseDown() {
		Debug.Log("Mouse down");
	}
}
