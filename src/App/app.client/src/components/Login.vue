<script setup lang="ts">
import { ref } from 'vue'
import { createApi } from '../api'
import { LoginModel, AuthApi } from '../metadata/console-jobs-scheduler-api'
import { useRouter } from 'vue-router'

const router = useRouter()

const loginModel = ref<LoginModel>({ rememberMe: true })

const authApi = createApi(AuthApi)

async function login() {
    if (loginModel.value) {
        const { data } = await authApi.apiAuthLoginPost(loginModel.value)
        if (data.succeeded) {
            await router.push({ path: '/' })
        }
    }
}
</script>
<template>
    <div class="page-container">
        <div class="container">
            <div class="row">
                <section class="w-100 p-4 d-flex justify-content-center pb-4">
                    <form style="width: 22rem;">
                        <div class="mb-4">
                            <input type="text" id="txtUserName" class="form-control" v-model="loginModel.userName" />
                            <label class="form-label" for="txtUserName">Username</label>
                        </div>

                        <div class="mb-4">
                            <input type="password" id="txtPassword" class="form-control" v-model="loginModel.password" />
                            <label class="form-label" for="txtPassword">Password</label>
                        </div>

                        <div class="row mb-4">
                            <div class="col d-flex justify-content-center">
                                <div class="form-check">
                                    <input class="form-check-input" type="checkbox" value="" id="cbRememberMe" v-model="loginModel.rememberMe" />
                                    <label class="form-check-label" for="cbRememberMe"> Remember me </label>
                                </div>
                            </div>
                        </div>

                        <button type="button" class="btn btn-primary mb-4" @click="login">Sign in</button>
                    </form>
                </section>
            </div>
        </div>
    </div>
</template>