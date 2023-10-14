import { Router } from "vue-router"
import { AuthApi } from "./metadata/console-jobs-scheduler-api"
import axios from "axios"
import { createApi } from "./api"
import { isUserInRole, useUserStore } from "./stores/userStore"

export class AuthHelper {
  authApi: AuthApi = createApi(AuthApi)
  userStore = useUserStore()

  private async getUser() {
    let user = this.userStore.currentUser
    if (!user) {
      const { data } = await this.authApi.apiAuthGetUserGet()
      user = data
      this.userStore.setUser(user)
    }
    return user
  }

  configureRouter(router: Router) {
    router.beforeEach(async (to, _, next) => {
      if (to.meta.requiresAuth) {
        const user = await this.getUser()
        const roles = to.meta.roles as string[] | undefined
        if (user && (!roles || isUserInRole(user, ...roles))) {
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
}