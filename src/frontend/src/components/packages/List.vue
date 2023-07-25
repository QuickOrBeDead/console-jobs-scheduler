<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { createApi } from '../../api'
import { PackagesApi } from '../../metadata/console-jobs-scheduler-api'

const packagesApi = createApi(PackagesApi)
const packages = ref<Array<string>>()

onMounted(async () => {
    const { data } = await packagesApi.apiPackagesGet()
    packages.value = data
})
</script>
<template>
  <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <h1 class="display-6">Packages</h1>
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
                            <template v-for="item in packages">
                            <tr>
                                <td class="text-nowrap"><router-link :to="{ name: 'EditPackage', params: { name: item } }">{{ item }}</router-link></td>
                            </tr>
                            </template>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</template>