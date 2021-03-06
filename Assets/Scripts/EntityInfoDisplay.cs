﻿using System.Collections;
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
		public Text m_responses;

		public Text m_results;

		public void NewEntityMade(GameObject entity, GeneticData data)
		{
			m_entity = entity;

			m_generationText.text = "Generation: " + data.generation;

			DisplayResults();

			DisplayResponses();
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

		private void DisplayResponses()
		{
			AIComponent aiComp = m_entity.GetComponent<AIComponent>();

			var sb = new StringBuilder();
			sb.AppendLine("Receptors/Responses:");

			Dictionary<ReceptorType, Response> responses = aiComp.GeneticData.m_responses;
			foreach (var kvp in responses)
			{
				// TODO: Create FastToString to prevent enum.ToString() perf hits
				sb.AppendLine("Has receptor: " + aiComp.GeneticData.m_receptors.Contains(kvp.Key));
				sb.AppendLine(kvp.Key.ToString() + ", " + kvp.Value.GetType().Name);
				sb.AppendLine("Odds: " + kvp.Value.data.odds + ", Amount: " + kvp.Value.data.amount);

				if (kvp.Value is JumpResponse)
				{
					sb.AppendLine("Angle: " + kvp.Value.data.secondaryValue);
				}
				sb.AppendLine();
			}

			m_responses.text = sb.ToString();
		}

		private void Update()
		{
			var aiComp = m_entity.GetComponent<AIComponent>();

			// TODO: Optimize to reduce garbage.
			m_scoreText.text = "Score: " + m_entityController.GetScore(aiComp.transform.position.x).ToString("0.00");
		}
	}
}