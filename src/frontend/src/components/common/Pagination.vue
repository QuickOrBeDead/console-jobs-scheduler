<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'

const emit = defineEmits(['pageChanged'])

const props = defineProps<{
  totalPages: number | undefined,
  totalCount: number | undefined,
  pageSize: number | undefined,
  page: number | undefined
}>()

let currentPage: number = 1
let nextPage = 0
let previousPage = 0
let start = 0
let end = 0
const pages = ref<number[]>()

watch(() => props.totalPages, () => {
   updatePageList()
})

watch(() => props.page, () => {
   currentPage = props.page ? props.page : 0
   updatePageList()
})

onMounted(async () => { 
   setCurrentPage(1)
})

function setCurrentPage(page: number)  {
    currentPage = page

    updatePageList()

    emit('pageChanged', page)
}

function updatePageList() {
    const totalPages = props.totalPages ? props.totalPages : 0

    start = Math.max(1, currentPage - 5)
    end = Math.min(start + 10, totalPages)
    if (end - start < 10)
    {
        start = Math.max(1, end - 10)
    }

    nextPage = Math.min(currentPage + 1, totalPages)
    previousPage = Math.max(currentPage - 1, start)

    const pageList: number[] = []
    for (let i = start; i <= end; i++) {
        pageList.push(i) 
    }

    pages.value = pageList
}

function getPageSize() {
    return props.pageSize ? props.pageSize : 0
}

function getTotalCount() {
    return props.totalCount ? props.totalCount : 0
}
</script>

<template>
    <div class="pagination-container">
        <div class="float-start justify-content-start text-muted mb-2"><small class="align-bottom">Showing {{ ((currentPage - 1) * getPageSize()) + 1 }} to {{ Math.min(currentPage * getPageSize(), getTotalCount()) }} of {{ getTotalCount() }} rows</small></div>   
        <ul class="float-end pagination pagination-sm justify-content-end mb-2">
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
    </div>
</template>
<style scoped>
    .pagination-container {
        display: block;
        clear: both;
    }
</style>