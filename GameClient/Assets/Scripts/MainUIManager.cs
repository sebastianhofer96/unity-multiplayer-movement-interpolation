using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    [SerializeField] private AppSettings m_appSettings;

    [SerializeField] private GameObject m_settingsScreen;
    [SerializeField] private GameObject m_connectionFailedRow;
    [SerializeField] private GameObject m_connectingScreen;
    [SerializeField] private GameObject m_statisticsScreen;

    [SerializeField] private InputField m_frameRateField;
    [SerializeField] private InputField m_simulationRateField;
    [SerializeField] private InputField m_networkRateField;
    [SerializeField] private Toggle m_useInterpolationToggle;
    [SerializeField] private InputField m_thresholdDistanceField;
    [SerializeField] private InputField m_bufferSizeField;
    [SerializeField] private Dropdown m_positionAlgorithmDropdown;
    [SerializeField] private Dropdown m_rotationAlgorithmDropdown;
    [SerializeField] private Toggle m_useSimulationInputToggle;
    [SerializeField] private Toggle m_saveSamplesToggle;
    [SerializeField] private InputField m_serverAddressField;

    [SerializeField] private Text m_playerInputText;
    [SerializeField] private Text m_pingText;
    [SerializeField] private Text m_tickText;

    private void Update()
    {
        // esc to disconnect
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            NetworkManager.Instance.DisconnectFromServer();
        }

        // update statistics
        if (NetworkManager.Instance.Client.IsConnected)
        {
            m_pingText.text = NetworkManager.Instance.Client.RTT != -1 ? NetworkManager.Instance.Client.RTT.ToString() : "-";
            m_tickText.text = NetworkManager.Instance.Tick.ToString();
        }
    }

    private void SetCursorMode(bool isUiCursor)
    {
        Cursor.visible = isUiCursor;
        Cursor.lockState = isUiCursor ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void ApplySettings()
    {
        // overwrite app settings
        m_appSettings.FrameRateHz = int.Parse(m_frameRateField.text);
        m_appSettings.SimulationRateHz = int.Parse(m_simulationRateField.text);
        m_appSettings.NetworkRateHz = int.Parse(m_networkRateField.text);
        m_appSettings.UseInterpolation = m_useInterpolationToggle.isOn;
        m_appSettings.ThresholdDistance = float.Parse(m_thresholdDistanceField.text);
        m_appSettings.BufferSize = int.Parse(m_bufferSizeField.text);
        m_appSettings.PositionAlgorithm = (PositionAlgorithm)m_positionAlgorithmDropdown.value;
        m_appSettings.RotationAlgorithm = (RotationAlgorithm)m_rotationAlgorithmDropdown.value;
        m_appSettings.UseSimulationInput = m_useSimulationInputToggle.isOn;
        m_appSettings.SaveSamples = m_saveSamplesToggle.isOn;
        m_appSettings.ServerAddress = m_serverAddressField.text;

        // check simulation rate and network rate requirement
        Assert.IsTrue(m_appSettings.SimulationRateHz % m_appSettings.NetworkRateHz == 0);

        // set FixedUpdate rate according to client tick rate
        Time.fixedDeltaTime = 1f / m_appSettings.SimulationRateHz;
        QualitySettings.vSyncCount = 0;

        // set frame rate
        Application.targetFrameRate = m_appSettings.FrameRateHz;
    }

    public void UseInterpolationChanged()
    {
        m_thresholdDistanceField.interactable = m_useInterpolationToggle.isOn;
        m_bufferSizeField.interactable = m_useInterpolationToggle.isOn;
        m_positionAlgorithmDropdown.interactable = m_useInterpolationToggle.isOn;
        m_rotationAlgorithmDropdown.interactable = m_useInterpolationToggle.isOn;
    }

    public void ConnectClicked()
    {
        ApplySettings();

        m_statisticsScreen.SetActive(false);
        m_settingsScreen.SetActive(false);
        m_connectionFailedRow.SetActive(false);
        m_connectingScreen.SetActive(true);

        // establish server connection
        NetworkManager.Instance.Connect(m_appSettings.ServerAddress);
    }

    public void QuitClicked()
    {
        Application.Quit();
    }

    public void IsConnected()
    {
        m_statisticsScreen.SetActive(true);
        m_settingsScreen.SetActive(false);
        m_connectionFailedRow.SetActive(false);
        m_connectingScreen.SetActive(false);

        SetCursorMode(false);
    }

    public void ConnectionFailed()
    {
        m_statisticsScreen.SetActive(false);
        m_settingsScreen.SetActive(true);
        m_connectionFailedRow.SetActive(true);
        m_connectingScreen.SetActive(false);

        SetCursorMode(true);
    }

    public void SetPlayerInputText(string text)
    {
        m_playerInputText.text = text;
    }

    public void UseSimulationInputChanged()
    {
        m_saveSamplesToggle.interactable = m_useSimulationInputToggle.isOn;
    }

    public void PositionAlgorithmChanged()
    {
        m_rotationAlgorithmDropdown.value = m_positionAlgorithmDropdown.value;
    }

    public void RotationAlgorithmChanged()
    {
        m_positionAlgorithmDropdown.value = m_rotationAlgorithmDropdown.value;
    }
}