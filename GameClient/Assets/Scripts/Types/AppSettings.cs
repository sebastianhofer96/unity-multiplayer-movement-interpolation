using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AppSettings", order = 1)]
public class AppSettings : ScriptableObject
{
    public int FrameRateHz;
    public int SimulationRateHz;
    public int NetworkRateHz;
    public bool UseInterpolation;
    public float ThresholdDistance;
    public int BufferSize;
    public PositionAlgorithm PositionAlgorithm;
    public RotationAlgorithm RotationAlgorithm;
    public bool UseSimulationInput;
    public bool SaveSamples;
    public string ServerAddress;
}

public enum PositionAlgorithm
{
    Linear, CatmullRom
}

public enum RotationAlgorithm
{
    Slerp, Squad
}