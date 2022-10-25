using UnityEngine;

namespace Runtime.InputData
{
    public abstract class AbstractInputDataProvider<TData> : MonoBehaviour, IInputDataProvider
    {
        [SerializeField]
        private InputDataTextSetter _textSetter;

        [SerializeField]
        private float _timeTreshold = 0.01f;

        protected TData _data;
        private float _currentTime;

        public TData Data => _data;

        protected abstract string GetConvertedData();
        protected abstract void UpdateData();

        public void SendData()
        {
            _textSetter.SetText(GetConvertedData());
        }

        public virtual void Update()
        {
            if (_currentTime > _timeTreshold)
            {
                _currentTime = 0;
                UpdateData();
                SendData();
            }
            else
            {
                _currentTime += Time.deltaTime;
            }
        }
    }
}
