using UnityEngine;
using UnityEngine.UI;

public class BenchmarkUIManager : MonoBehaviour
{
    [SerializeField] private BenchmarkManager m_benchmarkManager;
    [SerializeField] private Toggle m_showServerPathToggle;
    [SerializeField] private Toggle m_showClientPathToggle;
    [SerializeField] private Toggle m_showRotationsToggle;

    private void UpdateShowRotationsInteractable()
    {
        m_showRotationsToggle.interactable = m_showServerPathToggle.isOn || m_showClientPathToggle.isOn;
    }
    public void ResetCameraClicked()
    {
        m_benchmarkManager.ResetCamera();
    }

    public void ReloadSamplesClicked()
    {
        m_benchmarkManager.ReloadSamples(m_showServerPathToggle.isOn, m_showClientPathToggle.isOn, m_showRotationsToggle.isOn);
    }

    public void GenerateReportClicked()
    {
        m_benchmarkManager.GenerateReport();
    }

    public void QuitClicked()
    {
        Application.Quit();
    }

    public void ShowServerPathChanged()
    {
        UpdateShowRotationsInteractable();
        m_benchmarkManager.ShowServerPositionsPath(m_showServerPathToggle.isOn);
        m_benchmarkManager.ShowServerRotations(m_showRotationsToggle.isOn && m_showServerPathToggle.isOn);
    }

    public void ShowClientPathChanged()
    {
        UpdateShowRotationsInteractable();
        m_benchmarkManager.ShowClientPositionsPath(m_showClientPathToggle.isOn);
        m_benchmarkManager.ShowClientRotations(m_showRotationsToggle.isOn && m_showClientPathToggle.isOn);
    }

    public void ShowRotationsChanged()
    {
        m_benchmarkManager.ShowServerRotations(m_showRotationsToggle.isOn && m_showServerPathToggle.isOn);
        m_benchmarkManager.ShowClientRotations(m_showRotationsToggle.isOn && m_showClientPathToggle.isOn);
    }
}