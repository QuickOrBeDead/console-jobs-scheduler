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
    nodes.value = data.nodes ? data.nodes : [];
})
</script>

<template>
<div class="container" style="margin-top: 10px;">
    <div class="row">
        <div class="col-md-12">
            <h4 class="display-6" style="margin-bottom: 0;">Statistics</h4>
            <hr style="margin: 4px 0px;">
        </div>
    </div>
    <div class="row">
        <div class="col-md-2">
            <div class="counter-box" style="background-color: #0c5a0c; background-image: linear-gradient(#107c10, #0c5a0c);">
                <i class="bi bi-check"></i>
                <span class="counter">{{ statistics ? statistics.totalSucceededJobs : 0 }}</span>
                <p>Jobs Succeeded</p>
            </div>
        </div>
        <div class="col-md-2">
            <div class="counter-box" style="background-color: #c42121; background-image: linear-gradient(#db2828, #c42121);">
                <i class="bi bi-x"></i>
                <span class="counter">{{ statistics ? statistics.totalFailedJobs : 0 }}</span>
                <p>Jobs Failed</p>
            </div>
        </div>
        <div class="col-md-2">
            <div class="counter-box" style="background-color: #1c70b0; background-image: linear-gradient(#2185d0, #1c70b0);">
                <i class="bi bi-play"></i>
                <span class="counter">{{ statistics ? statistics.totalRunningJobs : 0 }}</span>
                <p>Jobs Running</p>
            </div>
        </div>
        <div class="col-md-2">
            <div class="counter-box" style="background-color: rgb(235, 178, 8); background-image: linear-gradient(rgb(198, 153, 16), rgb(235, 178, 8));">
                <i class="bi bi-stop"></i>
                <span class="counter">{{ statistics ? statistics.totalVetoedJobs : 0 }}</span>
                <p>Jobs Vetoed</p>
            </div>
        </div>
    </div>	
    <div class="row">
        <div class="col-md-6">
            <h4 class="display-6" style="margin-bottom: 0;">Status</h4>
            <hr style="margin: 4px 0px;">
        </div>
        <div class="col-md-6">
            <h4 class="display-6" style="margin-bottom: 0;">Scheduler</h4>
            <hr style="margin: 4px 0px;">
        </div>
    </div>
    <div class="row">
        <div class="col-6">
            <table class="table table-striped table-bordered">
                <tbody>
                <tr>
                    <th scope="row">Running Since</th>
                    <td>{{ metadata?.runningSince?.toLocaleString() }}</td>
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
        <div class="col-6">
            <table class="table table-striped table-bordered">
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
    <div class="row">
        <div class="col-md-6">
            <h4 class="display-6" style="margin-bottom: 0;">Job Store</h4>
            <hr style="margin: 4px 0px;">
        </div>
        <div class="col-md-6">
            <h4 class="display-6" style="margin-bottom: 0;">Thread Pool</h4>
            <hr style="margin: 4px 0px;">
        </div>
    </div>
    <div class="row">
        <div class="col-6">
            <table class="table table-striped table-bordered">
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
        <div class="col-6">
            <table class="table table-striped table-bordered">
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
    <div class="row">
        <div class="col-md-12">
            <h4 class="display-6" style="margin-bottom: 0;">Nodes</h4>
            <hr style="margin: 4px 0px;">
        </div>
    </div>
    <div class="row" v-for="node in nodes">
        <div class="col-12">
            <table class="table table-striped table-bordered">
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
                    <td>{{ node.checkInTimestamp?.toLocaleString() }}</td>
                </tr>
                </tbody>
            </table>
        </div>
    </div>
</div>
</template>

<style scoped>
.counter-box {
    border-radius: 4px;
    text-align: center;
}

.counter-box p {
	color: #fff;
	font-size: 18px;
    text-shadow: 1px 2px 4px rgba(0,0,0,0.25);
}

.counter-box i {
	font-size: 60px;
	color: #fff;
    line-height: 70px;
    text-shadow: 1px 2px 4px rgba(0,0,0,0.25);
}

.counter { 
	font-size: 64px;
	color: #fff;
	line-height: 28px;
    font-weight: 700;
    text-shadow: 1px 2px 4px rgba(0,0,0,0.25);
}
</style>
