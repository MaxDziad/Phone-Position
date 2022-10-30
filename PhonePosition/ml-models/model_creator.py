from keras import Sequential
from keras.layers import Dense, Dropout
from numpy import loadtxt, ndarray


class ModelCreator:

    def __init__(self, output_preprocessed_data_path: str, output_model_path: str, samples_in_vector: int, classes_number: int):
        self.__output_preprocessed_data_path = output_preprocessed_data_path
        self.__samples_in_vector = samples_in_vector
        self.__classes_number = classes_number
        self.__output_model_path = output_model_path

    def create_model(self):
        dataset, values, labels = self.__load_data(self.__output_preprocessed_data_path)
        model = Sequential()
        model.add(Dense(units=64, kernel_initializer='normal', activation='sigmoid', input_dim=values.shape[1]))
        model.add(Dropout(0.2))
        model.add(Dense(128, activation='relu'))
        model.add(Dropout(0.2))
        model.add(Dense(64, activation='relu'))
        model.add(Dense(units=self.__classes_number + 1, kernel_initializer='normal', activation='softmax'))
        model.compile(optimizer='adam', loss='sparse_categorical_crossentropy', metrics=['accuracy'])
        model.fit(values, labels, batch_size=120, epochs=30)
        model.save(self.__output_model_path + f"model_{self.__samples_in_vector}_samples")

    def __load_data(self, path: str) -> tuple[ndarray, ndarray, ndarray]:
        dataset = loadtxt(path, delimiter=',')
        columns_number = 6 * self.__samples_in_vector
        x = dataset[:, 0:columns_number]
        y = dataset[:, columns_number]
        return dataset, x, y
