<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { createApi } from '../../api'
import { JobExecutionHistoryPagedResult, JobHistoryApi } from '../../metadata/console-jobs-scheduler-api'

const jobHistoryItems = ref<JobExecutionHistoryPagedResult>()
const totalPages = ref<number>(0)
const jobHistoryApi = createApi(JobHistoryApi)

onMounted(async () => { 
   setCurrentPage(1)
})

async function setCurrentPage(page: number)  {
    const { data } = await jobHistoryApi.apiJobHistoryPageNumberGet(page)
    jobHistoryItems.value = data

    totalPages.value = data.totalPages!
}
</script>

<template>
    <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <h1 class="display-6">Job History</h1>
                </div>
            </div>
            <div class="row justify-content-center">
                <div class="col-12">
                    <table class="table table-striped table-bordered">
                        <thead>
                        <tr>
                            <th scope="column">Job Name</th>
                            <th scope="column">Scheduled Time</th>
                            <th scope="column">Fired Time</th>
                            <th scope="column">Next Fire Time</th>
                            <th scope="column">Completed</th>
                            <th scope="column">Run Time</th>
                            <th scope="column">Has Error</th>
                            <th scope="column">Vetoed</th>
                        </tr>
                        </thead>
                        <tbody>
                            <template v-for="item in jobHistoryItems?.items">
                            <tr :class="[item.completed && !item.hasError && !item.vetoed ? 'table-success': '' ]">
                                <td class="text-nowrap"><router-link :to="{ name: 'JobExecutionDetails', params: { id: item.id }}">{{ item.jobName }}</router-link></td>
                                <td class="text-nowrap">{{ item.scheduledTime?.toLocaleDateTimeString() }}</td>
                                <td class="text-nowrap">{{ item.firedTime?.toLocaleDateTimeString() }}</td>
                                <td class="text-nowrap">{{ item.nextFireTime?.toLocaleDateTimeString() }}</td>
                                <td>{{ item.completed ? "TRUE" : "FALSE" }}</td>
                                <td class="text-nowrap">{{ item.runTime }}</td>
                                <td>{{ item.hasError ? "TRUE" : "FALSE" }}</td>
                                <td>{{ item.vetoed ? "TRUE" : "FALSE" }}</td>
                            </tr>
                            </template>
                        </tbody>
                    </table>
                    <pagination :totalPages="totalPages" @pageChanged="setCurrentPage"></pagination>
                </div>
            </div>
        </div>
    </div>
</template>