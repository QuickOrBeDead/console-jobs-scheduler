<script setup lang="ts">
import { ref, reactive, onMounted, onUpdated } from 'vue'
import { useRoute } from 'vue-router'
import { createApi } from '../../api'
import {
  JobRunAttachmentInfo,
  JobExecutionHistoryDetail,
  JobExecutionDetailsApi,
  JobRunLogDetail,
  JobHistoryApi
} from '../../metadata/console-jobs-scheduler-api'
import { HubConnectionBuilder } from '@microsoft/signalr'

const route = useRoute()
const id = route.params.id as string

const job = ref<JobExecutionHistoryDetail>()
const logs = reactive<JobRunLogDetail[]>([])
const attachments = ref<JobRunAttachmentInfo[]>()
const jobExecutionDetailsApi = createApi(JobExecutionDetailsApi)
const jobHistoryApi = createApi(JobHistoryApi)

onMounted(async () => {
  await setJobExecutionHistoryDetail();
  await setJobExecutionDetail();

  const $console = document.getElementById('console')

    const connection = new HubConnectionBuilder().withUrl('/jobRunConsoleHub').build()

    connection.on('ReceiveJobConsoleLogMessage', function (data, isError) {
        logs.push({
            content: data,
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

async function setJobExecutionHistoryDetail() {
  const {data} = await jobHistoryApi.apiJobHistoryGetJobExecutionDetailIdGet(id)
  job.value = data;
}

async function setJobExecutionDetail() {
  const {data} = await jobExecutionDetailsApi.apiJobExecutionDetailsIdGet(id)
  logs.push(...data.logs as JobRunLogDetail[])
  attachments.value = data.attachments as JobRunAttachmentInfo[]
}

function getAttachmentUrl(attachment: JobRunAttachmentInfo): string {
    const basePath = jobExecutionDetailsApi["basePath"]
    return `${basePath}/api/JobExecutionDetails/GetAttachment/${attachment.id}?attachmentName=${encodeURIComponent(attachment.fileName as string)}`
}
</script>
<template>
<div class="page-container">
    <div class="container">
        <div class="row">
            <div class="col-12">
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
                        <span class="fw-lighter me-1" title="Node"><i class="bi bi-diagram-2 text-primary"></i>{{ job?.instanceName }}</span>
                        <span class="fw-lighter me-1" title="Job Group"><i class="bi bi-collection text-warning"></i> {{ job?.jobGroup }}</span>
                        <span class="fw-lighter" title="Trigger Interval"><i class="bi bi-alarm text-secondary"></i> {{ job?.cronExpressionDescription }}</span>
                    </div>
                    <router-link to="/history" v-slot="{ navigate }" custom>
                        <button type="button" @click="navigate" class="btn btn-sm btn-outline-primary rounded-pill"><i class="bi bi-backspace"></i> Back</button>
                    </router-link>
                </div>
                <div class="card flex-fill mt-2 mb-3">
					<div class="card-body p-3">
                        <div class="row">
                            <div class="col-12">
                                <strong><i class="bi bi-card-checklist text-info"></i> Summary</strong>
                                <ul class="list-group list-group-flush">
                                    <li class="list-group-item">
                                        <div class="fs-6">
                                            <span class="fw-normal">Started:</span>
                                            <span class="fw-light" title="Start Time"><i class="bi bi-calendar"></i> {{ job?.firedTime?.toLocaleDateTimeString() }}</span>
                                        </div>
                                    </li>
                                    <li v-if="job?.runTime" class="list-group-item">
                                        <div class="fs-6">
                                            <span class="fw-normal">Elapsed:</span>
                                            <span class="fw-light" title="Run Time"><i class="bi bi-stopwatch"></i> {{ job?.runTime }}</span>
                                        </div>
                                    </li>
                                </ul>
                            </div>
                            <div class="col-12">
                                <hr class="mt-2 mb-2">
                                <strong><i class="bi bi-activity text-success"></i> Logs</strong>
                                <p>
                                    <div id="console">
                                        <template v-for="jobLog in logs">
                                            <div :class="[jobLog.isError ? 'text-danger' : '']" class="col-12 text-start text-nowrap ps-1">{{ jobLog.content }}</div>
                                        </template>
                                    </div>
                                </p>
                            </div>
                            <div class="col-12" v-if="job?.errorMessage">
                                <div class="d-flex align-items-start">
                                    <div class="flex-grow-1">
                                        <strong><i class="bi bi-bug text-danger"></i> Error Message</strong>
                                        <p>{{ job?.errorMessage }}</p>
                                    </div>
                                </div>
                                <hr>
                            </div>
                            <div class="col-12">
                                <div class="d-flex align-items-start">
                                    <div class="flex-grow-1">
                                        <strong><i class="bi bi-paperclip text-warning"></i> Attachments <span class="badge bg-secondary">{{ attachments?.length }}</span></strong><br>
                                        <ul class="list-group list-group-flush">
                                            <li class="list-group-item" v-for="attachment in attachments">
                                                <a class="text-reset" :href="getAttachmentUrl(attachment)" title="Click to Download"><i class="bi bi-file-earmark-arrow-down"></i>{{ attachment.fileName }}</a>
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                                <hr>
                            </div>
                            <div class="col-12">
                                <div class="d-flex align-items-start">
                                    <div class="flex-grow-1">
                                        <strong><i class="bi bi-envelope text-secondary"></i> Emails <span class="badge bg-secondary">0</span></strong><br>
                                        <ul class="list-group list-group-flush">
                                            <li class="list-group-item">
                                                
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                            </div>
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