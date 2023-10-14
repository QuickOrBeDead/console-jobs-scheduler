<script setup lang="ts">
import { ref, onMounted, Ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { createApi } from '../../api'
import { UsersApi } from '../../metadata/console-jobs-scheduler-api'

interface UserEditModel {
    userName: string | undefined | null
    password: string | undefined | null
    roles: string[] | undefined | null
}

const route = useRoute()
const router = useRouter()

const user = ref<UserEditModel>() as Ref<UserEditModel>
const roles = ref<string[]>() as Ref<string[]>

let userId: number | undefined
let isInEditMode: boolean

const usersApi = createApi(UsersApi)

const loadPage = async () => {
    userId = !route.params.userId ? 0 : parseInt(route.params.userId as string, 10)
    isInEditMode = !!userId

    const { data } = await usersApi.apiUsersGetRolesGet()
    roles.value = data

    if (isInEditMode) {
        const { data } = await usersApi.apiUsersGetUserUserIdGet(userId)
        user.value = {
            userName: data.userName,
            password: '',
            roles: data.roles
        }
    }
    else {
        user.value = {
            userName: '',
            password: '',
            roles: []
        }
    }
}

onMounted(async () => {
   await loadPage()
})

watch(
  () => route.params, 
  loadPage,
  {
    deep:true
  }
)

async function save() {
    if (user.value) {
        const { data } = await usersApi.apiUsersPost({
            id: userId,
            userName: user.value.userName!,
            password: '',
            roles: user.value.roles
        })

        if (isInEditMode) {
            await loadPage()
        } else {
            if (data.succeeded) {
                await router.push({ name: 'EditUser', params: { userId: userId }})
            }
        }
    }
}
</script>

<template>

<template v-if="user">
    <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <h1 class="display-6">{{ isInEditMode ? "Edit" : "Add" }} User</h1>
                    <hr style="margin: 4px 0px;">
                </div>
            </div>
            <div class="row justify-content-center">
                <div class="col-6">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label for="Username" class="form-label">Username</label>
                            <input id="Username" type="text" class="form-control" :disabled="isInEditMode" v-model="user.userName"/>
                        </div>
                        <div class="col-md-6">
                            <label for="Password" class="form-label">Password</label>
                            <input id="Password" type="password" class="form-control" v-model="user.password"/>
                        </div>
                        <div class="col-md-12">
                            <label class="form-label">Roles</label>
                            <ul style="list-style-type: none;">
                                <li v-for="role in roles">
                                    <label>
                                        <input :value="role" :checked="user.roles?.some(x => x === role)" v-model="user.roles" :id="role" type="checkbox" />
                                        {{ role }}
                                    </label>
                                </li>
                            </ul>
                        </div>
                        <button class="btn btn-primary" @click="save">{{ isInEditMode ? "Edit" : "Add" }} User</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
</template>