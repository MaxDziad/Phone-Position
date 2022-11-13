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

    private Model _runtimeModel;
    private IWorker _worker;
    private string _outputLaterName;

    private const int DATA_BUFFOR_SIZE = 25;

    void Start()
    {
        InitializeKerasModel();
    }

    private void InitializeKerasModel()
    {
        _runtimeModel = ModelLoader.Load(_kerasModel);
        _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _runtimeModel);
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
        return new float [] { quaternion.x, quaternion.y, quaternion.z };
        //return new float[] { quaternion.eulerAngles.x, quaternion.eulerAngles.y, quaternion.eulerAngles.z };
    }

    private string GetCurrentActionFromModel()
    {
        var outputAction = CalculateActionType();

        var lol = (ModelActionType)outputAction switch
        {
            ModelActionType.Walking => "Walking",
            ModelActionType.Laying => "Laying",
            ModelActionType.Sitting => "Sitting",
            ModelActionType.Walking_Downstairs => "Walking Downstairs",
            ModelActionType.Walking_Upstairs => "Walking Upstairs",
            ModelActionType.Standing => "Standing",
            _ => "Unknown"
        };

        Debug.Log(lol);
        return lol;
    }

    private int CalculateActionType()
    {
        TensorShape tensorShape = new TensorShape(1, 1, _rawData.Length, 1);
        Tensor input = new Tensor(tensorShape, _rawData);
        Tensor outputTensor = _worker.Execute(input).PeekOutput(_outputLaterName);

        return outputTensor.ArgMax()[0];
    }

    private void OnDestroy()
    {
        _bufforedAccelometerData.Clear();
        _bufforedGyroscopeData.Clear();
        _worker.Dispose();
    }
}
