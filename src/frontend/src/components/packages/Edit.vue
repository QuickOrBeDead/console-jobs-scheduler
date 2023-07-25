<script setup lang="ts">
import { ref, onMounted, Ref } from 'vue'
import { useRoute } from 'vue-router'
import { createApi } from '../../api'
import { PackageDetailsModel, PackagesApi } from '../../metadata/console-jobs-scheduler-api'

const packageDetail = ref<PackageDetailsModel>() as Ref<PackageDetailsModel>
const route = useRoute()
const name = route.params.name as string
const isInEditMode = !!name
const packagesApi = createApi(PackagesApi)

onMounted(async () => {
    const { data } = await packagesApi.apiPackagesDetailGet(name)
    packageDetail.value = data
})
</script>
<template>
     <div v-if="packageDetail" class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <h1 class="display-6">{{ isInEditMode ? "Edit" : "Add" }} Package</h1>
                    <hr style="margin: 4px 0px;">
                </div>
            </div>
            <div class="row justify-content-center">
                <div class="col-6">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label for="Name" class="form-label">Name</label>
                            <label id="Name" class="form-control-plaintext"><b>{{ packageDetail.name }}</b></label>
                        </div>
                        <div class="col-md-6">
                            <label for="ModifyDate" class="form-label">Last Modify Date</label>
                            <label id="ModifyDate" class="form-control-plaintext"><b>{{ packageDetail.modifyDate?.toLocaleDateTimeString() }}</b></label>
                        </div>
                        <div class="col-12">
                            <label for="Path" class="form-label">Path</label>
                            <label id="Path" class="form-control-plaintext"><b>{{ packageDetail.path }}</b></label>
                        </div>
                        <button class="btn btn-primary">{{ isInEditMode ? "Edit" : "Add" }} Package</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>