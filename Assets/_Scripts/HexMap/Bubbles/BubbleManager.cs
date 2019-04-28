using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HexMap {
	/// <summary>
	/// Tracks a point in x-z space (probably the player) and ensures that they are always contained
	/// inside a bubble.
	/// -Upon no bubbles -> creates a bubble on top of the player and then creates neighboring bubbles
	///		around that bubble
	/// -Upon the player entering a new bubble -> checks the new bubble for any adjacent empty space 
	///		and fills with new bubbles
	/// -Bubbles are generated so that overlap between adjacent bubbles is avoided but not uncommon
	/// -Bubbles are generated so that empty space between adjacent bubbles is always bounded:
	///		-From any point within an empty space, there is no direction or ray that does not eventually
	///			collide with some bubble
	/// </summary>
	public class BubbleManager : Singleton<BubbleManager> {
		private BubbleManager() { }

		public GameObject target;

		//public float radiusMean = 5;
		//public float radiusVariance = 2;
		public float radiusMin = 4;
		public float radiusMax = 7;

		private List<Bubble> allBubbles = new List<Bubble>();

		private Bubble currentBubble;

		public static QuickEvent Events { get { return Instance._events; } }
		private QuickEvent _events = new QuickEvent();

		private void OnEnable() {
			if ( target==null ) {
				QueryManager.GetPlayer(out target);
			}
		}
		
		private void Update() {
			if (target==null) {
				return;
			}
			Vector2 tarPoint = target.transform.position.JustXZ();

			Bubble closestBubble = GetBubbleContainingPoint(tarPoint);
			
			if ( currentBubble==null ) {
				// possibly the first time we've ever checked, and we have no bubbles yet
				if (closestBubble == null) {
					currentBubble = CreateNewBubble(tarPoint, RandomRadius(), BubbleType.Maze );
					SurroundBubbleWithBubbles(currentBubble);
				}
			} else 
			if ( closestBubble != currentBubble ) {
				currentBubble = closestBubble;
				SurroundBubbleWithBubbles(currentBubble);
			}
		}

		/// <summary>
		/// Searches all the bubbles and returns the bubble closest or overlapping the given point, if there is one.
		/// </summary>
		/// <param name="point"></param>
		/// <returns></returns>
		public Bubble GetBubbleContainingPoint( Vector2 point ) {
			Bubble closestBubble = null;
			float min = -1;
			foreach( var bubble in allBubbles ) {
				float distance = ColDet.DistanceFromPointToCircle(point, bubble.center, bubble.radius);
				// if the bubble contains that point just return this bubble
				if ( distance < 0 ) {
					return bubble;
				}
				if (closestBubble == null || distance < min ) {
					closestBubble = bubble;
					min = distance;
				}
			}

			// return the closest bubble
			return closestBubble;
		}

		public Bubble CreateNewBubble(Vector2 center, float radius, BubbleType type ) {
			Bubble newBubble = new Bubble();
			newBubble.center = center;
			newBubble.radius = radius;
			newBubble.type = type;
			allBubbles.Add(newBubble);
			Events.Fire(BubbleEvent.NewBubble, newBubble);
			return newBubble;
		}

		public float RandomRadius() {
			float value = Random.value * 2f - 1.0f;
			return radiusMin + (radiusMax - radiusMin) * value;
		}

		public void SurroundBubbleWithBubbles( Bubble centerBubble ) {
			// get a list of all the bubbles within a certain distance
			List<float> leftSides = new List<float>();
			// the other side is linked with the left side
			Dictionary<float,float> rightSides = new Dictionary<float, float>();

			//SortedList<float, Bubble> touchingBubbles = new SortedList<float, Bubble>();
			foreach( var bubble in allBubbles ) {
				if (bubble!=centerBubble && ColDet.CircleAndCircle( 
						centerBubble.center, centerBubble.radius + radiusMax,
						bubble.center, bubble.radius )) {
					// order the circles we found by the angle between them and the center
					float theta = CommonUtils.ThetaBetween(centerBubble.center, bubble.center);
					float width = Mathf.Atan2(bubble.radius, centerBubble.radius + bubble.radius);

					float leftSide = theta - width;
					// if by some miracle two left sides happen to be exactly the same, just remember the larger bubble
					if ( rightSides.ContainsKey( leftSide ) ) {
						float bigger = Mathf.Max(rightSides[leftSide], theta + width);
						rightSides[leftSide] = bigger;
					} else {
						leftSides.Add(leftSide);
						rightSides[leftSide] = theta + width;
					}
					
					// if the bubble's edges cross over the Q2 - Q3 boundary -> add them twice, 360 degrees apart
					if ( theta > 0 && theta + width > Mathf.PI ) {
						leftSide = theta - width - Mathf.PI * 2f;
						// if by some miracle two left sides happen to be exactly the same, just remember the larger bubble
						if (rightSides.ContainsKey(leftSide)) {
							float bigger = Mathf.Max(rightSides[leftSide], theta + width - Mathf.PI * 2f);
							rightSides[leftSide] = bigger;
						} else {
							leftSides.Add(leftSide);
							rightSides[leftSide] = theta + width - Mathf.PI * 2f;
						}
					}else 
					if ( theta < 0 && theta - width < -Mathf.PI ) {
						leftSide = theta - width + Mathf.PI * 2f;
						// if by some miracle two left sides happen to be exactly the same, just remember the larger bubble
						if (rightSides.ContainsKey(leftSide)) {
							float bigger = Mathf.Max(rightSides[leftSide], theta + width + Mathf.PI * 2f);
							rightSides[leftSide] = bigger;
						} else {
							leftSides.Add(leftSide);
							rightSides[leftSide] = theta + width + Mathf.PI * 2f;
						}
					}

					//touchingBubbles.Add(theta, bubble);
				}
			}

			// if there are no touching bubbles
			if (leftSides.Count == 0 ) {
				float theta = Random.value * Mathf.PI;// start at a random angle

				int count = Random.Range(6, 11);

				CreateBubblesOnArc(centerBubble.center, centerBubble.radius, theta, Mathf.PI * 2f);
				
				// call it done
				return;
			}

			leftSides.Sort();

			//we move CCW around the center circle
			// this leftSide represents the start of an adjacent bubble
			//float leftSide = leftSides[0];

			float rightSide = rightSides[leftSides[0]];

			// we simply continue CCW checking whether the next edge is a leftside or rightside
			for( int i = 1; i < leftSides.Count; i ++ ) {
				float nextLeftSide = leftSides[i];
				// if the start of the next circle is BEFORE the end of the current bubble -> overlap, not empty space
				if (nextLeftSide < rightSide) {

				} else
				// there is some empty space between this circle and the next
				{
					float space = nextLeftSide - rightSide;

					// fill that space with some bubbles
					CreateBubblesOnArc(centerBubble.center, centerBubble.radius, rightSide, space);
				}

				// keep the ending edge of which ever circle is bigger or at least ends later
				rightSide = Mathf.Max(rightSide, rightSides[nextLeftSide]);
				
			}

			// we also need to consider the wrap-around
			{
				float leastLeftSide = leftSides[0];
				float mostLeftSide = leftSides[leftSides.Count - 1];
				float mostRightSide = rightSides[mostLeftSide];
				float rightSideAdjusted = mostRightSide - Mathf.PI * 2f;

				float space = leastLeftSide - rightSideAdjusted;

				// fill that space with some bubbles
				CreateBubblesOnArc(centerBubble.center, centerBubble.radius, rightSide, space);
			}
			
		}

		private void CreateBubblesOnArc( Vector2 center, float centerRadius, float startTheta, float length ) {

			// we want the biggest bubbles we can fit
			// if the length is more that pi/4 then just use pi/4
			float a = length > Mathf.PI / 2 ? Mathf.PI / 2 : length;
			float maxRadius = Mathf.Min( radiusMax, RadiusFromTheta(a, centerRadius) );

			// if our biggest circle is smaller than our minimum just be done
			if ( maxRadius < radiusMin ) {
				return;
			}

			// calulcate the distance across the arc over center of our bubbles
			float arcLength = ArcLength(length, centerRadius + maxRadius);

			// the number of bubbles we can fit along this arc is approx 
			// equal to the arc length divided by the diameters
			float b = arcLength / (maxRadius * 2f);

			// we'll always be either half a bubble too much or half a bubble not enough
			// choose one way or the other randomly
			//int count = Random.Range(Mathf.FloorToInt(b), Mathf.CeilToInt(b) + 1);
			int count = Mathf.CeilToInt(b);

			float[] subdivisions = CommonUtils.VariedSubdivide(length, count, Mathf.PI / 12f);
			foreach (var sub in subdivisions) {
				// sub will be an angle, let's add half of it to theta to get the angle to the center of our bubble
				float halfSub = sub * 0.5f;
				startTheta += halfSub;

				float r = RadiusFromTheta(sub, centerRadius);
				r = Mathf.Min(r, radiusMax);
				r = Mathf.Max(r, radiusMin);

				Vector2 bubbleCenter = center + CommonUtils.AngleToVector(startTheta) * (r + centerRadius);
				CreateNewBubble(bubbleCenter, r, BubbleType.Empty);

				startTheta += halfSub;
			}
		}

		public float RadiusFromTheta( float theta, float centerRadius ) {
			//float sigma = Mathf.Sin(theta*0.5f);
			float sigma = Mathf.Tan(theta * 0.5f); // tan will cause them to overlap a lil
			return (centerRadius * sigma) / (1 - sigma);
		}

		public float ArcLength( float theta, float radius ) {
			return radius * theta;
		}
	}

	public enum BubbleEvent {
		NewBubble = 2,
	}
}