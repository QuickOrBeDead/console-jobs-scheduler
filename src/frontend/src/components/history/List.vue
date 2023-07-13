<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { createApi } from '../../api'
import { JobExecutionHistoryPagedResult, JobHistoryApi } from '../../metadata/console-jobs-scheduler-api'

const jobHistoryItems = ref<JobExecutionHistoryPagedResult>()

let totalPages = 0
let currentPage: number = 1
let nextPage = 0
let previousPage = 0
let start = 0
let end = 0
const pages =  ref<number[]>()
const jobHistoryApi = createApi(JobHistoryApi)

onMounted(async () => { 
   setCurrentPage(1)
})

async function setCurrentPage(page: number)  {
    currentPage = page

    const { data } = await jobHistoryApi.apiJobHistoryPageNumberGet(currentPage)
    jobHistoryItems.value = data

    totalPages = data.totalPages!
    start = Math.max(1, currentPage - 5)
    end = Math.min(start + 10, totalPages)
    if (end - start < 10)
    {
        start = Math.max(1, end - 10)
    }

    nextPage = Math.min(currentPage + 1, totalPages)
    previousPage = Math.max(currentPage - 1, start)

    const pageList = []
    for (let i = start; i <= end; i++) {
        pageList.push(i) 
    }

    pages.value = pageList
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
                                <td class="text-nowrap"><a asp-page="Detail" asp-route-id="@item.Id">{{ item.jobName }}</a></td>
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
                    <ul class="pagination justify-content-center">
                        <li :class="[currentPage == start ? 'disabled': '']" class="page-item">
                            <a class="page-link" href="#" @click="setCurrentPage(previousPage)" aria-label="Previous">
                                <span aria-hidden="true">«</span>
                            </a>
                        </li>
                        <template v-for="pageNumber in pages">
                            <li :class="[pageNumber == currentPage ? 'disabled': '']" class="page-item">
                                <a class="page-link" href="#" @click="setCurrentPage(pageNumber)">{{ pageNumber }}</a>
                            </li>
                        </template>
                        <li :class="[currentPage == end ? 'disabled': '']" class="page-item">
                            <a class="page-link" href="#" @click="setCurrentPage(nextPage)" aria-label="Next">
                                <span aria-hidden="true">»</span>
                            </a>
                        </li>
                    </ul>
                </div>
            </div>
        </div>
    </div>
</template>