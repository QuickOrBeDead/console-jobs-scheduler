<script setup lang="ts">
import { ref, onMounted, Ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { createApi } from '../../api'
import { PackageDetails, PackagesApi } from '../../metadata/console-jobs-scheduler-api'

const route = useRoute()
const router = useRouter()

let name: string
let isInEditMode: boolean

const packageDetail = ref<PackageDetails>() as Ref<PackageDetails>
const fileInput = ref<HTMLInputElement>()
const file = ref<File | null>()

const packagesApi = createApi(PackagesApi)

const loadPage = async () => {
    name = route.params.name as string
    isInEditMode = !!name

    file.value = null
    if (fileInput.value) {
        fileInput.value.value = ''
    }

    if (isInEditMode) {
        const { data } = await packagesApi.apiPackagesDetailGet(name)
        packageDetail.value = data
    } else {
        packageDetail.value = {}
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

function onFileChanged() {
    if (fileInput.value && fileInput.value.files) {
        file.value = fileInput.value.files[0];
    }
}

async function save() {
    const packageName = isInEditMode ? name : packageDetail.value.name
    if (file.value && packageName) {
        await packagesApi.apiPackagesSavePost(packageName, file.value)

        if (isInEditMode) {
            await loadPage()
        } else {
            await router.push({ name: 'EditPackage', params: { name: packageName }})
        }
    }
}
</script>
<template>
     <div v-if="packageDetail" class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <div class="card flex-fill">
                        <div class="card-header d-flex justify-content-between align-items-center">
                            <h4 class="card-title mb-0 text-muted"><small>{{ isInEditMode ? "Edit" : "Add" }} Package</small></h4>
                            <router-link to="/packages" v-slot="{ navigate }" custom>
                                <button type="button" @click="navigate" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-backspace"></i> Back</button>
                            </router-link>
                        </div>
                        <div class="card-body">
                            <div class="col-12">
                                <div class="row g-3 mb-3">
                                    <div :class="[isInEditMode ? 'col-md-6' : 'col-md-12']">
                                        <label for="Name" class="form-label">Name</label>
                                        <label v-if="isInEditMode" id="Name" class="form-control-plaintext"><b>{{ packageDetail.name }}</b></label>
                                        <input v-if="!isInEditMode" id="Name" type="text" class="form-control" v-model="packageDetail.name">
                                    </div>
                                    <div v-if="isInEditMode" class="col-md-6">
                                        <label for="ModifyDate" class="form-label">Last Modify Date</label>
                                        <label id="ModifyDate" class="form-control-plaintext"><b>{{ packageDetail.modifyDate?.toLocaleDateTimeString() }}</b></label>
                                    </div>
                                    <div class="col-12">
                                        <label for="File" class="form-label">Package Zip File</label>
                                        <input id="File" class="form-control" type="file" @change="onFileChanged" ref="fileInput" accept=".zip">
                                    </div>
                                </div>
                                <button class="btn btn-primary" @click="save">{{ isInEditMode ? "Edit" : "Add" }} Package</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>