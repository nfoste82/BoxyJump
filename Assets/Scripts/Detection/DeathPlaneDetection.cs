using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoxyJump
{
	public class DeathPlaneDetection : MonoBehaviour 
	{
		private void Update()
		{
			if (transform.position.y < -1.0f)
			{
				// Fell below death plane
			}
		}
	}
}