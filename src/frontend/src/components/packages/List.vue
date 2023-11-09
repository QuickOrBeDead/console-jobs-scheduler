<script setup lang="ts">
import { ref } from 'vue'
import { createApi } from '../../api'
import { PackageListItemModelPagedResult, PackagesApi } from '../../metadata/console-jobs-scheduler-api'

const packagesApi = createApi(PackagesApi)
const packages = ref<PackageListItemModelPagedResult>()

async function loadPage(page: number)  {
    const { data } = await packagesApi.apiPackagesPageNumberGet(page)
    packages.value = data
}
</script>
<template>
  <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <h1 class="display-6">Packages</h1>
                </div>
            </div>
            <div class="row">
                <div class="col-12 mb-1">
                    <router-link :to="{ name: 'EditPackage' }" custom v-slot="{ navigate }">
                        <button class="btn btn-primary float-end" @click="navigate">Add</button>
                    </router-link>
                </div>
            </div>
            <div class="row justify-content-center">
                <div class="col-12">
                    <table class="table table-striped table-bordered">
                        <thead>
                        <tr>
                            <th scope="column">Package Name</th>
                        </tr>
                        </thead>
                        <tbody>
                            <tr v-for="item in packages?.items">
                                <td class="text-nowrap"><router-link :to="{ name: 'EditPackage', params: { name: item.name } }">{{ item.name }}</router-link></td>
                            </tr>
                        </tbody>
                    </table>

                    <pagination :totalPages="packages?.totalPages" :totalCount="packages?.totalCount" :pageSize="packages?.pageSize" :page="packages?.page" @pageChanged="loadPage"></pagination>
                </div>
            </div>
        </div>
    </div>
</template>