using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace Runtime.InputData
{
    public class CurrentPhonePositionDataProvider : AbstractInputDataProvider<string>
    {
        [SerializeField] 
        private GyroscopeDataProvider _gyroscopeDataProvider;

        [SerializeField] 
        private AccelometerDataProvider _accelerometerDataProvider;
        
        [SerializeField] 
        private NNModel _kerasModel;

        private Model _runtimeModel;
        private IWorker _worker;
        private string _outputLaterName;
        
        private Vector3 _accelometerData;
        private Quaternion _gyroscopeData;

        private readonly List<Vector3> _lastFiveAccelometerDatas = new();
        private readonly List<Quaternion> _lastFiveMeasurementGyroscope = new();

        private const float ACCEPTABLE_MEASUREMENT_ERROR = 0.03f;

        void Start()
        {
            _runtimeModel = ModelLoader.Load(_kerasModel);
            _worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _runtimeModel);
            _outputLaterName = _runtimeModel.outputs[_runtimeModel.outputs.Count - 1];
            Predict();
        }

        public void Predict()
        {
            float[] rawInputs =
            {
                0.7083334f,-0.1972222f, 0.0958333f, 0.7569445f,-0.1736111f, 0.1694444f, 0.9027778f,-0.1694444f, 0.1472222f, 0.9708334f,-0.1833333f, 0.1180556f, 0.9722223f,-0.1763889f, 0.1666667f, 0.9236112f,-0.1569445f, 0.1680556f, 0.9375001f,-0.1444445f, 0.1972222f, 1.0472222f,-0.1083333f, 0.2194445f, 1.1763889f,-0.0458333f, 0.2472222f, 1.2305556f, 0.0125000f, 0.2722222f, 1.2388889f, 0.0180556f, 0.3513889f, 1.2958334f,-0.1305556f, 0.3597222f, 1.4763890f,-0.4000000f, 0.2750000f, 1.5930556f,-0.6069445f, 0.0777778f, 1.2888889f,-0.7125000f,-0.1194444f, 0.9277778f,-0.5652778f, 0.0486111f, 0.6500000f,-0.3986111f,-0.0138889f, 0.6777778f,-0.2625000f,-0.0680556f, 0.8319445f,-0.2236111f,-0.1444445f, 0.9791667f,-0.2708333f,-0.1541667f, 1.0277779f,-0.2944444f,-0.1444445f, 1.0833333f,-0.3847222f,-0.1347222f, 1.0486111f,-0.4000000f,-0.1138889f, 0.7750000f,-0.3319445f, 0.0027778f, 0.6208334f,-0.2388889f, 0.0333333f,-0.7510588f, 0.3454443f, 0.0381791f,-0.5455027f, 0.2189952f, 0.0464258f,-0.4657848f, 0.4401284f,-0.0458149f,-0.3576616f, 0.5039638f,-0.2064725f,-0.3127630f, 0.6426302f,-0.3097087f,-0.3982841f, 0.4770858f,-0.3051272f,-0.5183192f, 0.2528982f,-0.2629775f,-0.7214319f, 0.1603521f,-0.1786781f,-0.9001099f, 0.1142318f,-0.0882700f,-1.0458013f,-0.0244346f,-0.0442877f,-1.0821478f,-0.2419026f, 0.0085521f,-1.0253373f,-0.3246749f, 0.1740966f,-0.4456262f,-0.2480113f, 0.4807509f, 0.2635883f, 0.4401284f, 0.4599815f, 0.4575381f, 0.7449502f, 0.0708604f, 0.6490443f, 0.2296853f,-0.0302378f, 0.4205807f,-0.3570507f,-0.0045815f, 0.2000584f,-0.6148359f, 0.2348777f,-0.1588250f,-0.7831292f, 0.2590069f,-0.3802636f,-0.8686504f, 0.1493566f,-0.3878994f,-0.8701776f, 0.1285871f,-0.1896737f,-0.4630359f, 0.0503964f,-0.1093449f,-0.2409863f,-0.2357940f,-0.0232129f,-0.3888157f,-0.4098906f, 0.0992656f,-0.7861836f,-0.2999348f
            };
            TensorShape tensorShape = new TensorShape(1, 1, rawInputs.Length, 1);
            Tensor input = new Tensor(tensorShape, rawInputs);
            Tensor outputTensor = _worker.Execute(input).PeekOutput(_outputLaterName);
            Debug.LogFormat(outputTensor.ArgMax()[0].ToString());
        }

        protected override string GetConvertedData()
        {
            return _data;
        }

        protected override void UpdateData()
        {
            _gyroscopeData = _gyroscopeDataProvider.Data;
            _accelometerData = _accelerometerDataProvider.Data;
            _data = GetCurrentPhonePosition();
        }

        private string GetCurrentPhonePosition()
        {
            AddRecentMeasurements();

            if (CheckPositionCompatibility(Vector3.back))
            {
                return "Your phone is laying still on the table with its screen facing up.";
            }

            if (CheckPositionCompatibility(Vector3.forward))
            {
                return "Your phone is laying still on the table with its screen facing down.";
            }

            if (CheckPositionCompatibility(Vector3.down))
            {
                return "Your phone is standing still on the lower edge.";
            }

            if (CheckPositionCompatibility(Vector3.up))
            {
                return "Your phone is standing still on the upper edge.";
            }

            if (CheckPositionCompatibility(Vector3.right))
            {
                return "Your phone is standing still on its right edge.";
            }

            if (CheckPositionCompatibility(Vector3.left))
            {
                return "Your phone is standing still on its left edge.";
            }

            return "Your phone is in an unknown state.";
        }

        private void AddRecentMeasurements()
        {
            if (_lastFiveMeasurementGyroscope.Count == 5)
            {
                _lastFiveMeasurementGyroscope.RemoveAt(0);
            }

            if (_lastFiveAccelometerDatas.Count == 5)
            {
                _lastFiveAccelometerDatas.RemoveAt(0);
            }

            _lastFiveMeasurementGyroscope.Add(_gyroscopeData);
            _lastFiveAccelometerDatas.Add(_accelometerData);
        }

        private bool CheckPositionCompatibility(Vector3 comparer)
        {
            foreach (var accelerometerData in _lastFiveAccelometerDatas)
            {
                if (Vector3.Distance(accelerometerData, comparer) > ACCEPTABLE_MEASUREMENT_ERROR)
                {
                    return false;
                }
            }

            return true;
        }

        private void OnDestroy()
        {
            _lastFiveAccelometerDatas.Clear();
            _lastFiveMeasurementGyroscope.Clear();
        }
    }
}
