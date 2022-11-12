import numpy as np
from matplotlib import pyplot as plt
from numpy import loadtxt, ndarray


class DataDownsampler:

    def __init__(self, original_output_file_path: str, output_directory_path: str, samples_in_vector: int,
                 equalise_sets: bool, remove_redundant_classes: bool):
        self.__original_output_file_path = original_output_file_path
        self.__output_directory_path = output_directory_path
        self.__samples_in_vector = samples_in_vector
        self.__equalise_sets = equalise_sets
        self.__remove_redundant_classes = remove_redundant_classes
        if remove_redundant_classes:
            self.__class_numbers_range = 6
        else:
            self.__class_numbers_range = 12

    def create_csv_downsampled_data_file(self):
        dataset, values, labels = self.__load_data(self.__original_output_file_path)
        row_list, smallest_set_size = self.__get_smallest_set_size(dataset)
        dataset_downsampled = self.__downsample_classes(row_list, smallest_set_size)
        output_path = self.__save_downsampled_data(dataset_downsampled)
        _, _, downsampled_labels = self.__load_data(output_path)
        self.__plot_histograms(labels, downsampled_labels)
        return output_path, self.__class_numbers_range

    def __load_data(self, path: str):
        dataset = loadtxt(path, delimiter=',')
        columns_number = 6 * self.__samples_in_vector
        x = dataset[:, 0:columns_number]
        y = dataset[:, columns_number]
        return dataset, x, y

    def __get_smallest_set_size(self, dataset: ndarray):
        row_list = []
        smallest_set_size = 100000000000000000
        for i in range(self.__class_numbers_range + 1):
            if i != 0:
                rows = dataset[dataset[:, 6 * self.__samples_in_vector] == float(i), :]
                if rows.shape[0] < smallest_set_size:
                    smallest_set_size = rows.shape[0]
                row_list.append(rows)
        return row_list, smallest_set_size

    def __downsample_classes(self, row_list, smallest_set_size: int):
        list_downsampled = []
        for row in row_list:
            if self.__equalise_sets:
                rnd_indices = np.random.choice(len(row), size=smallest_set_size)
                list_downsampled.extend(row[rnd_indices])
            else:
                list_downsampled.extend(row)
        return list_downsampled

    def __save_downsampled_data(self, downsampled_set) -> str:
        output_path = self.__output_directory_path + f"output_downsampled_{self.__samples_in_vector}.csv"
        np.savetxt(output_path, downsampled_set, delimiter=",")
        return output_path

    def __plot_histograms(self, original_labels: ndarray, downsampled_labels: ndarray):
        plt.hist(original_labels, bins='auto')
        plt.title("Data distribution before")
        plt.show()
        plt.hist(downsampled_labels, bins='auto')
        plt.title("Data distribution after downsampling")
        if self.__equalise_sets and self.__remove_redundant_classes:
            plt.title("Data distribution after downsampling and removing redundant classes")
        elif self.__equalise_sets:
            plt.title("Data distribution after downsampling")
        elif self.__remove_redundant_classes:
            plt.title("Data distribution after removing redundant classes")
        else:
            plt.title("Data distribution has not changed with that parameters combination")
        plt.show()
