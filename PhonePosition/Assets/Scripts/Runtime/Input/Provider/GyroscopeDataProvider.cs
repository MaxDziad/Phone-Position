using UnityEngine;

namespace Runtime.InputData
{
    public class GyroscopeDataProvider : AbstractInputDataProvider<Vector3>
    {
        private bool _isGyroscopeEnabled;

        private void Start()
        {
            InitializeGyroscope();
        }

        private void InitializeGyroscope()
        {
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
                _isGyroscopeEnabled = true;
            }
            else
            {
                Input.gyro.enabled = false;
                Debug.LogWarning("Device is not supporting gyroscope.");
            }
        }

        protected override string GetConvertedData()
        {
            return _data.ToString();
        }

        protected override void UpdateData()
        {
            if (_isGyroscopeEnabled)
            {
                _data = Input.gyro.userAcceleration;
            }
        }

        private Quaternion GyroToUnity(Quaternion quaternion)
        {
            return new Quaternion(quaternion.x, quaternion.y, -quaternion.z, -quaternion.w);
        }
    }
}
