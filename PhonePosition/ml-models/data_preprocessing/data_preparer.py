import os


def extract_key_values(line):
    experiment_number_id = line[0]
    user_number_id = line[1]
    activity_number_id = line[2]
    label_start_point = line[3]
    label_ending_point = line[4]
    return activity_number_id, experiment_number_id, label_ending_point, label_start_point, user_number_id


def get_samples(file, label_start_point, label_ending_point):
    return file[int(label_start_point):int(label_ending_point)]


class DataPreparer:
    __ACC_PREFIX = 'acc_exp'
    __GYRO_PREFIX = 'gyro_exp'
    __USER_SUFFIX = '_user'
    __FINAL_SUFFIX = '.txt'
    __LABELS = 'labels'

    def __init__(self, original_input_directory_path: str, output_directory_path: str, samples_in_vector: int):
        self.__original_input_directory_path = original_input_directory_path
        self.__output_directory_path = output_directory_path
        self.__samples_in_vector = samples_in_vector

    def create_csv_original_data_file(self):
        all_files = self.__generate_file_list()
        output_lines = self.__generate_data_from_labels(all_files)
        return self.__write_output_csv(output_lines)

    def __generate_file_list(self):
        all_files = {}
        for filename in os.listdir(self.__original_input_directory_path):
            name, file_extension = os.path.splitext(filename)
            if not name.startswith(self.__LABELS):
                all_files[name] = open(self.__original_input_directory_path + filename, 'r').read().splitlines()
        return all_files

    def __generate_data_from_labels(self, all_files):
        all_lines = list()
        for line in open(self.__original_input_directory_path + self.__LABELS + self.__FINAL_SUFFIX, "r")\
                .read().splitlines():
            line = line.split(' ')
            activity_number_id, experiment_number_id, label_ending_point, label_start_point, user_number_id \
                = extract_key_values(line)

            if len(experiment_number_id) == 1:
                experiment_number_id = '0' + experiment_number_id

            if len(user_number_id) == 1:
                user_number_id = '0' + user_number_id

            actual_acc_file_name = self.__generate_file_name(self.__ACC_PREFIX, experiment_number_id, user_number_id)
            actual_gyro_file_name = self.__generate_file_name(self.__GYRO_PREFIX, experiment_number_id, user_number_id)
            actual_acc_file = all_files[actual_acc_file_name]
            actual_gyro_file = all_files[actual_gyro_file_name]
            acc_lines = get_samples(actual_acc_file, label_start_point, label_ending_point)
            gyro_lines = get_samples(actual_gyro_file, label_start_point, label_ending_point)
            new_lines = self.__generate_lines(acc_lines, gyro_lines, activity_number_id)
            all_lines.extend(new_lines)
        return all_lines

    def __generate_file_name(self, prefix, experiment_number_id, user_number_id):
        return prefix + experiment_number_id + self.__USER_SUFFIX + user_number_id

    def __generate_lines(self, acc_lines, gyro_lines, activity_number_id):
        lines = []
        actual_gyro_lines = []
        actual_acc_lines = []

        for index, line in enumerate(acc_lines):
            acc_line = line.split(' ')
            gyro_line = gyro_lines[index].split(' ')
            actual_gyro_lines.extend(acc_line)
            actual_acc_lines.extend(gyro_line)
            if (index + 1) % self.__samples_in_vector == 0:
                new_line = []
                new_line.extend(actual_gyro_lines)
                new_line.extend(actual_acc_lines)
                new_line.append(activity_number_id)
                lines.append(','.join(new_line))
                actual_gyro_lines = []
                actual_acc_lines = []
        return lines

    def __write_output_csv(self, all_lines):
        output_path = self.__output_directory_path + f"output_samples_{self.__samples_in_vector}.csv"
        with open(output_path, "w+") as outfile:
            outfile.write("\n".join(all_lines))
            return output_path
