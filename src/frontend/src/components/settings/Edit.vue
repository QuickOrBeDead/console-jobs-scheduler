<script setup lang="ts">
import Smtp from './Smtp.vue'
import General from './General.vue'
import { ref } from 'vue'

const currentTab = ref('General')

const tabs: { [key: string]: any } = {
  'General': General,
  'Smtp': Smtp
}
</script>

<template>
    <div class="page-container">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-md-8">
                    <ul class="nav nav-tabs mb-3" role="tablist">
                        <li class="nav-item" role="presentation" v-for="(_, tab) in tabs">
                            <button @click="() => currentTab = tab as string" :class="['nav-link', { active: currentTab === tab }]" :id="`${tab}-tab-button`" data-bs-toggle="tab" type="button" role="tab" aria-controls="settings-tab" :aria-selected="currentTab === tab">
                                {{ tab }}
                            </button>
                        </li>
                    </ul>
                </div>
                <div class="tab-content">
                    <div class="tab-pane fade show active" id="settings-tab" role="tabpanel" :aria-labelledby="`${currentTab}-tab-button`">
                        <component :is="tabs[currentTab] as string"></component>  
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>