import './style.css'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.min.js'

import App from './App.vue'
import { createApp } from 'vue'
import { createRouter, createWebHistory } from 'vue-router'

import Scheduler from './components/Scheduler.vue'

const routes = [
{ path: '/', component: Scheduler }
]

createApp(App).use(createRouter({  history : createWebHistory(), routes: routes })).mount('#app')
