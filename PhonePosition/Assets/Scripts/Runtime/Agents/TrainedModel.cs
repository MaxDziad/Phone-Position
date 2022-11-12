using UnityEngine;
using TensorFlow;

namespace Runtime.Agents
{
    public class TrainedModel : MonoBehaviour
    {
        [SerializeField]
        private TextAsset _model;

        private TFGraph _graph = new TFGraph();
        private TFSession.Runner _runner;

        void Start()
        {
#if UNITY_ANDROID
            //TensorFlowSharp.Android.NativeBinding.Init();
#endif
            _graph.Import(_model.bytes);
            var session = new TFSession(_graph);
            _runner = session.GetRunner();
        }

        void Update()
        {
            _runner.AddInput(_graph["input_placeholder_name"][0], new float[] { 1, 2 });
            _runner.Fetch(_graph["output_placeholder_name"][0]);
            float[,] recurrent_tensor = _runner.Run()[0].GetValue() as float[,];
        }
    }
}
