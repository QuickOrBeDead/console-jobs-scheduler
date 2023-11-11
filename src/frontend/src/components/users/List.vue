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
                <div class="col-sm-12">
                    <div class="card flex-fill">
                        <div class="card-header">
                            <h4 class="card-title mb-0 text-muted"><small>Users</small></h4>
                        </div>
                        <div class="card-body pb-0">
                            <div class="row">
                                <div class="col-12 mb-1">
                                    <button type="button" class="btn btn-outline-primary btn-sm float-start" @click="loadPage(1)"><i class="bi bi-arrow-repeat"></i></button>
                                    <router-link :to="{ name: 'EditUser' }" custom v-slot="{ navigate }">
                                        <button type="button" class="btn btn-outline-primary btn-sm rounded-pill float-end" @click="navigate"><i class="bi bi-plus-circle-fill"></i> New User</button>
                                    </router-link>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-12">
                                    <div class="table-responsive">
                                        <table class="table table-sm mb-2">
                                            <thead class="table-light fw-semibold">
                                            <tr>
                                                <th class="text-muted" scope="column">Username</th>
                                                <th class="text-muted" scope="column">Roles</th>
                                                <th class="text-muted text-center" scope="column">Actions</th>
                                            </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="item in userItems?.items">
                                                    <td class="text-nowrap" scope="row">{{ item.userName }}</td>
                                                    <td class="text-nowrap">{{ item.roles }}</td>
                                                    <td class="text-nowrap text-center">
                                                        <router-link :to="{ name: 'EditUser', params: { userId: item.id } }" custom v-slot="{ navigate }">
                                                            <button type="button" class="btn btn-success btn-sm rounded-pill" @click="navigate" title="Edit"><i class="bi bi-pencil-square"></i></button>
                                                        </router-link>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>

                                    <pagination :totalPages="userItems?.totalPages" :totalCount="userItems?.totalCount" :pageSize="userItems?.pageSize" :page="userItems?.page" @pageChanged="loadPage"></pagination>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>