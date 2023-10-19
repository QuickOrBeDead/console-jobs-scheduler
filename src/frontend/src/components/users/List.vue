<script setup lang="ts">
import { ref } from 'vue'
import { createApi } from '../../api'
import { UserListItemModelPagedResult, UsersApi } from '../../metadata/console-jobs-scheduler-api'

const userItems = ref<UserListItemModelPagedResult>()
const totalPages = ref<number>(0)
const usersApi = createApi(UsersApi)

async function loadPage(page: number)  {
    const { data } = await usersApi.apiUsersPageNumberGet(page)
    userItems.value = data

    totalPages.value = data.totalPages!
}
</script>

<template>
    <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <h1 class="display-6">Users</h1>
                </div>
            </div>
            <div class="row">
                <div class="col-12 mb-1">
                    <router-link :to="{ name: 'EditUser' }" custom v-slot="{ navigate }">
                        <button class="btn btn-primary float-end" @click="navigate">Add</button>
                    </router-link>
                </div>
            </div>
            <div class="row justify-content-center">
                <div class="col-12">
                    <table class="table table-striped table-bordered">
                        <thead>
                        <tr>
                            <th scope="column">Username</th>
                            <th scope="column">Roles</th>
                            <th scope="column"></th>
                        </tr>
                        </thead>
                        <tbody>
                            <template v-for="item in userItems?.items">
                            <tr>
                                <td class="text-nowrap">{{ item.userName }}</td>
                                <td class="text-nowrap">{{ item.roles }}</td>
                                <td class="text-nowrap" style="text-align: center">
                                    <router-link :to="{ name: 'EditUser', params: { userId: item.id } }">Edit</router-link>
                                </td>
                            </tr>
                            </template>
                        </tbody>
                    </table>
                    <pagination :totalPages="totalPages" @pageChanged="loadPage"></pagination>
                </div>
            </div>
        </div>
    </div>
</template>