import { UserModel } from "./metadata/console-jobs-scheduler-api";

export class AuthHelper {
    private static _instance: AuthHelper = new AuthHelper()

    onAuthenticate: (u: UserModel) => void = () => {}
    
    static getInstance(): AuthHelper {
        return this._instance
    }
}