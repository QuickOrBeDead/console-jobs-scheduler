<script setup lang="ts">
import { ref, reactive, onMounted, onUpdated } from 'vue'
import { useRoute } from 'vue-router'
import { createApi } from '../../api'
import { JobExecutionDetail, JobExecutionDetailsApi, LogLine } from '../../metadata/console-jobs-scheduler-api'
import { HubConnectionBuilder } from '@aspnet/signalr'

const route = useRoute()
const id = route.params.id as string

const job = ref<JobExecutionDetail>()
const logs = reactive<LogLine[]>([])
const attachments = ref<string[]>()
const jobExecutionDetailsApi = createApi(JobExecutionDetailsApi)

onMounted(async () => {
    const { data } = await jobExecutionDetailsApi.apiJobExecutionDetailsIdGet(id)
    job.value = data.details
    logs.push(...data.logs as LogLine[])
    attachments.value = data.attachments as string[]

    const $console = document.getElementById('console')

    var connection = new HubConnectionBuilder().withUrl('/jobRunConsoleHub').build()

    connection.on('ReceiveJobConsoleLogMessage', function (data, isError) {
        logs.push({
            message: data,
            isError: isError
        })

        $console!.scrollTop = $console!.scrollHeight
    })

    connection.start().then(function () {
        connection.invoke('AddToGroup', id)
    })
})

onUpdated(() => {
    const $console = document.getElementById('console')
    $console!.scrollTop = $console!.scrollHeight
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
                        <template v-for="jobLog in logs">
                            <div :class="[jobLog.isError ? 'text-danger' : '']" class="col-12 text-start text-nowrap ps-1">{{ jobLog.message }}</div>
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
                        <td>{{ job?.jobName }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Job Group</th>
                        <td>{{ job?.jobGroup }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Instance Name</th>
                        <td>{{ job?.instanceName }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Trigger Name</th>
                        <td>{{ job?.triggerName }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Trigger Group</th>
                        <td>{{ job?.triggerGroup }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Scheduled Time</th>
                        <td>{{ job?.scheduledTime?.toLocaleDateTimeString() }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Run Time</th>
                        <td>{{ job?.runTime }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Has Error</th>
                        <td>{{ job?.hasError ? "TRUE" : "FALSE" }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Error Message</th>
                        <td>{{ job?.errorMessage }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Vetoed</th>
                        <td>{{ job?.vetoed ? "TRUE" : "FALSE" }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Completed</th>
                        <td>{{ job?.completed ? "TRUE" : "FALSE" }}</td>
                    </tr>
                    <tr>
                        <th scope="row">Attachments</th>
                        <td>
                            <template v-for="attachment in attachments">
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