<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { createApi } from '../api'
import { SchedulerApi, SchedulerJobExecutionStatisticsModel, SchedulerMetadataModel, SchedulerStateRecordModel } from '../metadata/console-jobs-scheduler-api'

const statistics = ref<SchedulerJobExecutionStatisticsModel>()
const metadata = ref<SchedulerMetadataModel>()
const nodes = ref<SchedulerStateRecordModel[]>()

onMounted(async () => {
    const schedulerApi = createApi(SchedulerApi)
    const { data } = await schedulerApi.apiSchedulerGet()
    statistics.value = data.statistics
    metadata.value = data.metadata
    nodes.value = data.nodes ? data.nodes : []
})
</script>

<template>
<div class="page-container">
    <div class="container">
        <div class="card mb-4">
            <div class="card-header"><h2 class="display-6 mb-0 mt-0">Statistics</h2></div>
            <div class="card-body">
                <div class="row">
                    <div class="col-sm-6">
                        <div class="row">
                            <div class="col-6">
                                <div class="border-start border-5 border-success px-3 mb-3"><span><i class="bi bi-check"></i> Jobs Succeeded</span>
                                <div class="fs-1 fw-semibold">{{ statistics ? statistics.totalSucceededJobs : 0 }}</div>
                                </div>
                            </div>
                            <div class="col-6">
                                <div class="border-start border-5 border-danger px-3 mb-3"><span><i class="bi bi-x"></i> Jobs Failed</span>
                                <div class="fs-1 fw-semibold">{{ statistics ? statistics.totalFailedJobs : 0 }}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-sm-6">
                        <div class="row">
                            <div class="col-6">
                                <div class="border-start border-5 border-info px-3 mb-3"><span><i class="bi bi-play"></i> Jobs Running</span>
                                <div class="fs-1 fw-semibold">{{ statistics ? statistics.totalRunningJobs : 0 }}</div>
                                </div>
                            </div>
                            <div class="col-6">
                                <div class="border-start border-5 border-warning px-3 mb-3"><span><i class="bi bi-stop"></i> Jobs Vetoed</span>
                                <div class="fs-1 fw-semibold">{{ statistics ? statistics.totalVetoedJobs : 0 }}</div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-6">
                <div class="card mb-4">
                    <div class="card-header"><h3 class="mb-0 mt-0">Scheduler</h3></div>
                    <div class="card-body p-0">
                        <table class="table table-condensed table-hover mb-0">
                            <tbody>
                            <tr>
                                <th scope="row">Name</th>
                                <td>{{ metadata?.schedulerName }}</td>
                            </tr>
                            <tr>
                                <th scope="row">Instance Id</th>
                                <td>{{ metadata?.schedulerInstanceId }}</td>
                            </tr>
                            <tr>
                                <th scope="row">Type</th>
                                <td>{{ metadata?.schedulerType }}</td>
                            </tr>
                            <tr>
                                <th scope="row">Remote</th>
                                <td>{{ metadata?.schedulerRemote ? "TRUE" : "FALSE" }}</td>
                            </tr>
                            <tr>
                                <th scope="row">Version</th>
                                <td>{{ metadata?.version }}</td>
                            </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
            <div class="col-sm-6">
                <div class="card mb-4">
                    <div class="card-header"><h3 class="mb-0 mt-0">Status</h3></div>
                    <div class="card-body p-0">
                        <table class="table table-condensed table-hover mb-0">
                            <tbody>
                            <tr>
                                <th scope="row">Running Since</th>
                                <td>{{ metadata?.runningSince?.toLocaleDateTimeString() }}</td>
                            </tr>
                            <tr>
                                <th scope="row">Started</th>
                                <td>{{ metadata?.started ? "TRUE" : "FALSE" }}</td>
                            </tr>
                            <tr>
                                <th scope="row">In Standby Mode</th>
                                <td>{{ metadata?.inStandbyMode ? "TRUE" : "FALSE" }}</td>
                            </tr>
                            <tr>
                                <th scope="row">Shutdown</th>
                                <td>{{ metadata?.shutdown ? "TRUE" : "FALSE" }}</td>
                            </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-6">
                <div class="card mb-4">
                    <div class="card-header"><h3 class="mb-0 mt-0">Job Store</h3></div>
                    <div class="card-body p-0">
                        <table class="table table-condensed table-hover mb-0">
                            <tbody>
                            <tr>
                                <th scope="row">Type</th>
                                <td>{{ metadata?.jobStoreType }}</td>
                            </tr>
                            <tr>
                                <th scope="row">Clustered</th>
                                <td>{{ metadata?.jobStoreClustered ? "TRUE" : "FALSE" }}</td>
                            </tr>
                            <tr>
                                <th scope="row">Supports Persistence</th>
                                <td>{{ metadata?.jobStoreSupportsPersistence ? "TRUE" : "FALSE" }}</td>
                            </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
            <div class="col-sm-6">
                <div class="card mb-4">
                    <div class="card-header"><h3 class="mb-0 mt-0">Thread Pool</h3></div>
                    <div class="card-body p-0">
                        <table class="table table-condensed table-hover mb-0">
                            <tbody>
                            <tr>
                                <th scope="row">Type</th>
                                <td>{{ metadata?.threadPoolType }}</td>
                            </tr>
                            <tr>
                                <th scope="row">Size</th>
                                <td>{{ metadata?.threadPoolSize }}</td>
                            </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
        <div class="row">
            <div class="col-sm-12">
                <div class="card mb-4">
                    <div class="card-header"><h3 class="mb-0 mt-0">Nodes</h3></div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col-sm-6" v-for="node in nodes">
                                <table class="table table-condensed table-hover">
                                    <tbody>
                                    <tr>
                                        <th scope="row">Instance Id</th>
                                        <td>{{ node.schedulerInstanceId }}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Check-in Interval</th>
                                        <td>{{ node.checkInInterval }}</td>
                                    </tr>
                                    <tr>
                                        <th scope="row">Check-in Timestamp</th>
                                        <td>{{ node.checkInTimestamp?.toLocaleDateTimeString() }}</td>
                                    </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
</template>