using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public static class VectorUtils
	{
		public static Vector2 Vector2FromAngleDegrees(float degrees)
		{
			degrees *= Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(degrees), Mathf.Sin(degrees));
		}

		public static Vector2 Vector2FromAngleRadians(float radians)
		{
			return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
		}
	}
}