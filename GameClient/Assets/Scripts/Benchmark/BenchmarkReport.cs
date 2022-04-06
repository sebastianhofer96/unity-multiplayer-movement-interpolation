using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BenchmarkReport : MonoBehaviour
{
    private List<BenchmarkSampler.Row> m_serverSamples;
    private List<BenchmarkSampler.Row> m_clientSamples;

    private List<Row> GetReport()
    {
        List<Row> report = new List<Row>();

        for (int i = 0; i < m_serverSamples.Count; i++)
        {
            Assert.AreEqual(m_serverSamples[i].Tick, m_clientSamples[i].Tick, "Compared sample ticks are not equal");

            Vector3 serverPosition = new Vector3(m_serverSamples[i].PositionX, m_serverSamples[i].PositionY, m_serverSamples[i].PositionZ);
            Vector3 clientPosition = new Vector3(m_clientSamples[i].PositionX, m_clientSamples[i].PositionY, m_clientSamples[i].PositionZ);
            float positionError = Vector3.Distance(serverPosition, clientPosition);

            Vector2 serverRotation = new Vector2(m_serverSamples[i].RotationX, m_serverSamples[i].RotationY);
            Vector3 clientRotation = new Vector2(m_clientSamples[i].RotationX, m_clientSamples[i].RotationY);
            float rotationError = Quaternion.Angle(Quaternion.Euler(serverRotation), Quaternion.Euler(clientRotation));

            report.Add(new Row
            {
                Tick = m_serverSamples[i].Tick,
                ServerPositionX = serverPosition.x,
                ServerPositionZ = serverPosition.z,
                ClientPositionX = clientPosition.x,
                ClientPositionZ = clientPosition.z,
                PositionError = positionError,
                PositionErrorSquared = positionError * positionError,
                ServerRotationX = serverRotation.x,
                ServerRotationY = serverRotation.y,
                ClientRotationX = clientRotation.x,
                ClientRotationY = clientRotation.y,
                RotationError = rotationError,
                RotationErrorSquared = rotationError * rotationError
            });
        }

        return report;
    }

    public void GenerateReport()
    {
        Assert.AreEqual(m_serverSamples.Count, m_clientSamples.Count, "Server and client sample count are not equal");

        List<Row> report = GetReport();
        Utils.ExportAsCSV(report, Utils.REPORT_FILE);
        Debug.Log("Benchmark report generated.");
    }

    public void SetSamples(List<BenchmarkSampler.Row> serverSamples, List<BenchmarkSampler.Row> clientSamples)
    {
        m_serverSamples = serverSamples;
        m_clientSamples = clientSamples;
    }

    public struct Row
    {
        public long Tick { get; set; }
        public float ServerPositionX { get; set; }
        public float ServerPositionZ { get; set; }
        public float ClientPositionX { get; set; }
        public float ClientPositionZ { get; set; }
        public float PositionError { get; set; }
        public float PositionErrorSquared { get; set; }
        public float ServerRotationX { get; set; }
        public float ServerRotationY { get; set; }
        public float ClientRotationX { get; set; }
        public float ClientRotationY { get; set; }
        public float RotationError { get; set; }
        public float RotationErrorSquared { get; set; }
    }
}