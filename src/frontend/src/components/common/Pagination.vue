<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'

const emit = defineEmits(['pageChanged'])

const props = defineProps<{
  totalPages: number
}>()

let currentPage: number = 1
let nextPage = 0
let previousPage = 0
let start = 0
let end = 0
const pages =  ref<number[]>()

watch(() => props.totalPages, () => {
    setCurrentPage(currentPage)
})

onMounted(async () => { 
   setCurrentPage(1)
})

function setCurrentPage(page: number)  {
    currentPage = page

    start = Math.max(1, currentPage - 5)
    end = Math.min(start + 10, props.totalPages)
    if (end - start < 10)
    {
        start = Math.max(1, end - 10)
    }

    nextPage = Math.min(currentPage + 1, props.totalPages)
    previousPage = Math.max(currentPage - 1, start)

    const pageList: number[] = []
    for (let i = start; i <= end; i++) {
        pageList.push(i) 
    }

    emit('pageChanged', page)

    pages.value = pageList
}
</script>

<template>
 <ul class="pagination justify-content-center">
    <li :class="[currentPage == start ? 'disabled': '']" class="page-item">
        <a class="page-link" href="javascript:void(0)" @click="setCurrentPage(previousPage)" aria-label="Previous">
            <span aria-hidden="true">«</span>
        </a>
    </li>
    <template v-for="pageNumber in pages">
        <li :class="[pageNumber == currentPage ? 'disabled': '']" class="page-item">
            <a class="page-link" href="javascript:void(0)" @click="setCurrentPage(pageNumber)">{{ pageNumber }}</a>
        </li>
    </template>
    <li :class="[currentPage == end ? 'disabled': '']" class="page-item">
        <a class="page-link" href="javascript:void(0)" @click="setCurrentPage(nextPage)" aria-label="Next">
            <span aria-hidden="true">»</span>
        </a>
    </li>
</ul>
</template>