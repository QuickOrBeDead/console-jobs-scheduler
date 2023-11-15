<script setup lang="ts">
import { ref, reactive, onMounted, onUpdated } from 'vue'
import { useRoute } from 'vue-router'
import { createApi } from '../../api'
import { AttachmentInfoModel, JobExecutionDetail, JobExecutionDetailsApi, LogLine } from '../../metadata/console-jobs-scheduler-api'
import { HubConnectionBuilder } from '@aspnet/signalr'

const route = useRoute()
const id = route.params.id as string

const job = ref<JobExecutionDetail>()
const logs = reactive<LogLine[]>([])
const attachments = ref<AttachmentInfoModel[]>()
const jobExecutionDetailsApi = createApi(JobExecutionDetailsApi)

onMounted(async () => {
    const { data } = await jobExecutionDetailsApi.apiJobExecutionDetailsIdGet(id)
    job.value = data.details
    logs.push(...data.logs as LogLine[])
    attachments.value = data.attachments as AttachmentInfoModel[]

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

function getAttachmentUrl(attachment: AttachmentInfoModel): string {
    const basePath = jobExecutionDetailsApi["basePath"]
    return `${basePath}/api/JobExecutionDetails/GetAttachment/${attachment.id}?attachmentName=${encodeURIComponent(attachment.fileName as string)}`
}
</script>
<template>
<div class="page-container">
    <div class="container">
        <div class="row">
            <div class="col-12">
                <div class="card flex-fill">
					<div class="card-header d-flex justify-content-between align-items-center">
						<h4 class="card-title mb-0 text-muted"><small>Job Execution Details</small></h4>
                        <router-link class="nav-link" to="/history" custom v-slot="{ navigate }">
                            <button type="button" @click="navigate" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-backspace"></i> Back</button>
                        </router-link>
					</div>
					<div class="card-body">
                        <div class="row justify-content-center">
                            <div class="col-12">
                                <div class="fs-5">
                                    <i :class="[job?.hasSignalTimeout ? 'bi bi-question-circle-fill text-warning' : job?.completed && !job?.hasError && !job?.vetoed ? 'bi bi-check-circle-fill text-success' : job?.completed && job?.hasError && !job?.vetoed ? 'bi bi-x-circle-fill text-danger' : job?.vetoed ? 'bi bi-stop-circle-fill text-warning' : 'bi bi-play-circle-fill text-primary' ]"></i>
                                    <span class="text-muted">#{{ job?.id }}:</span>
                                    <span class="fw-bold">{{ job?.jobName }}</span>
                                </div>
                            </div>
                            <div class="col-12 d-flex justify-content-between align-items-center">
                                <div class="fs-6">
                                    <span class="fw-lighter">Scheduled at {{ job?.scheduledTime?.toLocaleDateTimeString() }}</span>
                                    <span class="fw-lighter me-1" title="Node"><i class="bi bi-diagram-2"></i>{{ job?.instanceName }}</span>
                                    <span class="fw-lighter" title="Job Group"><i class="bi bi-collection"></i> {{ job?.jobGroup }}</span>
                                </div>
                            </div>
                            <div class="col-12 d-flex justify-content-between align-items-center mt-3">
                                <div class="fs-6">
                                    <span class="fw-normal">Trigger Name:</span>
                                    <span class="fw-light">{{ job?.triggerName }}</span>
                                </div>
                                <div class="fs-6">
                                    <span class="fw-normal">Started:</span>
                                    <span class="fw-light">{{ job?.firedTime?.toLocaleDateTimeString() }}</span>
                                </div>
                            </div>
                            <div class="col-12 d-flex justify-content-between align-items-center">
                                <div class="fs-6">
                                    <span class="fw-normal">Trigger Group:</span>
                                    <span class="fw-light">{{ job?.triggerGroup }}</span>
                                </div>
                                <div v-if="job?.runTime" class="fs-6">
                                    <span class="fw-light" title="Run Time"><i class="bi bi-stopwatch"></i> {{ job?.runTime }}</span>
                                </div>
                            </div>
                            <div class="col-12 mt-2">
                                <strong><i class="bi bi-activity"></i> Logs</strong>
                                <p>
                                    <div id="console">
                                        <template v-for="jobLog in logs">
                                            <div :class="[jobLog.isError ? 'text-danger' : '']" class="col-12 text-start text-nowrap ps-1">{{ jobLog.message }}</div>
                                        </template>
                                    </div>
                                </p>
                            </div>
                        </div>
                        <div class="col-12">
                            <div class="d-flex align-items-start">
                                <div class="flex-grow-1">
                                    <strong><i class="bi bi-paperclip"></i> Attachments <span class="badge bg-secondary">{{ attachments?.length }}</span></strong><br>
                                    <ul class="list-group list-group-flush">
                                        <li class="list-group-item" v-for="attachment in attachments">
                                            <a class="text-reset" :href="getAttachmentUrl(attachment)" title="Click to Download"><i class="bi bi-file-earmark-arrow-down"></i>{{ attachment.fileName }}</a>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                            <template v-if="job?.errorMessage">
                                <hr>
                                <div class="d-flex align-items-start">
                                    <div class="flex-grow-1">
                                        <strong><i class="bi bi-bug"></i> Error Message</strong><br>
                                        <p>{{ job?.errorMessage }}</p>
                                    </div>
                                </div>
                            </template>
                            
                        </div>
					</div>
				</div>
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