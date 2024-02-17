import { defineStore } from 'pinia'
import { UserContext } from '../metadata/console-jobs-scheduler-api'

export function isUserInRole(user: UserContext, ...roles: string[]): boolean {
  return !!user?.roles?.some(x => roles?.some(y => y === x))
}

export const useUserStore = defineStore('user', {
  state: () => {
    const state: {
      user: UserContext | undefined
    } = { user: undefined }
    return state
  },
  getters: {
    currentUser: (state) => state.user,
  },
  actions: {
    setUser(user: UserContext) {
      this.user = user
    },
    removeUser() {
      this.user = undefined
    },
    isUserInRole(...roles: string[]): boolean {
      return !!this.user?.roles?.some(x => roles?.some(y => y === x))
    }
  },
  persist: {
    storage: sessionStorage
  }
})