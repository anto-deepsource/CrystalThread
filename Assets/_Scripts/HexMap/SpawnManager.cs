using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HexMap {
	/// <summary>
	/// Opens up methods for spawning pickups and enemies either during the game or while generating the hex map.
	/// The manager uses the height of the hex map and places the spawned item at the correct y position.
	/// </summary>
	public class SpawnManager : Singleton<SpawnManager> {
		private SpawnManager() { }

		#region Public Members

		public HexagonMaker hexagonMaker;

		public GameObject lightStonePrefab;

		#endregion


		#region Private Members

		private GameObject pickupsFolder;

		#endregion


		#region Component Methods

		private void Awake() {
			ResetSingleton();
		}

		private void Start() {
			pickupsFolder = ObjectFactory.Folder("Picksups", transform);
		}

		#endregion


		#region Public Static Functions

		public static void NewLightStone(Vector3 pos ) {
			NewLightStone(pos.x, pos.z);
		}

		public static void NewLightStone( float x, float z ) {
			Instance.ValidatePickupsFolder();
			GameObject newStone = GameObject.Instantiate(Instance.lightStonePrefab, Instance.pickupsFolder.transform);
			float y = Instance.hexagonMaker.metrics.XZPositionToHeight(x, z, true );
			newStone.transform.position = new Vector3(x, y, z);
		}

		#endregion


		#region Private Helper Methods

		private void ValidatePickupsFolder() {
			if ( pickupsFolder == null ) {

				pickupsFolder = ObjectFactory.Folder("Picksups", transform);
			}
		}

		#endregion
	}
}