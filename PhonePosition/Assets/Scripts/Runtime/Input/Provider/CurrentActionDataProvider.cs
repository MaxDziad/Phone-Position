using Runtime.InputData;
using Runtime.Utils;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class CurrentActionDataProvider : AbstractInputDataProvider<string>
{
    [SerializeField]
    private GyroscopeDataProvider _gyroscopeDataProvider;

    [SerializeField]
    private AccelometerDataProvider _accelerometerDataProvider;

    [SerializeField]
    private NNModel _kerasModel;

    private float[] _rawData = new float[150];
    private List<Vector3> _bufforedAccelometerData = new();
    private List<Quaternion> _bufforedGyroscopeData = new();

    private readonly TensorShape _tensorShape = new TensorShape(1, 1, 150, 1);
    private Model _runtimeModel;
    private IWorker _worker;
    private string _outputLaterName;

    private Tensor _input;
    private Tensor _output;

    private const int DATA_BUFFOR_SIZE = 25;

    void Start()
    {
        InitializeKerasModel();
    }

    private void InitializeKerasModel()
    {
        _runtimeModel = ModelLoader.Load(_kerasModel);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, _runtimeModel);
        _outputLaterName = _runtimeModel.outputs[_runtimeModel.outputs.Count - 1];
    }

    protected override string GetConvertedData()
    {
        return _data;
    }

    protected override void UpdateData()
    {
        UpdateBufforData(ref _bufforedGyroscopeData, _gyroscopeDataProvider);
        UpdateBufforData(ref _bufforedAccelometerData, _accelerometerDataProvider);

        if (IsDataReady())
        {
            PrepareRawData();
            _data = GetCurrentActionFromModel();
            CleanTensors();
        }
    }

    private void UpdateBufforData<TType>(ref List<TType> dataList, AbstractInputDataProvider<TType> dataProvider)
    {
        if (dataList.Count == DATA_BUFFOR_SIZE)
        {
            dataList.RemoveAt(0);
        }

        dataList.Add(dataProvider.Data);
    }

    private bool IsDataReady()
    {
        return _bufforedAccelometerData.Count == DATA_BUFFOR_SIZE
            && _bufforedGyroscopeData.Count == DATA_BUFFOR_SIZE;
    }

    private void PrepareRawData()
    {
        var rawData = new List<float>();

        _bufforedAccelometerData.ForEach(sample => rawData.AddRange(new float[] { sample.x, sample.y, sample.z }));
        _bufforedGyroscopeData.ForEach(sample => rawData.AddRange(GetSampledEulerAngles(sample)));
        _bufforedAccelometerData.Clear();
        _bufforedGyroscopeData.Clear();

        _rawData = rawData.ToArray();
    }

    private float[] GetSampledEulerAngles(Quaternion quaternion)
    {
        return new float[] { quaternion.eulerAngles.x, quaternion.eulerAngles.y, quaternion.eulerAngles.z };
    }

    private string GetCurrentActionFromModel()
    {
        var outputAction = CalculateActionType();

        return (ModelActionType)outputAction switch
        {
            ModelActionType.Walking => "Walking",
            ModelActionType.Laying => "Laying",
            ModelActionType.Sitting => "Sitting",
            ModelActionType.Walking_Downstairs => "Walking Downstairs",
            ModelActionType.Walking_Upstairs => "Walking Upstairs",
            ModelActionType.Standing => "Standing",
            _ => "Unknown"
        };
    }

    private int CalculateActionType()
    {
        _input = new Tensor(_tensorShape, _rawData);
        _output = _worker.Execute(_input).CopyOutput(_outputLaterName);

        return _output.ArgMax()[0];
    }

    private void CleanTensors()
    {
        _input.Dispose();
        _output.Dispose();
        _input = null;
        _output = null;
    }

    private void OnDestroy()
    {
        _bufforedAccelometerData.Clear();
        _bufforedGyroscopeData.Clear();
        _worker?.Dispose();
    }
}
