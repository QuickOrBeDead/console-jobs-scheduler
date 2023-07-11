import './style.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.min.js'
import 'bootstrap-icons/font/bootstrap-icons.css'

import App from './App.vue'
import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'

import Scheduler from './components/Scheduler.vue'
import JobsList from './components/jobs/List.vue'
import JobsEdit from './components/jobs/Edit.vue'
import HistoryList from './components/history/List.vue'

const routes = [
{ path: '/', component: Scheduler },
{ path: '/jobs', component: JobsList },
{ path: '/jobs/edit', component: JobsEdit, name: 'EditJob', props: true },
{ path: '/history', component: HistoryList }
]

createApp(App).use(createRouter({  history : createWebHistory(), routes: routes, linkActiveClass: 'active' })).mount('#app')

declare global {
    interface String {
        toLocaleDateTimeString(): string;
    }
  }

String.prototype.toLocaleDateTimeString = function (this: string) {
    var s = this
    if (!s) {
        return ''
    }
    
    return new Date(s).toLocaleString()
}