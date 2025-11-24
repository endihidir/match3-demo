using System.Collections.Generic;
using UnityEngine;

namespace Core.Extensions
{
	public static class Vector3Extensions
	{
		public static float Distance(this Vector3 vector, Vector3 pos)
		{
			var xDist = Mathf.Abs(vector.x - pos.x);
			var yDist = Mathf.Abs(vector.y - pos.y);
			var zDist = Mathf.Abs(vector.z - pos.z);

			return (xDist + yDist + zDist) / 3f;
		}
		
		public static float DistanceXY(this Vector3 a, Vector3 b) => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) * 0.5f;
		public static float DistanceXZ(this Vector3 a, Vector3 b) => (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.z - b.z)) * 0.5f;
		public static float DistanceYZ(this Vector3 a, Vector3 b) => (Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) * 0.5f;

		public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
		{
			return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
		}
		
		public static Vector3Int With(this Vector3Int vector, int? x = null, int? y = null, int? z = null)
		{
			return new Vector3Int(x ?? vector.x, y ?? vector.y, z ?? vector.z);
		}


		public static Vector3 Add(this Vector3 vector, float? x = null, float? y = null, float? z = null) 
		{
			return new Vector3(vector.x + (x ?? 0), vector.y + (y ?? 0), vector.z + (z ?? 0));
		}
		
		public static Vector3Int Add(this Vector3Int vector, int? x = null, int? y = null, int? z = null)
		{
			return new Vector3Int(vector.x + (x ?? 0), vector.y + (y ?? 0), vector.z + (z ?? 0));
		}

		public static Vector3 SetLength(this Vector3 v, float length)
		{
			return v.normalized * length;
		}
		
		public static Vector3 GetClosest(this Vector3 v, Vector3[] to)
		{
			Vector3 closestPos = Vector3.zero;

			var minDist = float.MaxValue;

			var targetsLength = to.Length;

			for (int i = 0; i < targetsLength; i++)
			{
				var potentialT = to[i] - v;

				potentialT.y = 0f;

				if (potentialT.sqrMagnitude < minDist)
				{
					minDist = potentialT.sqrMagnitude;

					closestPos = to[i];
				}
			}

			return closestPos;
		}

		public static T GetClosest<T>(this Vector3 v, T[] to) where T : Component
		{
			T closest = null;

			var minDist = float.MaxValue;

			var targetsLength = to.Length;

			for (int i = 0; i < targetsLength; i++)
			{
				var potentialT = to[i].transform.position - v;

				potentialT.y = 0f;

				if (potentialT.sqrMagnitude < minDist)
				{
					minDist = potentialT.sqrMagnitude;

					closest = to[i];
				}
			}

			return closest;
		}

		public static Vector3 WorldToScreen(this Vector3 v, Camera cam)
		{
			var pos = cam.WorldToScreenPoint(v);

			return pos;
		}

		public static Vector3 ViewPortToScreen(this Vector3 v, Camera cam)
		{
			var pos = cam.ViewportToScreenPoint(v);

			return pos;
		}

		public static Vector3 ScreenToCanvas(this Vector3 v, Camera cam)
		{
			var xPos = v.x.Remap(0f, 0f, Screen.width, 1f);
			var yPos = v.y.Remap(0f, 0f, Screen.height, 1f);

			var screenPos = cam.ViewportToScreenPoint(new Vector3(xPos, yPos));

			return screenPos;
		}

		public static Vector3 SelectSimulationSpace(this Vector3 v, PositionSpace positionSpace)
		{
			switch (positionSpace)
			{
				case PositionSpace.CanvasSpace:
					return v;
				case PositionSpace.ScreenSpace:
					return v.ScreenToCanvas(Camera.allCameras[0]);
				case PositionSpace.WorldSpace:
					return v.WorldToScreen(Camera.allCameras[0]);
				case PositionSpace.ViewPortSpace:
					return v.ViewPortToScreen(Camera.allCameras[0]);
				default:
					return v;
			}
		}

		public static List<Vector3> CatmullRomSpline(this List<Vector3> points, int subdivisions = 10)
		{
			if (points.Count < 4)
			{
				Debug.LogError("CatmullRomSpline requires at least 4 points.");
				return new List<Vector3>();
			}

			var splinePoints = new List<Vector3>();

			for (int i = 0; i < points.Count - 2; i++)
			{
				var p0 = i == 0 ? points[0] : points[i - 1];
				var p1 = points[i];
				var p2 = points[i + 1];
				var p3 = i == points.Count - 4 ? points[^1] : points[i + 2];

				for (int j = 0; j < subdivisions; j++)
				{
					var t = j / (float)subdivisions;

					var t2 = t * t;
					var t3 = t2 * t;

					var v0 = (p2 - p0) * 0.5f;
					var v1 = (p3 - p1) * 0.5f;
					var a = 2f * p1 - 2f * p2 + v0 + v1;
					var b = -3f * p1 + 3f * p2 - 2f * v0 - v1;
					var c = v0;
					var d = p1;

					var position = a * t3 + b * t2 + c * t + d;
					splinePoints.Add(position);
				}
			}

			splinePoints.Add(points[^1]);

			return splinePoints;
		}

		public static float PathDistance(this Vector3[] path)
		{
			var pathLength = 0f;

			for (int i = 0; i < path.Length - 1; i++)
			{
				pathLength += Vector3.Distance(path[i], path[i + 1]);
			}

			return pathLength;
		}
	}

	public enum PositionSpace
	{
		CanvasSpace,
		WorldSpace,
		ScreenSpace,
		ViewPortSpace
	}
}