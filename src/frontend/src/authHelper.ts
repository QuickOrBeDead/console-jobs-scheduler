import { Router } from "vue-router";
import { AuthApi, UserModel } from "./metadata/console-jobs-scheduler-api";
import axios from "axios";
import { createApi } from "./api";

export class AuthHelper {
    private static _instance: AuthHelper = new AuthHelper()

    authApi: AuthApi

    constructor() {
        this.authApi = createApi(AuthApi)
    }

    onAuthenticate: (u: UserModel) => void = () => {}

    configureRouter(router: Router) {
        router.beforeEach(async (to, _, next) => {
            if (to.matched.some(record => record.meta.requiresAuth)) {
              const user = (await this.authApi.apiAuthGetUserGet())?.data
              if (user) {
                if (this.onAuthenticate) {
                  this.onAuthenticate(user)
                }
                
                next()
              } else {
                next({ name: 'Login' })
              }
            } else {
              next()
            }
          })

        axios.interceptors.response.use((r: any) => {
            return r
         }, (error: any) => {
           if (error.response.status === 401 || error.response.status === 403) {
            router.push({ path: '/' })
           }
           return error
         })
        }

    static getInstance(): AuthHelper {
        return this._instance
    }
}