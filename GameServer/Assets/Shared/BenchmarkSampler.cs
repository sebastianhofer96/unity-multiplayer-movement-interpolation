using System.Collections.Generic;
using UnityEngine;

public class BenchmarkSampler
{
    private static List<Row> m_samples = new List<Row>();
    private static int m_intentionCount = 0;

    public static void Initialize(int intentionCount)
    {
        m_intentionCount = intentionCount;
    }

    public static void Add(Vector3 position, Vector2 rotation)
    {
        // only store samples for intention count in simulation input
        if (m_intentionCount == 0 || m_samples.Count >= m_intentionCount) return;

        // add sample to list
        m_samples.Add(new Row
        {
            Tick = m_samples.Count + 1,
            PositionX = position.x,
            PositionY = position.y,
            PositionZ = position.z,
            RotationX = rotation.x,
            RotationY = rotation.y
        });

        // export samples when simulation is finished
        if (m_samples.Count == m_intentionCount)
        {
            Debug.Log("Saving Samples");
            Utils.ExportAsCSV(m_samples, $"{Application.productName}_{Utils.SAMPLES_FILE}");
        }
    }

    public class Row
    {
        public long Tick { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float RotationX { get; set; }
        public float RotationY { get; set; }
    }
}
