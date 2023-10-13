import { defineStore } from 'pinia'
import { UserModel } from '../metadata/console-jobs-scheduler-api'

export const useUserStore = defineStore('user', {
  state: () => {
    const state: {
      user: UserModel | undefined
    } = { user: undefined }
    return state
  },
  getters: {
    currentUser: (state) => state.user as UserModel | undefined,
  },
  actions: {
    setUser(user: UserModel | undefined) {
      this.user = user
    }
  },
  persist: {
    storage: sessionStorage
  }
})