using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlow;

#if UNITY_ANDROID
//TensorFlowSharp.Android.NativeBinding.Init();
#endif

public class TrainedModel : MonoBehaviour
{
    private TextAsset _graphModel = Resources.Load<TextAsset>("saved_model");
    private TFGraph _graph = new TFGraph();
    private TFSession _session = null;

    // Start is called before the first frame update
    void Start()
    {
        _graph.Import(_graphModel.bytes);
        _session = new TFSession(_graph);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
