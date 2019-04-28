
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws a flat filled circle or arc using mesh and material.
/// 
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class InWorldProgressBar : MonoBehaviour {

	[NonSerialized] List<Vector3> vertices;
	[NonSerialized] List<int> triangles;
	//[NonSerialized] List<Color> colors;
	[NonSerialized] List<Vector2> uvs;

	private Mesh mesh;

	//public float startAngle = 0;

	[Range(0, 1)]
	public float value = 1f;

	[Range(3, 20)]
	public int segments = 12;

	public float radius = 3f;

	void OnEnable() {
		Setup();
	}

	void Update() {
		Clear();
		FillCircle();
		Apply();
	}

	public void Setup() {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Radius Mesh";
	}

	public void Clear() {
		if (mesh != null) {
			mesh.Clear();
		}

		vertices = ListPool<Vector3>.Get();

		triangles = ListPool<int>.Get();
		//colors = ListPool<Color>.Get();
		uvs = ListPool<Vector2>.Get();
	}

	public void Apply() {
		mesh.SetVertices(vertices);
		ListPool<Vector3>.Add(vertices);

		mesh.SetTriangles(triangles, 0);
		ListPool<int>.Add(triangles);
		mesh.RecalculateNormals();

		//mesh.SetColors(colors);
		//ListPool<Color>.Add(colors);

		mesh.SetUVs(0, uvs);
		ListPool<Vector2>.Add(uvs);
	}

	public void FillCircle() {
		// negative values for the segments or radius don't make sense and cause problems
		segments = Mathf.Max(3, segments);
		radius = Mathf.Max(0, radius);

		float twoPI = Mathf.PI * 2.0f;
		float delta = twoPI / (float)segments;

		float startAngle = Mathf.PI * 0.5f;

		float lastTheta = startAngle;
		Vector3 lastLeg;
		Vector3 leg;
		Vector2 uvCenter = new Vector2(0.5f, 0.5f);
		for (int i = 0; (float)(i - 1) / (float)segments <= value; i++) {
			float theta = startAngle + Mathf.Min(i * delta, value * twoPI);

			lastLeg = new Vector3(Mathf.Cos(lastTheta), Mathf.Sin(lastTheta),0);
			leg = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta),0);

			AddTriangle(Vector3.zero, lastLeg * radius, leg * radius);

			AddTriangleUV(
				uvCenter,
				new Vector2(Mathf.Cos(lastTheta), Mathf.Sin(lastTheta)) * 0.5f + uvCenter,
				new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * 0.5f + uvCenter
			);
			lastTheta = theta;
		}
	}

	public void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
		int vertexIndex = vertices.Count;
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);
		triangles.Add(vertexIndex);
		triangles.Add(vertexIndex + 1);
		triangles.Add(vertexIndex + 2);
	}

	public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector3 uv3) {
		uvs.Add(uv1);
		uvs.Add(uv2);
		uvs.Add(uv3);
	}
}
