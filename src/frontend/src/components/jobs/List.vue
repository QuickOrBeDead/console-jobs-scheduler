<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { createApi } from '../../api'
import { JobListItemModelPagedResult, JobsApi } from '../../metadata/console-jobs-scheduler-api'

const jobs = ref<JobListItemModelPagedResult>()
const jobsApi = createApi(JobsApi)
const totalPages = ref<number>(0)

onMounted(async () => {
    await loadPage(1)
})

async function loadPage(page: number)  {
    const { data } = await jobsApi.apiJobsPageNumberGet(page)
    jobs.value = data

    totalPages.value = data.totalPages!
}
</script>
<template>
    <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <h1 class="display-6">Job List</h1>
                </div>
            </div>
            <div class="row">
                <div class="col-12 mb-1">
                    <router-link :to="{ name: 'EditJob' }" custom v-slot="{ navigate }">
                        <button class="btn btn-primary float-end" @click="navigate">Add</button>
                    </router-link>
                </div>
            </div>
            <div class="row" v-if="jobs">
                <div class="col-12">
                    <table class="table table-striped table-bordered">
                        <thead>
                        <tr>
                            <th scope="column">Name</th>
                            <th scope="column">Group</th>
                            <th scope="column">Type</th>
                            <th scope="column">Trigger</th>
                            <th scope="column">Last Fire Time</th>
                            <th scope="column">Next Fire Time</th>
                            <th scope="column"></th>
                        </tr>
                        </thead>
                        <tbody>
                            <template v-for="job in jobs.items">
                                <tr>
                                    <th class="text-nowrap" scope="row"><a>{{ job.jobName }}</a></th>
                                    <td class="text-nowrap">{{ job.jobGroup }}</td>
                                    <td class="text-nowrap">{{ job.jobType }}</td>
                                    <td class="text-nowrap">{{ job.triggerDescription }}</td>
                                    <td class="text-nowrap">{{ job.lastFireTime?.toLocaleDateTimeString() }}</td>
                                    <td class="text-nowrap">{{ job.nextFireTime?.toLocaleDateTimeString() }}</td>
                                    <td class="text-nowrap">
                                        <router-link :to="{ name: 'EditJob', params: { jobName: job.jobName, jobGroup: job.jobGroup } }">Edit</router-link>
                                        &nbsp;
                                        <router-link :to="{ name: 'JobHistory', params: { jobName: job.jobName }}">History</router-link>
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