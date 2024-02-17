<script setup lang="ts">
import { ref } from 'vue'
import { createApi } from '../../api'
import { PackageListItemPagedResult, PackagesApi } from '../../metadata/console-jobs-scheduler-api'

const packagesApi = createApi(PackagesApi)
const packages = ref<PackageListItemPagedResult>()

async function loadPage(page: number)  {
    const { data } = await packagesApi.apiPackagesListPageNumberGet(page)
    packages.value = data
}
</script>
<template>
    <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-sm-12">
                    <div class="card flex-fill">
                        <div class="card-header">
                            <h4 class="card-title mb-0 text-muted"><small>Packages</small></h4>
                        </div>
                        <div class="card-body pb-0">
                            <div class="row">
                                <div class="col-12 mb-1">
                                    <button type="button" class="btn btn-outline-primary btn-sm float-start" @click="loadPage(1)"><i class="bi bi-arrow-repeat"></i></button>
                                    <router-link :to="{ name: 'EditPackage' }" v-slot="{ navigate }" custom>
                                        <button type="button" class="btn btn-outline-primary btn-sm rounded-pill float-end" @click="navigate"><i class="bi bi-plus-circle-fill"></i> New Package</button>
                                    </router-link>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-12">
                                    <div class="table-responsive">
                                        <table class="table table-sm mb-2">
                                            <thead class="table-light fw-semibold">
                                            <tr>
                                                <th class="text-muted" scope="column">Package Name</th>
                                                <th class="text-muted text-center" scope="column">Actions</th>
                                            </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="item in packages?.items">
                                                    <td class="text-nowrap" scope="row">{{ item.name }}</td>
                                                    <td class="text-nowrap text-center">
                                                        <router-link :to="{ name: 'EditPackage', params: { name: item.name } }" v-slot="{ navigate }" custom>
                                                            <button type="button" class="btn btn-success btn-sm rounded-pill" @click="navigate" title="Edit"><i class="bi bi-pencil-square"></i></button>
                                                        </router-link>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                
                                    <pagination :totalPages="packages?.totalPages" :totalCount="packages?.totalCount" :pageSize="packages?.pageSize" :page="packages?.page" @pageChanged="loadPage"></pagination>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>