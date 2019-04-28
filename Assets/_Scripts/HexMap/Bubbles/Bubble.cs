using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	/// <summary>
	/// Defines a circular area on the hexmap.
	/// Contains hextiles and further defines their parameters and behaviors, ie:
	///		-whether it is a maze and when to generate it
	///		-kinds of objects that occasionaly spawn and how frequently
	///		-wall height?
	///		-terrain height?
	///		-terrain texture?
	///	A hextile usually belongs to one bubble but may sometimes belong to zero or two or more.
	///	
	/// </summary>
	public class Bubble {
		/// <summary>
		/// The x-z coordinates for the center of the bubble.
		/// </summary>
		public Vector2 center;

		public float radius;

		public BubbleType type;
	}

	public enum BubbleType {
		Empty,
		Maze,
	}
}
