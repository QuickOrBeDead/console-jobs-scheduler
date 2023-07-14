<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { createApi } from '../../api'
import { JobExecutionDetailModel, JobExecutionDetailsApi } from '../../metadata/console-jobs-scheduler-api'

const route = useRoute()
const id = route.params.id as string

const job = ref<JobExecutionDetailModel>()
const jobExecutionDetailsApi = createApi(JobExecutionDetailsApi)

onMounted(async () =>{
    const { data } = await jobExecutionDetailsApi.apiJobExecutionDetailsIdGet(id)
    job.value = data
})
</script>
<template>
<div class="page-container">
    <div class="container">
        <div class="row">
            <div class="col-12">
                <h1 class="display-6">Job Execution Details</h1>
            </div>
        </div>
        <div class="row justify-content-center">
            <h2 class="display-6 text-center">Job Console</h2>
            <div class="col-8">
                <p>
                    <div id="console">
                        <template v-if="job?.details?.completed" v-for="jobLog in job.logs">
                            <div class="col-12 text-start text-nowrap ps-1">{{ jobLog }}</div>
                        </template>
                    </div>
                </p>
            </div>
        </div>
        <div class="row justify-content-center">
            <div class="col-8">
                <table class="table table-striped table-bordered"> <tbody>
                    <tr>
                        <th scope="row">Job Name</th>
                        <td>{{ job?.details?.jobName }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Job Group</th>
                        <td>{{ job?.details?.jobGroup }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Instance Name</th>
                        <td>{{ job?.details?.instanceName }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Trigger Name</th>
                        <td>{{ job?.details?.triggerName }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Trigger Group</th>
                        <td>{{ job?.details?.triggerGroup }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Scheduled Time</th>
                        <td>{{ job?.details?.scheduledTime?.toLocaleDateTimeString() }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Run Time</th>
                        <td>{{ job?.details?.runTime }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Has Error</th>
                        <td>{{ job?.details?.hasError ? "TRUE" : "FALSE" }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Error Message</th>
                        <td>{{ job?.details?.errorMessage }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Vetoed</th>
                        <td>{{ job?.details?.vetoed ? "TRUE" : "FALSE" }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Completed</th>
                        <td>{{ job?.details?.completed ? "TRUE" : "FALSE" }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Attachments</th>
                        <td>
                            <template v-for="attachment in job?.attachments">
                                <a href="#">{{ attachment }}</a>
                                <br />
                            </template>
                        </td>
                    </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>
</template>
<style>
div#console {
    background: #000;
    border: 3px groove #ccc;
    color: #ccc;
    display: block;
    padding: 5px;
    height: 200px;
    overflow: scroll;
    counter-reset: line;
}

div#console div {
    counter-increment: line;
}

div#console div::before {
    content: counter(line);
    display: inline-block;
    width: 2.5em; /* Fixed width */
    border-right: 1px solid #ddd;
    padding: 0 .5em;
    margin-right: .5em;
    text-align: right;
    color: #888;
    -webkit-user-select: none;
}
</style>