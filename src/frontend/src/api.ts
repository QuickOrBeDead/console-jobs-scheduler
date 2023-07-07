import { BaseAPI } from "./metadata/console-jobs-scheduler-api/base"

export function createApi<T extends BaseAPI>(ctor: { new (): T }) : T {
    const api = new ctor()
    api["basePath"] = "http://localhost:5000"
    return api
}