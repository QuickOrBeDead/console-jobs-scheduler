<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { createApi } from '../api'
import { SchedulerApi, SchedulerJobExecutionStatisticsModel, SchedulerMetadataModel, SchedulerStateRecordModel } from '../metadata/console-jobs-scheduler-api'
import { createTypedChart } from 'vue-chartjs'
import { Chart as ChartJS, Tooltip, Legend, BarElement, CategoryScale, LinearScale, TimeScale, ChartData, ChartOptions, BarController } from 'chart.js'
import 'chartjs-adapter-date-fns'

ChartJS.register(Tooltip, Legend, BarElement, CategoryScale, LinearScale, TimeScale)

const Bar = createTypedChart<'bar', {x: string, y: number} []>('bar', BarController)

const statistics = ref<SchedulerJobExecutionStatisticsModel>()
const metadata = ref<SchedulerMetadataModel>()
const nodes = ref<SchedulerStateRecordModel[]>()

const historyChartData = ref<ChartData<'bar', {x: string, y: number} []>>()

const historyChartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
        legend: {
            display: false
        }
    },
    scales: {
      xAxis: {
        type: "time",
        min: new Date().setHours(0, 0, 0),
        max: new Date().setHours(23, 59, 59),
        time: {
          unit: "hour",
          displayFormats: {
            hour: "HH:mm",
          }
        }
      }
    }
}

const schedulerApi = createApi(SchedulerApi)

onMounted(async () => {
    const { data } = await schedulerApi.apiSchedulerGet()
    statistics.value = data.statistics
    metadata.value = data.metadata
    nodes.value = data.nodes ? data.nodes : []

    loadChart()
})

async function loadChart() {
    const { data } = await schedulerApi.apiSchedulerGetJobHistoryChartDataGet()
    historyChartData.value = {
        datasets: [ 
            { 
                data: data as {x: string, y: number} [],
                backgroundColor: '#4bc0c0' 
            } 
        ]
    }
}
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
            <div class="col">
                <div class="card mb-4">
                    <div class="card-header"><h3 class="mb-0 mt-0">History</h3></div>
                    <div class="card-body p-0"><Bar v-if="historyChartData" :options="historyChartOptions" :data="historyChartData">Chart couldn't be loaded.</Bar></div>
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