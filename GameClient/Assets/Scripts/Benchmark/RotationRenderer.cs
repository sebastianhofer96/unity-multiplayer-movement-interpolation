using System.Collections.Generic;
using UnityEngine;

public class RotationRenderer : MonoBehaviour
{
    private Vector3[,] m_positions;

    public void SetSamples(List<BenchmarkSampler.Row> samples, float lineLength, Color color)
    {
        // destroy all children
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // convert to array with positions only
        m_positions = new Vector3[samples.Count, 2];

        for (int i = 0; i < samples.Count; i++)
        {
            // set start point
            m_positions[i, 0] = new Vector3
            {
                x = samples[i].PositionX,
                y = samples[i].PositionY,
                z = samples[i].PositionZ
            };

            // set end point
            Quaternion rotation = Quaternion.Euler(samples[i].RotationX, samples[i].RotationY, 0f);
            Vector3 forward = rotation * Vector3.forward;
            m_positions[i, 1] = m_positions[i, 0] + forward.normalized * lineLength;

            // create child game object for line renderer
            GameObject lineGameObject = new GameObject();
            lineGameObject.transform.SetParent(transform, false);
            lineGameObject.transform.SetAsFirstSibling(); // needed to know position as children are destroyed at end of frame
            LineRenderer lineRenderer = lineGameObject.AddComponent<LineRenderer>();
            lineRenderer.widthMultiplier = 0.01f;
            lineRenderer.material.color = color;
            lineRenderer.useWorldSpace = false;
        }
    }

    public void ShowLines(bool showLines)
    {
        // iterate children
        for (int i = 0; i < m_positions.GetLength(0); i++)
        {
            LineRenderer lineRenderer = transform.GetChild(i).GetComponent<LineRenderer>();

            if (showLines)
            {
                lineRenderer.positionCount = 2;
                lineRenderer.SetPositions(new Vector3[] { m_positions[i, 0], m_positions[i, 1] });
            }
            else
            {
                lineRenderer.positionCount = 0;
            }
        }
    }
}