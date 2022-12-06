# cnn model
import os

import numpy as np
from numpy import loadtxt
from keras.models import Sequential
from keras.layers import Dense
from keras.layers import Flatten
from keras.layers import Dropout
from keras.layers.convolutional import Conv1D
from keras.layers.convolutional import MaxPooling1D


samples_in_vector = 25

def load_data(path: str):
    dataset = loadtxt(path, delimiter=',')
    columns_number = 6 * samples_in_vector
    x = dataset[:, 0:columns_number]
    y = dataset[:, columns_number]
    return dataset, x, y

def load_dataset():
    new_values = []
    new_labels = []
    dataset, values, labels = load_data("output_data/original_output/output_samples_25.csv")
    for index, single_row in enumerate(values):
        if not labels[index] <= 6:
            continue
        new_labels.append(labels[index] - 1)
        single_row = np.array_split(single_row, 2)
        acc_values = single_row[0]
        acc_array = np.array_split(acc_values, len(acc_values) / 3)
        gyro_values = single_row[1]
        gyro_array = np.array_split(gyro_values, len(gyro_values) / 3)
        sample_vector = []
        for i, single_sample in enumerate(acc_array):
            acc_values = single_sample
            gyro_values = gyro_array[i]
            row = np.concatenate((acc_values, gyro_values))
            sample_vector.append(row)
        new_values.append(np.array(sample_vector))
    return np.array(new_values), np.array(new_labels)

# fit and evaluate a model
def evaluate_model(trainX, trainy):
    verbose, epochs, batch_size = 0, 10, 32
    n_timesteps, n_features, n_outputs = 25, 6, 7 # Input -> (25 x 6), output -> 7 (6) classes
    model = Sequential()
    model.add(Conv1D(filters=64, kernel_size=3, activation='relu', input_shape=(n_timesteps, n_features)))
    model.add(Conv1D(filters=64, kernel_size=3, activation='relu'))
    model.add(Dropout(0.5))
    model.add(MaxPooling1D(pool_size=2))
    model.add(Flatten())
    model.add(Dense(100, activation='relu'))
    model.add(Dense(n_outputs, activation='softmax'))
    model.compile(loss='sparse_categorical_crossentropy', optimizer='adam', metrics=['accuracy'])
    # fit network
    model.fit(trainX, trainy, epochs=epochs, batch_size=batch_size)
    # evaluate model
    cwd = os.getcwd()
    output_model_path = cwd + "/output_data/models/"
    model.save(output_model_path + f"model_{samples_in_vector}_samples_Conv1D.h5")


# run an experiment
def run_experiment():
    trainX, trainy = load_dataset()
    evaluate_model(trainX, trainy)


# run the experiment
run_experiment()
