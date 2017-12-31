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
		public Text m_jumpAngle;

		public Text m_results;

		public void NewEntityMade(GameObject entity, GeneticData data)
		{
			m_entity = entity;

			m_generationText.text = "Generation: " + data.generation;
			m_horzThrust.text = "Horz. Thrust: " + data.horizontalThrust.ToString("0.0");
			m_horzOdds.text = "Horz. Odds/s: " + (data.thrustOddsPerSecond * 100.0f).ToString("0") + "%";
			m_jumpStrength.text = "Jump Strength: " + data.jumpStrength.ToString("0.0");
			m_jumpOdds.text = "Jump Odds/s: " + (data.jumpOddsPerSecond * 100.0f).ToString("0") + "%";
			m_jumpAngle.text = "Jump Angle: " + data.jumpAngle.ToString("0.0");

			DisplayResults();
		}

		private void DisplayResults()
		{
			var sb = new StringBuilder();

			sb.AppendLine("Top 3 Scores:");
			// Top 3 results
			List<KeyValuePair<float, GeneticData>> topScores = m_entityController.GetTopScores(3);
			for (int i = 0; i < topScores.Count; ++i)
			{
				sb.AppendLine("Gen: " + topScores[i].Value.generation + ", Score: " + topScores[i].Key);
			}
			sb.AppendLine();

			sb.AppendLine("10 most recent:");

			// 10 most recent results
			int startIndex = System.Math.Max(m_entityController.ScoresToGeneticData.Count - 10, 0);
			for (int i = startIndex; i < m_entityController.ScoresToGeneticData.Count; ++i)
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
			m_scoreText.text = "Score: " + m_entityController.GetScore(aiComp.transform.position.x).ToString("0.00");
		}
	}
}