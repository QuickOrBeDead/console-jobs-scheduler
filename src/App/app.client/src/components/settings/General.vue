<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { callApi, createApi } from '../../api'
import { GeneralSettings, SettingsApi } from '../../metadata/console-jobs-scheduler-api'

const settingsApi = createApi(SettingsApi)

const settings = ref<GeneralSettings>()
const errors = ref<{[key: string]: string[]}>({})
const errorMessages = ref<string[]>([])

onMounted(async () => {
    const { data } = await settingsApi.apiSettingsGetGeneralSettingsGet()
    settings.value = data
})

async function save() {
    if (settings.value && !settings.value.pageSize) {
        settings.value.pageSize = null
    }

    const { hasError } = await callApi(() => settingsApi.apiSettingsSaveGeneralSettingsPost(settings.value), errors)

    if (!hasError) {
        console.log("saved")
    }
}

</script>

<template>
    <div class="card flex-fill">
        <div class="card-header">
            <h6 class="card-title mb-0 text-muted">Misc</h6>
        </div>
        <div class="card-body" v-if="settings">
            <div v-if="errorMessages && errorMessages.length" class="alert alert-danger" role="alert">
                <div v-for="msg in errorMessages" class="d-flex align-items-center">
                    <i class="bi bi-exclamation-triangle-fill"></i>&nbsp;
                    <div>{{ msg }}</div>
                </div>
            </div>
            <div class="row g-3">
                <div class="col-md-2 mb-3">
                    <label for="PageSize" class="form-label">Page Size</label>
                    <input id="PageSize" type="number" min="10" max="50" class="form-control" v-model.trim="settings.pageSize" :class="errors && errors.PageSize ? 'is-invalid' : ''"/>
                    <div v-if="errors && errors.PageSize" class="invalid-feedback" role="alert"><template v-for="msg in errors.PageSize">{{ msg }}<br></template></div>
                </div>
            </div>
            <button @click="save" class="btn btn-primary btn-sm">Save Changes</button>
        </div>
    </div>
</template>