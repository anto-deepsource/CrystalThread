using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMap;

[RequireComponent(typeof(MeshFilter),typeof(MeshRenderer), typeof(PolyShape))]
public class PolyShapeMesh : MonoBehaviour {

	public void Setup() {
		PolyShape shape = GetComponent<PolyShape>();

		Vector2[] vertices2D = shape.GetPoints2D();

		Triangulator trian = new Triangulator(vertices2D);
		int[] indices = trian.Triangulate();

		// Create the Vector3 vertices
		Vector3[] vertices = new Vector3[vertices2D.Length];
		for (int i = 0; i < vertices.Length; i++) {
			vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
		}

		// Create the mesh
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = indices;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		MeshFilter filter = GetComponent<MeshFilter>();
		filter.mesh = mesh;
	}
}
