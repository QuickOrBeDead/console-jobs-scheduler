<script setup lang="ts">
import { onMounted } from 'vue';
import { createApi } from '../api'
import { SchedulerApi } from '../metadata/console-jobs-scheduler-api'

onMounted(async () => {
    const schedulerApi = createApi(SchedulerApi)
    const data = await schedulerApi.apiSchedulerGet()
    console.log(data.data)
})
</script>

<template>
 
</template>

<style scoped>
</style>
