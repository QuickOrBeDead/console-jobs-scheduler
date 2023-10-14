<script setup lang="ts">
import { ref, watch } from 'vue'
import { createApi } from '../../api'
import { JobExecutionHistoryPagedResult, JobHistoryApi } from '../../metadata/console-jobs-scheduler-api'
import { useRoute } from 'vue-router';

const route = useRoute()

const jobHistoryItems = ref<JobExecutionHistoryPagedResult>()
const totalPages = ref<number>(0)
const jobHistoryApi = createApi(JobHistoryApi)

async function loadPage(page: number)  {
    const { data } = await jobHistoryApi.apiJobHistoryPageNumberGet(page, route.params.jobName as string)
    jobHistoryItems.value = data

    totalPages.value = data.totalPages!
}

watch(
  () => route.params, 
  () => loadPage(1),
  {
    deep:true
  }
)
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
                            <th scope="column">Run Time</th>
                            <th scope="column" style="text-align: center">Status</th>
                        </tr>
                        </thead>
                        <tbody>
                            <template v-for="item in jobHistoryItems?.items">
                            <tr :class="[item.hasSignalTimeout ? 'table-warning' : item.completed && !item.hasError && !item.vetoed ? 'table-success' :  item.completed && item.hasError && !item.vetoed ? 'table-danger' : item.vetoed ? 'table-warning' : '' ]">
                                <td class="text-nowrap"><router-link :to="{ name: 'JobExecutionDetails', params: { id: item.id }}">{{ item.jobName }}</router-link></td>
                                <td class="text-nowrap">{{ item.scheduledTime?.toLocaleDateTimeString() }}</td>
                                <td class="text-nowrap">{{ item.firedTime?.toLocaleDateTimeString() }}</td>
                                <td class="text-nowrap">{{ item.nextFireTime?.toLocaleDateTimeString() }}</td>
                                <td class="text-nowrap">{{ item.runTime }}</td>
                                <td style="text-align: center"><i :class="[item.hasSignalTimeout ? 'bi bi-question-circle-fill text-warning' : item.completed && !item.hasError && !item.vetoed ? 'bi bi-check-circle-fill text-success' : item.completed && item.hasError && !item.vetoed ? 'bi bi-x-circle-fill text-danger' : item.vetoed ? 'bi bi-stop-circle-fill text-warning' : 'bi bi-play-circle-fill text-primary' ]"></i></td>
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