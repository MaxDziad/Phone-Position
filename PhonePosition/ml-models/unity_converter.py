import keras2onnx
from tensorflow.keras.models import load_model

model = load_model('model_25_samples_Conv1D.h5')
model.summary()
onnx_model = keras2onnx.convert_keras(model, name="converted_model",
                                      target_opset=9, channel_first_inputs=None)
keras2onnx.save_model(onnx_model, 'model_25_samples_Conv1D.onnx')
