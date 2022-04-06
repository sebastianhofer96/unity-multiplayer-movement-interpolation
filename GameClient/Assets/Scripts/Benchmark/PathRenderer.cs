using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    private LineRenderer m_lineRenderer;
    private Vector3[] m_positions;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
    }

    public void SetSamples(List<BenchmarkSampler.Row> samples)
    {
        // convert to array with positions only
        m_positions = new Vector3[samples.Count];

        for (int i = 0; i < samples.Count; i++)
        {
            m_positions[i] = new Vector3
            {
                x = samples[i].PositionX,
                y = samples[i].PositionY,
                z = samples[i].PositionZ
            };
        }
    }
    
    public void ShowLine(bool showLine)
    {
        if (showLine)
        {
            m_lineRenderer.positionCount = m_positions.Length;
            m_lineRenderer.SetPositions(m_positions);
        }
        else
        {
            m_lineRenderer.positionCount = 0;
        }
    }
}