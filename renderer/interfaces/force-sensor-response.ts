
export interface ForceSensorData {
    "fx": number,
    "fy": number,
    "fz": number,
    "mx": number,
    "my": number,
    "mz": number,
}

export interface RobotPosition {
    "x": number,
    "y": number,
    "z": number,
    "role": number,
    "pitch": number,
    "yaw": number,
}

export interface ForceSensorResponseSingle {
    "force": {
        "sensor-1": ForceSensorData|null,
        "time": number,
    },
    "robot": {
        "tcp": RobotPosition|null,
        "time": number,
    },
    "time": number,
}



export interface ForceSensorResponseDouble {
    "sensor-1": ForceSensorData|null,
    "sensor-2": ForceSensorData|null,
    "combined": ForceSensorData|null,
    "time-stamp": number,
}
