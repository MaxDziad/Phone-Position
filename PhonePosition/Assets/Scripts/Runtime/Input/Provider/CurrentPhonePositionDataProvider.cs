using UnityEngine;

namespace Runtime.InputData
{
    public class CurrentPhonePositionDataProvider : AbstractInputDataProvider<string>
    {
        [SerializeField]
        private GyroscopeDataProvider _gyroscopeDataProvider;

        [SerializeField]
        private AccelometerDataProvider _accelometerDataProvider;

        private Vector3 _accelometerData;
        private Quaternion _gyroscopeData;

        protected override string GetConvertedData()
        {
            return _data;
        }

        protected override void UpdateData()
        {
            _gyroscopeData = _gyroscopeDataProvider.Data;
            _accelometerData = _accelometerDataProvider.Data;
            _data = GetNewPositionData();
        }

        private string GetNewPositionData()
        {
            return string.Empty;
        }
    }
}
