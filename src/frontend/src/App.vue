<script setup lang="ts">
import { AuthApi } from './metadata/console-jobs-scheduler-api';
import { createApi } from './api';
import { useRouter } from 'vue-router';
import { storeToRefs } from 'pinia';
import { useUserStore, isUserInRole } from './stores/userStore';

const userStore = useUserStore()
const { user } = storeToRefs(userStore)

const router = useRouter()

const authApi = createApi(AuthApi)

async function logout() {
  await authApi.apiAuthLogoutPost()

  userStore.setUser(undefined)

  router.push({ path: '/' })
}
</script>

<template>
  <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
    <div class="container-fluid">
      <a class="navbar-brand" href="/">Console Jobs</a>
      <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarColor02" aria-controls="navbarColor02" aria-expanded="false" aria-label="Toggle navigation">
        <span class="navbar-toggler-icon"></span>
      </button>
      <div class="collapse navbar-collapse" id="navbarColor02" v-if="user">
        <ul class="navbar-nav me-auto mb-2 mb-lg-0">
          <li class="nav-item">
            <router-link class="nav-link" aria-current="page" to="/">Scheduler</router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" to="/jobs">Jobs</router-link>
          </li>
          <li class="nav-item">
            <router-link class="nav-link" to="/history">History</router-link>
          </li>
          <li class="nav-item" v-if="isUserInRole(user, 'Admin')">
            <router-link class="nav-link" to="/packages">Packages</router-link>
          </li>
        </ul>
        <div class="d-flex align-items-center">
          <span class="navbar-text me-1">
            {{ user?.userName }}
          </span>
          <button type="button" class="btn btn-primary me-3" @click="logout">
            Logout
          </button>
        </div>
      </div>
    </div>
  </nav>
  <router-view></router-view>
</template>

<style scoped>
.logo {
  height: 6em;
  padding: 1.5em;
  will-change: filter;
  transition: filter 300ms;
}
.logo:hover {
  filter: drop-shadow(0 0 2em #646cffaa);
}
.logo.vue:hover {
  filter: drop-shadow(0 0 2em #42b883aa);
}
</style>
