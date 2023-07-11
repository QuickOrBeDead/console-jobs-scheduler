<script setup lang="ts">
import { ref, onMounted, Ref } from 'vue'
import { useRoute } from "vue-router"
import { createApi } from '../../api'
import { JobDetailModel, JobsApi, PackagesApi } from '../../metadata/console-jobs-scheduler-api'

const job = ref<JobDetailModel>() as Ref<JobDetailModel>
const packages = ref<string[]>() as Ref<string[]>
const route = useRoute()
const jobGroup = route.params.jobGroup as string
const jobName = route.params.jobName as string
const isInEditMode = jobGroup && jobName

onMounted(async () => {
    const packagesApi = createApi(PackagesApi)
    const jobsApi = createApi(JobsApi)

    const { data } = await packagesApi.apiPackagesGet()
    packages.value = data
   
    if (isInEditMode)
    {
        const { data } = await jobsApi.apiJobsGroupNameGet(jobGroup, jobName)
        job.value = data
    }
})
</script>

<template>

<template v-if="job">
    <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <h1 class="display-6">{{ isInEditMode ? "Edit" : "Add" }} Job</h1>
                    <hr style="margin: 4px 0px;">
                </div>
            </div>
            <div class="row justify-content-center">
                <div class="col-6">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label for="JobName" class="form-label">Job Name</label>
                            <input id="JobName" class="form-control" v-model="job.jobName"/>
                        </div>
                        <div class="col-md-6">
                            <label for="JobGroup" class="form-label">Job Group</label>
                            <input id="JobGroup" class="form-control" v-model="job.jobGroup"/>
                        </div>
                        <div class="col-12">
                            <label for="Description" class="form-label">Description</label>
                            <input id="Description" class="form-control" v-model="job.description"/>
                        </div>
                        <div class="col-md-6">
                            <label for="CronExpression" class="form-label">Cron Expression</label>
                            <input id="CronExpression" class="form-control" v-model="job.cronExpression" aria-describedby="helpCron"/>
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
                            <textarea id="Parameters" class="form-control" v-model="job.parameters" rows="5" cols="30"></textarea>
                        </div>
                        <button class="btn btn-primary">{{ isInEditMode ? "Edit" : "Add" }} Job</button>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
</template>