<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { createApi } from '../../api'
import { JobListItemModel, JobsApi } from '../../metadata/console-jobs-scheduler-api'

const jobs = ref<JobListItemModel[]>()

onMounted(async () => {
    const jobsApi = createApi(JobsApi)
    const { data } = await jobsApi.apiJobsGet()
    jobs.value = data
})
</script>
<template>
    <div class="page-container">
        <div class="container">
            <div class="row">
                <div class="col-12">
                    <h1 class="display-6">Job List</h1>
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
                            <template v-for="job in jobs">
                                <tr>
                                    <th class="text-nowrap" scope="row"><a>{{ job.jobName }}</a></th>
                                    <td class="text-nowrap">{{ job.jobGroup }}</td>
                                    <td class="text-nowrap">{{ job.jobType }}</td>
                                    <td class="text-nowrap">{{ job.triggerDescription }}</td>
                                    <td class="text-nowrap">{{ job.lastFireTime?.toLocaleDateTimeString() }}</td>
                                    <td class="text-nowrap">{{ job.nextFireTime?.toLocaleDateTimeString() }}</td>
                                    <td class="text-nowrap"><a>Edit</a></td>
                                </tr>
                            </template>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</template>