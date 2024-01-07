import { defineStore } from 'pinia'
import { UserModel } from '../metadata/console-jobs-scheduler-api'

export function isUserInRole(user: UserModel, ...roles: string[]): boolean {
  return !!user?.roles?.some(x => roles?.some(y => y === x))
}

export const useUserStore = defineStore('user', {
  state: () => {
    const state: {
      user: UserModel | undefined
    } = { user: undefined }
    return state
  },
  getters: {
    currentUser: (state) => state.user,
  },
  actions: {
    setUser(user: UserModel) {
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