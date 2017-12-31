using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BoxyJump
{
	public class EntityInfoDisplay : MonoBehaviour
	{
		public GameObject m_entity;
		public EntityController m_entityController;

		public Text m_generationText;
		public Text m_scoreText;
		public Text m_horzThrust;
		public Text m_horzOdds;
		public Text m_jumpStrength;
		public Text m_jumpOdds;

		public Text m_results;

		public void NewEntityMade(GameObject entity, GeneticData data)
		{
			m_entity = entity;

			m_generationText.text = "Generation: " + data.generation;
			m_horzThrust.text = "Horz. Thrust: " + data.horizontalThrust.ToString("0.0");
			m_horzOdds.text = "Horz. Odds/s: " + (data.thrustOddsPerSecond * 100.0f).ToString("0") + "%";
			m_jumpStrength.text = "Jump Strength: " + data.jumpStrength.ToString("0.0");
			m_jumpOdds.text = "Jump Odds/s: " + (data.jumpOddsPerSecond * 100.0f).ToString("0") + "%";

			DisplayResults();
		}

		private void DisplayResults()
		{
			var sb = new StringBuilder();

			for (int i = 0; i < m_entityController.ScoresToGeneticData.Count; ++i)
			{
				var kvp = m_entityController.ScoresToGeneticData[i];

				int generation = i;
				float score = kvp.Key;

				sb.AppendLine("Gen: " + generation + ", Score: " + score);
			}

			m_results.text = sb.ToString();
		}

		private void Update()
		{
			var aiComp = m_entity.GetComponent<AIComponent>();

			// TODO: Optimize to reduce garbage.
			m_scoreText.text = "Score: " + aiComp.transform.position.x.ToString("0.00");
		}
	}
}