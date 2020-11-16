import { type } from "os"

interface ForceSensorData {
    fx: number,
    fy: number,
    fz: number,
    mx: number,
    my: number,
    mz: number,
}

interface ForceSensorResponse {
    "sensor-1": ForceSensorData|null,
    "sensor-2": ForceSensorData|null,
    "time-stamp": number,
}

export type {ForceSensorData, ForceSensorResponse}