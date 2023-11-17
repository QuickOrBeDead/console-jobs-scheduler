<script setup lang="ts">
import { ref, watch } from 'vue'
import { createApi } from '../../api'
import { JobExecutionHistoryPagedResult, JobHistoryApi } from '../../metadata/console-jobs-scheduler-api'
import { useRoute } from 'vue-router';

const route = useRoute()

const jobHistoryItems = ref<JobExecutionHistoryPagedResult>()
const jobHistoryApi = createApi(JobHistoryApi)

async function loadPage(page: number)  {
    const { data } = await jobHistoryApi.apiJobHistoryPageNumberGet(page, route.params.jobName as string)
    jobHistoryItems.value = data
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
                <div class="col-sm-12">
                    <div class="card flex-fill">
                        <div class="card-header">
                            <h4 class="card-title mb-0 text-muted"><small>Job History</small></h4>
                        </div>
                        <div class="card-body pb-0">
                            <div class="row">
                                <div class="col-12 mb-1">
                                    <button type="button" class="btn btn-outline-primary btn-sm float-start" @click="loadPage(1)"><i class="bi bi-arrow-repeat"></i></button>
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-12">
                                    <div class="table-responsive">
                                        <table class="table table-sm mb-2">
                                            <thead class="table-light fw-semibold">
                                            <tr>
                                                <th class="text-muted" scope="column">Job Name</th>
                                                <th class="text-muted" scope="column">Scheduled Time</th>
                                                <th class="text-muted" scope="column">Fired Time</th>
                                                <th class="text-muted" scope="column">Next Fire Time</th>
                                                <th class="text-muted" scope="column">Run Time</th>
                                                <th class="text-muted text-center" scope="column">Status</th>
                                                <th class="text-muted text-center" scope="column">Actions</th>
                                            </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="item in jobHistoryItems?.items" :class="[item.hasSignalTimeout ? 'table-warning' : item.completed && !item.hasError && !item.vetoed ? 'table-success' :  item.completed && item.hasError && !item.vetoed ? 'table-danger' : item.vetoed ? 'table-warning' : '' ]">
                                                    <td class="text-nowrap" scope="row">{{ item.jobName }}</td>
                                                    <td class="text-nowrap">{{ item.scheduledTime?.toLocaleDateTimeString() }}</td>
                                                    <td class="text-nowrap">{{ item.firedTime?.toLocaleDateTimeString() }}</td>
                                                    <td class="text-nowrap">{{ item.nextFireTime?.toLocaleDateTimeString() }}</td>
                                                    <td class="text-nowrap">{{ item.runTime }}</td>
                                                    <td class="text-center"><i :class="[item.hasSignalTimeout ? 'bi bi-question-circle-fill text-warning' : item.completed && !item.hasError && !item.vetoed ? 'bi bi-check-circle-fill text-success' : item.completed && item.hasError && !item.vetoed ? 'bi bi-x-circle-fill text-danger' : item.vetoed ? 'bi bi-stop-circle-fill text-warning' : 'bi bi-play-circle-fill text-primary' ]"></i></td>
                                                    <td class="text-center">
                                                        <router-link :to="{ name: 'JobExecutionDetails', params: { id: item.id }}" v-slot="{ navigate }" custom><button type="button" class="btn btn-secondary btn-sm rounded-pill" @click="navigate" title="Details"><i class="bi bi-search"></i></button></router-link>
                                                    </td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                    <pagination :totalPages="jobHistoryItems?.totalPages" :totalCount="jobHistoryItems?.totalCount" :pageSize="jobHistoryItems?.pageSize" :page="jobHistoryItems?.page" @pageChanged="loadPage"></pagination>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>