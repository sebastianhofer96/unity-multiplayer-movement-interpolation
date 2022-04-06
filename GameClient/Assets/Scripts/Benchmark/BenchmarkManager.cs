using System.Collections.Generic;
using UnityEngine;

public class BenchmarkManager : MonoBehaviour
{
    [SerializeField] private float m_rotationLineLength;
    [SerializeField] private PathRenderer m_serverPositionsPath;
    [SerializeField] private PathRenderer m_clientPositionsPath;
    [SerializeField] private RotationRenderer m_serverRotationsRenderer;
    [SerializeField] private RotationRenderer m_clientRotationsRenderer;
    [SerializeField] private BenchmarkReport m_benchmarkReport;
    [SerializeField] private CameraMover m_cameraMover;

    private List<BenchmarkSampler.Row> m_serverSamples;
    private List<BenchmarkSampler.Row> m_clientSamples;

    private void Start()
    {
        // load with default state
        LoadSamples(true, true, true);
    }

    private void LoadSamples(bool showServer, bool showClient, bool showRotations)
    {
        // import samples
        m_serverSamples = Utils.ImportFromCSV<BenchmarkSampler.Row>($"GameServer_{Utils.SAMPLES_FILE}");
        m_clientSamples = Utils.ImportFromCSV<BenchmarkSampler.Row>($"GameClient_{Utils.SAMPLES_FILE}");

        // set samples
        m_benchmarkReport.SetSamples(m_serverSamples, m_clientSamples);
        m_serverPositionsPath.SetSamples(m_serverSamples);
        m_clientPositionsPath.SetSamples(m_clientSamples);
        m_serverRotationsRenderer.SetSamples(m_serverSamples, m_rotationLineLength, new Color(0.78f, 0f, 0f));
        m_clientRotationsRenderer.SetSamples(m_clientSamples, m_rotationLineLength, new Color(0f, 0f, 0.78f));

        // show path and rotations
        ShowServerPositionsPath(showServer);
        ShowClientPositionsPath(showClient);
        ShowServerRotations(showRotations && showServer);
        ShowClientRotations(showRotations && showClient);
    }

    public void ReloadSamples(bool showServer, bool showClient, bool showRotations)
    {
        LoadSamples(showServer, showClient, showRotations);
    }

    public void ShowServerPositionsPath(bool showLine)
    {
        m_serverPositionsPath.ShowLine(showLine);
    }

    public void ShowClientPositionsPath(bool showLine)
    {
        m_clientPositionsPath.ShowLine(showLine);
    }

    public void ShowServerRotations(bool showRotations)
    {
        m_serverRotationsRenderer.ShowLines(showRotations);
    }

    public void ShowClientRotations(bool showRotations)
    {
        m_clientRotationsRenderer.ShowLines(showRotations);
    }

    public void GenerateReport()
    {
        m_benchmarkReport.GenerateReport();
    }

    public void ResetCamera()
    {
        m_cameraMover.Reset();
    }
}