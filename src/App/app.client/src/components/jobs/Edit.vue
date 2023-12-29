<script setup lang="ts">
import { ref, onMounted, Ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { createApi } from '../../api'
import { JobDetailModel, JobsApi, PackagesApi } from '../../metadata/console-jobs-scheduler-api'

const route = useRoute()
const router = useRouter()

const job = ref<JobDetailModel>() as Ref<JobDetailModel>
const packages = ref<string[]>() as Ref<string[]>

let jobGroup: string
let jobName: string
let isInEditMode: boolean

const jobsApi = createApi(JobsApi)
const packagesApi = createApi(PackagesApi)

const loadPage = async () => {
    jobGroup = route.params.jobGroup as string
    jobName = route.params.jobName as string
    isInEditMode = !!jobGroup && !!jobName

    const { data } = await packagesApi.apiPackagesGetPackageNamesGet()
    packages.value = data
   
    if (isInEditMode) {
        const { data } = await jobsApi.apiJobsGroupNameGet(jobGroup, jobName)
        job.value = data
    }
    else {
        job.value = {}
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
    if (job.value) {
        await jobsApi.apiJobsPost({
            jobName: job.value.jobName as string,
            jobGroup: job.value.jobGroup as string,
            description: job.value.description as string,
            cronExpression: job.value.cronExpression as string,
            package: job.value.package as string,
            parameters: job.value.parameters
        })

        if (isInEditMode) {
            await loadPage()
        } else {
            await router.push({ name: 'EditJob', params: { jobName: job.value.jobName, jobGroup: job.value.jobGroup }})
        }
    }
}
</script>

<template>
    <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <div class="card flex-fill">
                        <div class="card-header d-flex justify-content-between align-items-center">
                            <h4 class="card-title mb-0 text-muted"><small>{{ isInEditMode ? "Edit" : "Add" }} Job</small></h4>
                            <router-link to="/jobs" v-slot="{ navigate }" custom>
                                <button type="button" @click="navigate" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-backspace"></i> Back</button>
                            </router-link>
                        </div>
                        <div class="card-body">
                            <div v-if="job" class="col-12">
                                <div class="row g-3 mb-3">
                                    <div class="col-md-6">
                                        <label for="JobName" class="form-label">Job Name</label>
                                        <input id="JobName" type="text" class="form-control" v-model="job.jobName"/>
                                    </div>
                                    <div class="col-md-6">
                                        <label for="JobGroup" class="form-label">Job Group</label>
                                        <input id="JobGroup" type="text" class="form-control" v-model="job.jobGroup"/>
                                    </div>
                                    <div class="col-12">
                                        <label for="Description" class="form-label">Description</label>
                                        <input id="Description" type="text" class="form-control" v-model="job.description"/>
                                    </div>
                                    <div class="col-md-6">
                                        <label for="CronExpression" class="form-label">Cron Expression</label>
                                        <input id="CronExpression" type="text" class="form-control" v-model="job.cronExpression" aria-describedby="helpCron"/>
                                        <div id="helpCron" class="form-text text-black-50">{{ job.cronExpressionDescription }}</div>
                                    </div>
                                    <div class="col-md-6">
                                        <label for="Package" class="form-label">Package</label>
                                        <select id="Package" class="form-select" v-model="job.package">
                                            <option v-for="option in packages" :key="option" :value="option">
                                                {{ option }}
                                            </option>
                                        </select>
                                    </div>
                                    <div class="col-12">
                                        <label for="Parameters" class="form-label">Parameters</label>
                                        <textarea id="Parameters" class="form-control" v-model="(job.parameters as string)" rows="5" cols="30"></textarea>
                                    </div>
                                </div>
                                <button class="btn btn-primary" @click="save">{{ isInEditMode ? "Edit" : "Add" }} Job</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>   
        </div>
    </div>
</template>