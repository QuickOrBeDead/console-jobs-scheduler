<script setup lang="ts">
import { ref, onMounted, Ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { callApi, createApi } from '../../api'
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
const errors = ref<{[key: string]: string[]}>({})
const errorMessages = ref<string[]>([])

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
    errorMessages.value = []

    const { data } = await callApi(() => usersApi.apiUsersPost({
        id: userId,
        userName: user.value.userName!,
        password: user.value.password,
        roles: user.value.roles
    }), errors)

    if (data?.succeeded) {
        if (isInEditMode) {
            await loadPage()
        } else {
            await router.push({ name: 'EditUser', params: { userId: data.userId } })
        }
    } else if (data?.errors) {
        errorMessages.value = data.errors.map(x => x.description as string)
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
                    <div v-if="errorMessages && errorMessages.length" class="alert alert-danger" role="alert">
                        <div v-for="msg in errorMessages" class="d-flex align-items-center">
                            <i class="bi bi-exclamation-triangle-fill"></i>&nbsp;
                            <div>{{ msg }}</div>
                        </div>
                    </div>
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label for="Username" class="form-label">Username</label>
                            <input id="Username" type="text" class="form-control" :disabled="isInEditMode" v-model="user.userName" :class="errors && errors.UserName ? 'is-invalid' : ''"/>
                            <div v-if="errors && errors.UserName" class="invalid-feedback" role="alert"><template v-for="msg in errors.UserName">{{ msg }}<br></template></div>
                        </div>
                        <div class="col-md-6">
                            <label for="Password" class="form-label">Password</label>
                            <input id="Password" type="password" class="form-control" v-model="user.password" :class="errors && errors.Password ? 'is-invalid' : ''"/>
                            <div v-if="errors && errors.Password" class="invalid-feedback" role="alert"><template v-for="msg in errors.Password">{{ msg }}<br></template></div>
                        </div>
                        <div class="col-md-12">
                            <label class="form-label">Roles</label>
                            <div>
                                <div v-for="role in roles" class="form-check" :class="errors && errors.Roles ? 'is-invalid' : ''">
                                    <input class="form-check-input" :value="role" :checked="user.roles?.some(x => x === role)" v-model="user.roles" :id="`cb-role-${role}`" :class="errors && errors.Roles ? 'is-invalid' : ''" type="checkbox" />
                                    <label class="form-check-label" :for="`cb-role-${role}`">{{ role }}</label>
                                </div>
                                <div v-if="errors && errors.Roles" class="invalid-feedback" role="alert"><template v-for="msg in errors.Roles">{{ msg }}<br></template></div>
                            </div>
                        </div>
                        <button class="btn btn-primary" @click="save">{{ isInEditMode ? "Edit" : "Add" }} User</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
</template>