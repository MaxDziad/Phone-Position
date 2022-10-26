using System.Collections.Generic;
using UnityEngine;

namespace Runtime.InputData
{
    public class CurrentPhonePositionDataProvider : AbstractInputDataProvider<string>
    {
        [SerializeField] 
        private GyroscopeDataProvider _gyroscopeDataProvider;

        [SerializeField] 
        private AccelometerDataProvider _accelerometerDataProvider;

        private Vector3 _accelometerData;
        private Quaternion _gyroscopeData;

        private readonly List<Vector3> _lastFiveAccelometerDatas = new();
        private readonly List<Quaternion> _lastFiveMeasurementGyroscope = new();

        private const float ACCEPTABLE_MEASUREMENT_ERROR = 0.03f;

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
