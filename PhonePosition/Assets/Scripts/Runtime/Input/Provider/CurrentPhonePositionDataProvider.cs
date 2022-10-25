using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.InputData
{
    public class CurrentPhonePositionDataProvider : AbstractInputDataProvider<string>
    {
        [SerializeField] private GyroscopeDataProvider _gyroscopeDataProvider;

        [SerializeField] private AccelometerDataProvider _accelerometerDataProvider;

        private Vector3 _accelometerData;
        private Quaternion _gyroscopeData;
        private List<Vector3> _lastFiveMeasurementAccelerometer = new List<Vector3>();
        private List<Quaternion> _lastFiveMeasurementGyroscope = new List<Quaternion>();
        private static float _accelerometerDeviation = 0.05f;

        protected override string GetConvertedData()
        {
            return _data;
        }

        protected override void UpdateData()
        {
            _gyroscopeData = _gyroscopeDataProvider.Data;
            _accelometerData = _accelerometerDataProvider.Data;
            _data = GetNewPositionData(_gyroscopeData, _accelometerData);
        }

        private string GetNewPositionData(Quaternion _gyroscopeData, Vector3 _accelometerData)
        {
            AppendRecentMeasurement(_gyroscopeData, _accelometerData);

            if (checkPositionCompatibility(_lastFiveMeasurementAccelerometer, new Vector3(0, 0, -1)))
            {
                return "Your phone is laying still on the table with its screen facing up.";
            }
            if (checkPositionCompatibility(_lastFiveMeasurementAccelerometer, new Vector3(0, 0, 1)))
            {
                return "Your phone is laying still on the table with its screen facing down.";
            }
            if (checkPositionCompatibility(_lastFiveMeasurementAccelerometer, new Vector3(0, 0, -1)))
            {
                return "Your phone is laying still on the table.";
            }
            if (checkPositionCompatibility(_lastFiveMeasurementAccelerometer, new Vector3(0, -1, 0)))
            {
                return "Your phone is standing still on the lower edge.";
            }
            if (checkPositionCompatibility(_lastFiveMeasurementAccelerometer, new Vector3(0, 1, 0)))
            {
                return "Your phone is standing still on the upper edge.";
            }
            if (checkPositionCompatibility(_lastFiveMeasurementAccelerometer, new Vector3(1, 0, 0)))
            {
                return "Your phone is standing still on its right edge.";
            }
            if (checkPositionCompatibility(_lastFiveMeasurementAccelerometer, new Vector3(-1, 0, 0)))
            {
                return "Your phone is standing still on its left edge.";
            }

            return "Your phone is in an unknown state.";
        }

        private void AppendRecentMeasurement(Quaternion _gyroscopeData, Vector3 _accelometerData)
        {
            if (_lastFiveMeasurementGyroscope.Count != 5)
            {
                _lastFiveMeasurementGyroscope.Add(_gyroscopeData);
                _lastFiveMeasurementAccelerometer.Add(_accelometerData);
            }
            else
            {
                _lastFiveMeasurementGyroscope.RemoveAt(0);
                _lastFiveMeasurementAccelerometer.RemoveAt(0);
                _lastFiveMeasurementGyroscope.Add(_gyroscopeData);
                _lastFiveMeasurementAccelerometer.Add(_accelometerData);
            }
        }

        private bool checkPositionCompatibility(List<Vector3> lastFiveMeasurementAccelerometer, Vector3 valuesToCompare)
        {
            foreach (var accelerometerMeasurement in lastFiveMeasurementAccelerometer)
            {
                float x_value =  accelerometerMeasurement.x;
                float y_value =  accelerometerMeasurement.y;
                float z_value =  accelerometerMeasurement.z;
                if (!checkIfEqual(x_value, valuesToCompare.x))
                {
                    return false;
                }
                if (!checkIfEqual(y_value, valuesToCompare.y))
                {
                    return false;
                }
                if (!checkIfEqual(z_value, valuesToCompare.z))
                {
                    return false;
                }
            }

            return true;
        }

        private bool checkIfEqual(float value, float valueToCompare)
        {
            if (Math.Abs(value - valueToCompare) < _accelerometerDeviation)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
