using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProceduralFlora : MonoBehaviour {

	public float stemWidthBottom = 0.05f;
	public float stemWidthTop = 0.05f;

	public float stemHeight = 1f;

	public float stemJointPositionVariance = 0.1f;

	public float flowerWidth = 1f;

	public Material stemMaterial;

	[NonSerialized] public List<Vector3> vertices;
	[NonSerialized] public List<int> triangles;
	[NonSerialized] public List<Color> colors;
	[NonSerialized] public List<Vector2> uvs;
	[NonSerialized] public List<Vector2> uvs2;

	public Mesh mesh;

	public bool manualGenerate = false;

	private bool initialized = false;

	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	private void Start() {
		GenerateMesh();
	}

	private void OnDestroy() {
		if (mesh != null) {
			MeshPool.Add(mesh);
			mesh = null;
			if (meshFilter != null) {
				meshFilter.sharedMesh = null;
			}
			
		}
		initialized = false;
	}

	private void Update() {
		if (manualGenerate) {
			GenerateMesh();
			manualGenerate = false;
		}
	}

	private void OnValidate() {
		GenerateMesh();
	}

	private void Initialize() {
		if (initialized) {
			return;
		}

		if (!Application.isPlaying) {
			mesh = new Mesh();
		}
		else {
			mesh = MeshPool.Get();
		}
		mesh.name = "Flora";

		meshRenderer = GetComponent<MeshRenderer>();
		
		meshFilter = GetComponent<MeshFilter>();
		
		initialized = true;
	}

	private void StartLists() {
		vertices = ListPool<Vector3>.Get();
		triangles = ListPool<int>.Get();
		colors = ListPool<Color>.Get();
		uvs = ListPool<Vector2>.Get();
		uvs2 = ListPool<Vector2>.Get();
	}

	private void ApplyListsToMesh() {

		mesh.Clear();

		// Return the lists to the list pool
		mesh.SetVertices(vertices);
		ListPool<Vector3>.Add(vertices);

		mesh.SetTriangles(triangles, 0);
		ListPool<int>.Add(triangles);

		mesh.SetColors(colors);
		ListPool<Color>.Add(colors);

		mesh.SetUVs(0, uvs);
		ListPool<Vector2>.Add(uvs);

		mesh.SetUVs(1, uvs2);
		ListPool<Vector2>.Add(uvs2);

		mesh.RecalculateNormals();
	}

	private void LinkFilterAndRenderer() {
		meshFilter.sharedMesh = mesh;
		meshRenderer.material = stemMaterial;
	}

	private void GenerateMesh() {
		Initialize();
		StartLists();

		int joints = UnityEngine.Random.Range(2, 4);

		var startPosition = Vector3.zero;
		for( int i = 0; i < joints; i ++ ) {
			var nextJoint = startPosition + Vector3.up * stemHeight +
				UnityEngine.Random.onUnitSphere * stemJointPositionVariance;
			CreateStem(startPosition, nextJoint);
			startPosition = nextJoint;
		}

		CreateFlower(startPosition);
		
		LinkFilterAndRenderer();
		ApplyListsToMesh();
	}

	private void CreateStem( Vector3 head, Vector3 tail ) {
		var horizontalBottomOffset = Vector3.right * stemWidthBottom;
		var horizontalTopOffset = Vector3.right * stemWidthTop;
		AddQuad(head + horizontalBottomOffset, head - horizontalBottomOffset,
			tail - horizontalTopOffset, tail + horizontalTopOffset, Color.white);

		horizontalBottomOffset = Vector3.forward * stemWidthBottom;
		horizontalTopOffset = Vector3.forward * stemWidthTop;
		AddQuad(head + horizontalBottomOffset, head - horizontalBottomOffset,
			tail - horizontalTopOffset, tail + horizontalTopOffset, Color.white);

		for( int i = 0; i < 8; i++ ) {
			uvs2.Add(new Vector2(0, 0));
		}
	}

	private void CreateFlower(Vector3 head) {
		var horizontalBottomOffset = Vector3.right * stemWidthBottom;
		var horizontalTopOffset = Vector3.right * stemWidthTop;
		AddQuad(head + Vector3.right * flowerWidth * 0.5f,
			head + Vector3.forward * flowerWidth * 0.5f,
			head + Vector3.left * flowerWidth * 0.5f,
			head + Vector3.back * flowerWidth * 0.5f, Color.white);

		for (int i = 0; i < 4; i++) {
			uvs2.Add(new Vector2(1, 0));
		}
	}

	/// <summary>
	/// A, B, C, D should be points that wind CCW
	/// </summary>
	private void AddQuad(Vector3 A, Vector3 B, Vector3 C, Vector3 D, Color color) {
		int startIndex = vertices.Count;

		vertices.Add(A);
		uvs.Add(new Vector2(1,0));
		colors.Add(color);

		vertices.Add(B);
		uvs.Add(Vector2.zero);
		colors.Add(color);

		vertices.Add(C);
		uvs.Add(new Vector2(0, 1));
		colors.Add(color);

		vertices.Add(D);
		uvs.Add(Vector2.one);
		colors.Add(color);

		triangles.Add(startIndex);
		triangles.Add(startIndex + 1);
		triangles.Add(startIndex + 2);

		triangles.Add(startIndex);
		triangles.Add(startIndex + 2);
		triangles.Add(startIndex + 3);
	}
}
