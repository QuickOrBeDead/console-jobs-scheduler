import { AxiosError, AxiosResponse } from "axios"
import { BaseAPI } from "./metadata/console-jobs-scheduler-api/base"
import { ValidationProblemDetails } from "./metadata/console-jobs-scheduler-api"
import { Ref } from "vue"

export function createApi<T extends BaseAPI>(ctor: { new (): T }) : T {
    const api = new ctor()
    api["basePath"] = ""
    return api
}

export async function callApi<T>(func: () => Promise<AxiosResponse<T, any>>, validationErrorsRef: Ref<{[key: string]: string[]}>): Promise<{data: T | undefined, hasError: boolean}> {
    try {
        validationErrorsRef.value = {}
        return { data: (await func()).data, hasError: false }
    } catch (error: unknown) {
        const axiosError = error as AxiosError<ValidationProblemDetails, any>
        if (axiosError && axiosError.response && axiosError.response.data && axiosError.response.data.errors)
        {
            validationErrorsRef.value = axiosError.response.data.errors
            return { data: undefined, hasError: true }
        }

        throw error
    }
}