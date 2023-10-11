import './style.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.min.js'
import 'bootstrap-icons/font/bootstrap-icons.css'

import App from './App.vue'
import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'

import Pagination from './components/common/Pagination.vue'
import Scheduler from './components/Scheduler.vue'
import JobsList from './components/jobs/List.vue'
import JobsEdit from './components/jobs/Edit.vue'
import HistoryList from './components/history/List.vue'
import JobExecutionDetail from './components/history/Detail.vue'
import PackagesList from './components/packages/List.vue'
import PackagesEdit from './components/packages/Edit.vue'

const routes = [
{ path: '/', component: Scheduler },
{ path: '/jobs', component: JobsList },
{ path: '/jobs/edit', component: JobsEdit, name: 'EditJob', props: true },
{ path: '/history/:jobName?', component: HistoryList, name: 'JobHistory' },
{ path: '/history/details/:id', component: JobExecutionDetail, name: 'JobExecutionDetails', props: true },
{ path: '/packages', component: PackagesList },
{ path: '/packages/details/:name?', component: PackagesEdit, name: 'EditPackage' }
]

createApp(App)
    .use(createRouter({  history : createWebHistory(), routes: routes, linkActiveClass: 'active' }))
    .component('pagination', Pagination)
    .mount('#app')

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