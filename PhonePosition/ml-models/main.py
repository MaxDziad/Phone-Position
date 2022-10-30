import os

from data_preprocessing.data_preparer import DataPreparer
from data_preprocessing.data_downsampler import DataDownsampler
from model_creator import ModelCreator


if __name__ == "__main__":
    cwd = os.getcwd()
    input_directory_path = cwd + "/input_data/"
    output_csv_directory_path = cwd + "/output_data/original_output/"
    output_csv_downsampled_directory_path = cwd + "/output_data/downsampled_output/"
    output_model_path = cwd + "/output_data/models/"
    samples_in_vector = 25

    output_original_path = DataPreparer(input_directory_path, output_csv_directory_path, samples_in_vector)\
        .create_csv_original_data_file()

    output_downsampled_path, classes_number = DataDownsampler(output_original_path,
                                                              output_csv_downsampled_directory_path, samples_in_vector,
                                                              equalise_sets=False, remove_redundant_classes=True)\
        .create_csv_downsampled_data_file()

    ModelCreator(output_downsampled_path, output_model_path, samples_in_vector, classes_number).create_model()

