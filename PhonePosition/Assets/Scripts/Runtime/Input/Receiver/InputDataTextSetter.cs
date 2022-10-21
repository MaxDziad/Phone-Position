using TMPro;
using UnityEngine;

namespace Runtime.InputData
{
    public class InputDataTextSetter : MonoBehaviour, IInputTextSetter
    {
        [SerializeField]
        private TextMeshProUGUI _dataTextField;

        public void SetText(string data)
        {
            _dataTextField.text = data;
        }
    }
}
