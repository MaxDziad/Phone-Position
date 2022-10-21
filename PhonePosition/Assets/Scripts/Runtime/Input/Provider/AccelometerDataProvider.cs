using UnityEngine;

namespace Runtime.InputData
{
    public class AccelometerDataProvider : AbstractInputDataProvider<Vector3>
    {
        protected override string GetConvertedData()
        {
            return _data.ToString();
        }

        protected override void UpdateData()
        {
            _data = UnityEngine.Input.acceleration;
}
    }
}
