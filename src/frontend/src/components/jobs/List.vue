<script setup lang="ts">
import { ref } from 'vue'
import { createApi } from '../../api'
import { JobListItemModelPagedResult, JobsApi } from '../../metadata/console-jobs-scheduler-api'

const jobs = ref<JobListItemModelPagedResult>()
const jobsApi = createApi(JobsApi)

async function loadPage(page: number)  {
    const { data } = await jobsApi.apiJobsPageNumberGet(page)
    jobs.value = data
}
</script>
<template>
    <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-sm-12">
                    <div class="card flex-fill">
                        <div class="card-header">
                            <h4 class="card-title mb-0 text-muted"><small>Jobs</small></h4>
                        </div>
                        <div class="card-body pb-0">
                            <div class="row">
                                <div class="col-12 mb-1">
                                    <button type="button" class="btn btn-outline-primary btn-sm float-start" @click="loadPage(1)"><i class="bi bi-arrow-repeat"></i></button>
                                    <router-link :to="{ name: 'EditJob' }" custom v-slot="{ navigate }">
                                        <button type="button" class="btn btn-outline-primary btn-sm rounded-pill float-end" @click="navigate"><i class="bi bi-plus-circle-fill"></i> New Job</button>
                                    </router-link>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-12">
                                    <div class="table-responsive">
                                        <table class="table table-sm mb-2">
                                            <thead class="table-light fw-semibold">
                                            <tr>
                                                <th class="text-muted" scope="column">Name</th>
                                                <th class="text-muted" scope="column">Group</th>
                                                <th class="text-muted" scope="column">Type</th>
                                                <th class="text-muted" scope="column">Trigger</th>
                                                <th class="text-muted" scope="column">Last Fire Time</th>
                                                <th class="text-muted" scope="column">Next Fire Time</th>
                                                <th class="text-muted text-center" scope="column">Actions</th>
                                            </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="job in jobs?.items">
                                                    <th class="text-nowrap" scope="row">{{ job.jobName }}</th>
                                                    <td class="text-nowrap">{{ job.jobGroup }}</td>
                                                    <td class="text-nowrap">{{ job.jobType }}</td>
                                                    <td class="text-nowrap">{{ job.triggerDescription }}</td>
                                                    <td class="text-nowrap">{{ job.lastFireTime?.toLocaleDateTimeString() }}</td>
                                                    <td class="text-nowrap">{{ job.nextFireTime?.toLocaleDateTimeString() }}</td>
                                                    <td class="text-nowrap text-center">
                                                        <router-link :to="{ name: 'EditJob', params: { jobName: job.jobName, jobGroup: job.jobGroup } }" custom v-slot="{ navigate }">
                                                            <button type="button" class="btn btn-success btn-sm rounded-pill" @click="navigate" title="Edit"><i class="bi bi-pencil-square"></i></button>
                                                        </router-link>
                                                        <router-link :to="{ name: 'JobHistory', params: { jobName: job.jobName }}" custom v-slot="{ navigate }">
                                                            <button type="button" class="btn btn-secondary btn-sm rounded-pill" @click="navigate" title="History"><i class="bi bi-search"></i></button>
                                                        </router-link>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                
                                    <pagination :totalPages="jobs?.totalPages" :totalCount="jobs?.totalCount" :pageSize="jobs?.pageSize" :page="jobs?.page" @pageChanged="loadPage"></pagination>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>