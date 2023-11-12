<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { callApi, createApi } from '../../api'
import { SettingsApi, SmtpSettings } from '../../metadata/console-jobs-scheduler-api'

const settingsApi = createApi(SettingsApi)

const settings = ref<SmtpSettings>()
const errors = ref<{[key: string]: string[]}>({})
const errorMessages = ref<string[]>([])

onMounted(async () => {
    const { data } = await settingsApi.apiSettingsGetSmtpSettingsGet()
    settings.value = data
})

async function save() {
    const model = settings.value
    if (model && !model.port) {
        model.port = undefined
    }

    const { hasError } = await callApi(() => settingsApi.apiSettingsSaveSmtpSettingsPost(model), errors)

    if (!hasError) {
        console.log("saved")
    }
}

</script>

<template>
     <div class="card flex-fill">
        <div class="card-header">
            <h6 class="card-title mb-0 text-muted">Settings</h6>
        </div>
        <div class="card-body" v-if="settings">
            <div v-if="errorMessages && errorMessages.length" class="alert alert-danger" role="alert">
                <div v-for="msg in errorMessages" class="d-flex align-items-center">
                    <i class="bi bi-exclamation-triangle-fill"></i>&nbsp;
                    <div>{{ msg }}</div>
                </div>
            </div>
            <div class="row g-3">
                <div class="col-md-6">
                    <label for="Host" class="form-label">Host</label>
                    <input id="Host" type="text" class="form-control" v-model.trim="settings.host" :class="errors && errors.Host ? 'is-invalid' : ''"/>
                    <div v-if="errors && errors.Host" class="invalid-feedback" role="alert"><template v-for="msg in errors.Host">{{ msg }}<br></template></div>
                </div>
                <div class="col-md-3">
                    <label for="Port" class="form-label">Port</label>
                    <input id="Port" type="number" min="1" max="65535" class="form-control" v-model.number="settings.port" :class="errors && errors.Port ? 'is-invalid' : ''"/>
                    <div v-if="errors && errors.Port" class="invalid-feedback" role="alert"><template v-for="msg in errors.Port">{{ msg }}<br></template></div>
                </div>
                <div class="col-md-3">
                    <label class="form-label">&nbsp;</label>
                    <div class="form-check mt-2">
                        <input id="EnableSsl" type="checkbox" class="form-check-input" v-model.trim="settings.enableSsl" :class="errors && errors.EnableSsl ? 'is-invalid' : ''"/>
                        <label for="EnableSsl" class="form-check-label">Enable Ssl</label>
                    </div>
                    <div v-if="errors && errors.EnableSsl" class="invalid-feedback" role="alert"><template v-for="msg in errors.EnableSsl">{{ msg }}<br></template></div>
                </div>
                <div class="col-md-6">
                    <label for="From" class="form-label">From</label>
                    <input id="From" type="text" class="form-control" v-model.trim="settings.from" :class="errors && errors.From ? 'is-invalid' : ''"/>
                    <div v-if="errors && errors.From" class="invalid-feedback" role="alert"><template v-for="msg in errors.From">{{ msg }}<br></template></div>
                </div>
                <div class="col-md-6">
                    <label for="FromName" class="form-label">From Name</label>
                    <input id="FromName" type="text" class="form-control" v-model.trim="settings.fromName" :class="errors && errors.FromName ? 'is-invalid' : ''"/>
                    <div v-if="errors && errors.FromName" class="invalid-feedback" role="alert"><template v-for="msg in errors.FromName">{{ msg }}<br></template></div>
                </div>
                <fieldset class="border rounded-3 p-3 mb-3">
                    <legend class="float-none w-auto px-3"><small>Credentials</small></legend>
                    <div class="col-md-12">
                        <label for="Domain" class="form-label">Domain</label>
                        <input id="Domain" type="text" class="form-control" v-model.trim="settings.domain" :class="errors && errors.Domain ? 'is-invalid' : ''"/>
                        <div v-if="errors && errors.Domain" class="invalid-feedback" role="alert"><template v-for="msg in errors.Domain">{{ msg }}<br></template></div>
                    </div>
                    <div class="col-md-12">
                        <label for="UserName" class="form-label">Username</label>
                        <input id="UserName" type="text" class="form-control" v-model.trim="settings.userName" :class="errors && errors.UserName ? 'is-invalid' : ''"/>
                        <div v-if="errors && errors.UserName" class="invalid-feedback" role="alert"><template v-for="msg in errors.UserName">{{ msg }}<br></template></div>
                    </div>
                    <div class="col-md-12">
                        <label for="Password" class="form-label">Password</label>
                        <input id="Password" type="text" class="form-control" v-model="settings.password" :class="errors && errors.Password ? 'is-invalid' : ''"/>
                        <div v-if="errors && errors.Password" class="invalid-feedback" role="alert"><template v-for="msg in errors.Password">{{ msg }}<br></template></div>
                    </div>
                </fieldset>
            </div>
            <button @click="save" class="btn btn-primary btn-sm">Save Changes</button>
        </div>
    </div>
    <div class="row justify-content-center" >
        <div class="col-8">
            <div v-if="errorMessages && errorMessages.length" class="alert alert-danger" role="alert">
                <div v-for="msg in errorMessages" class="d-flex align-items-center">
                    <i class="bi bi-exclamation-triangle-fill"></i>&nbsp;
                    <div>{{ msg }}</div>
                </div>
            </div>
            
        </div>
    </div>
</template>