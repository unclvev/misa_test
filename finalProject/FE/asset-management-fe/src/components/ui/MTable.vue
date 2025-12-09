<template>
  <div class="m-table">
    <div class="m-table__container">
      <div class="m-table__header-wrapper">
        <table ref="headerTableRef" class="m-table__table">
          <thead class="m-table__header">
            <tr>
              <th v-if="selectable" class="m-table__checkbox-col">
                <MCheckbox
                  :model-value="allSelected"
                  @update:model-value="handleSelectAll"
                />
              </th>
              <th
                v-for="(column, index) in columns"
                :key="index"
                :class="['m-table__header-cell', column.align === 'right' ? 'text-right' : column.align === 'center' ? 'text-center' : '', column.class]"
              >
                {{ column.label }}
              </th>
            </tr>
          </thead>
        </table>
      </div>
      <div class="m-table__body-wrapper">
        <table class="m-table__table">
          <tbody class="m-table__body">
          <tr
            v-for="(row, rowIndex) in data"
            :key="rowIndex"
            :class="[
              'm-table__row',
              { 'm-table__row--selected': isRowSelected(row) }
            ]"
            @click="handleRowClick(row)"
          >
            <td v-if="selectable" class="m-table__checkbox-col">
              <MCheckbox
                :model-value="isRowSelected(row)"
                @update:model-value="(val) => handleRowSelect(row, val)"
                @click.stop
              />
            </td>
            <td
              v-for="(column, colIndex) in columns"
              :key="colIndex"
              :class="['m-table__cell', column.align === 'right' ? 'text-right' : column.align === 'center' ? 'text-center' : '', column.class]"
            >
              <slot
                :name="`cell-${column.key}`"
                :row="row"
                :value="row[column.key]"
                :column="column"
              >
                {{ formatCellValue(row[column.key], column) }}
              </slot>
            </td>
          </tr>
        </tbody>
        </table>
      </div>
    </div>
    <div v-if="showPagination" class="m-table__footer">
      <div class="m-table__footer-left">
        <div class="m-table__summary">
          Tổng số: <strong>{{ total }}</strong> bản ghi
        </div>
        <MDropdown
          :model-value="pageSize"
          :options="[10, 20, 50, 100, 200]"
          class="m-table__page-size m-table__page-size--up"
          @update:model-value="handlePageSizeChange"
        />
        <div class="m-table__page-controls">
          <button
            class="m-table__page-btn m-table__page-btn--nav"
            @click="goToPage(currentPage - 1)"
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
              <use href="#icon-left-2" />
            </svg>
          </button>
          <button
            v-for="page in visiblePages"
            :key="page"
            :class="[
              'm-table__page-btn',
              { 'm-table__page-btn--active': page === currentPage }
            ]"
            :disabled="page === '...'"
            @click="goToPage(page)"
          >
            {{ page === '...' ? '...' : page }}
          </button>
          <button
            class="m-table__page-btn m-table__page-btn--nav"
            @click="goToPage(currentPage + 1)"
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
              <use href="#icon-right-2" />
            </svg>
          </button>
        </div>
      </div>
      <div v-if="summaryRow" class="m-table__footer-right">
        <div class="m-table__summary-wrapper">
          <table ref="summaryTableRef" class="m-table__summary-table">
            <tbody>
              <tr>
                <td v-if="selectable" class="m-table__summary-checkbox-col"></td>
                <td
                  v-for="(column, index) in columns"
                  :key="index"
                  :class="['m-table__summary-cell', column.align === 'right' ? 'text-right' : column.align === 'center' ? 'text-center' : '', column.class]"
                >
                  <span v-if="summaryRow[column.key]" class="m-table__summary-value">
                    {{ formatCellValue(summaryRow[column.key], column) }}
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, nextTick, onUpdated } from 'vue'
import MCheckbox from './MCheckbox.vue'
import MDropdown from './MDropdown.vue'

const props = defineProps({
  columns: {
    type: Array,
    required: true
  },
  data: {
    type: Array,
    default: () => []
  },
  selectable: {
    type: Boolean,
    default: false
  },
  selectedRows: {
    type: Array,
    default: () => []
  },
  total: {
    type: Number,
    default: 0
  },
  currentPage: {
    type: Number,
    default: 1
  },
  pageSize: {
    type: Number,
    default: 20
  },
  showPagination: {
    type: Boolean,
    default: true
  },
  summaryRow: {
    type: Object,
    default: null
  }
})

const emit = defineEmits([
  'update:selectedRows',
  'update:currentPage',
  'update:pageSize',
  'row-click',
  'page-change'
])

const allSelected = computed(() => {
  if (!props.selectable || props.data.length === 0) return false
  return props.data.every((row) => isRowSelected(row))
})

const totalPages = computed(() => {
  return Math.ceil(props.total / props.pageSize)
})

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = props.currentPage

  if (total <= 7) {
    for (let i = 1; i <= total; i++) {
      pages.push(i)
    }
  } else {
    if (current <= 3) {
      for (let i = 1; i <= 4; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(total)
    } else if (current >= total - 2) {
      pages.push(1)
      pages.push('...')
      for (let i = total - 3; i <= total; i++) {
        pages.push(i)
      }
    } else {
      pages.push(1)
      pages.push('...')
      for (let i = current - 1; i <= current + 1; i++) {
        pages.push(i)
      }
      pages.push('...')
      pages.push(total)
    }
  }

  return pages
})

const isRowSelected = (row) => {
  return props.selectedRows.some((selected) => selected === row)
}

const handleSelectAll = (value) => {
  if (value) {
    emit('update:selectedRows', [...props.data])
  } else {
    emit('update:selectedRows', [])
  }
}

const handleRowSelect = (row, value) => {
  let newSelected = [...props.selectedRows]
  if (value) {
    if (!isRowSelected(row)) {
      newSelected.push(row)
    }
  } else {
    newSelected = newSelected.filter((r) => r !== row)
  }
  emit('update:selectedRows', newSelected)
}

const handleRowClick = (row) => {
  emit('row-click', row)
}

const formatCellValue = (value, column) => {
  if (value === null || value === undefined) return ''
  if (column.formatter && typeof column.formatter === 'function') {
    return column.formatter(value)
  }
  return value
}

const goToPage = (page) => {
  if (page === '...' || page < 1 || page > totalPages.value || page === props.currentPage) {
    return
  }
  emit('update:currentPage', page)
  emit('page-change', page)
}

const handlePageSizeChange = (value) => {
  const newPageSize = typeof value === 'number' ? value : parseInt(value, 10)
  emit('update:pageSize', newPageSize)
  emit('update:currentPage', 1)
}

// Refs for syncing column widths
const headerTableRef = ref(null)
const summaryTableRef = ref(null)
const summaryWrapperRef = ref(null)

// Sync column widths from header to summary table
const syncColumnWidths = () => {
  if (!headerTableRef.value || !summaryTableRef.value) return

  nextTick(() => {
    const headerTable = headerTableRef.value
    const summaryTable = summaryTableRef.value
    
    if (!headerTable || !summaryTable) return

    // Get all header cells (excluding checkbox if present)
    const headerCells = headerTable.querySelectorAll('thead th')
    const summaryCells = summaryTable.querySelectorAll('tbody td')
    
    if (headerCells.length === 0 || summaryCells.length === 0) return

    // Sync checkbox column width if selectable
    if (props.selectable) {
      const headerCheckbox = headerTable.querySelector('.m-table__checkbox-col')
      const summaryCheckbox = summaryTable.querySelector('.m-table__summary-checkbox-col')
      if (headerCheckbox && summaryCheckbox) {
        summaryCheckbox.style.width = headerCheckbox.offsetWidth + 'px'
        summaryCheckbox.style.minWidth = headerCheckbox.offsetWidth + 'px'
        summaryCheckbox.style.maxWidth = headerCheckbox.offsetWidth + 'px'
        summaryCheckbox.style.paddingLeft = window.getComputedStyle(headerCheckbox).paddingLeft
        summaryCheckbox.style.paddingRight = window.getComputedStyle(headerCheckbox).paddingRight
      }
    }

    // Sync each column width
    // The number of columns to sync equals props.columns.length
    const columnCount = props.columns.length
    const headerStartIndex = props.selectable ? 1 : 0
    const summaryStartIndex = props.selectable ? 1 : 0
    
    for (let i = 0; i < columnCount; i++) {
      const headerCell = headerCells[headerStartIndex + i]
      const summaryCell = summaryCells[summaryStartIndex + i]
      
      if (headerCell && summaryCell) {
        const headerWidth = headerCell.offsetWidth
        const headerStyle = window.getComputedStyle(headerCell)
        
        summaryCell.style.width = headerWidth + 'px'
        summaryCell.style.minWidth = headerWidth + 'px'
        summaryCell.style.maxWidth = headerWidth + 'px'
        summaryCell.style.paddingLeft = headerStyle.paddingLeft
        summaryCell.style.paddingRight = headerStyle.paddingRight
      }
    }
    
    // Sync total table width to ensure proper alignment
    const headerTableWidth = headerTable.offsetWidth
    if (summaryTable.offsetWidth !== headerTableWidth) {
      summaryTable.style.width = headerTableWidth + 'px'
    }
  })
}

// Watch for changes and sync widths
watch(() => [props.columns, props.selectable, props.summaryRow], () => {
  syncColumnWidths()
}, { deep: true })

onMounted(() => {
  syncColumnWidths()
  // Also sync after a short delay to ensure DOM is fully rendered
  setTimeout(syncColumnWidths, 100)
})

onUpdated(() => {
  syncColumnWidths()
})
</script>

<style scoped>
.m-table {
  width: 100%;
  background-color: #ffffff;
  border-radius: 4px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  height: 100%;
}

.m-table__container {
  overflow: hidden;
  display: flex;
  flex-direction: column;
  flex: 1;
  min-height: 0;
}

.m-table__header-wrapper {
  flex-shrink: 0;
  overflow: hidden;
}

.m-table__body-wrapper {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  min-height: 0;
  scrollbar-width: thin;
  scrollbar-color: #d0d0d0 transparent;
}

.m-table__body-wrapper::-webkit-scrollbar {
  width: 6px;
}

.m-table__body-wrapper::-webkit-scrollbar-track {
  background: transparent;
}

.m-table__body-wrapper::-webkit-scrollbar-thumb {
  background: #d0d0d0;
  border-radius: 3px;
}

.m-table__body-wrapper::-webkit-scrollbar-thumb:hover {
  background: #b0b0b0;
}

.m-table__table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
}

.m-table__header-wrapper .m-table__table,
.m-table__body-wrapper .m-table__table {
  width: 100%;
  box-sizing: border-box;
}

.m-table__body-wrapper {
  padding-right: 0;
  margin-right: 0;
}

.m-table__header {
  background-color: #f5f5f5;
}

.m-table__body {
  display: table-row-group;
}

.m-table__header-cell {
  padding: 12px 16px;
  text-align: left;
  font-weight: 700;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  color: #001031;
  border-bottom: 1px solid #e0e0e0;
  white-space: nowrap;
  height: 40px;
}

.m-table__header-cell.text-right {
  text-align: right;
}

.m-table__header-cell.text-center {
  text-align: center;
}

.m-table__header tr th:first-child.m-table__checkbox-col {
  padding-left: 16px;
}

.m-table__body tr td:first-child.m-table__checkbox-col {
  padding-left: 16px;
}

.m-table__checkbox-col {
  width: 40px;
  padding: 12px 16px;
  text-align: left;
  position: relative;
  z-index: 10;
  overflow: visible;
}

.m-table__body {
  background-color: #ffffff;
}

.m-table__row {
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  transition: background-color 0.2s;
  cursor: pointer;
  height: 40px;
}

.m-table__row:hover {
  background-color: rgba(26, 164, 200, 0.2);
}

.m-table__row--selected {
  background-color: #e3f2fd;
}

.m-table__cell {
  padding: 12px 16px;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  color: #001031;
  border-bottom: 1px solid rgba(0, 0, 0, 0.1);
  font-weight: 400;
}

/* Style cho cột STT - cột đầu tiên sau checkbox */
.m-table__header tr th:first-child.m-table__checkbox-col + th,
.m-table__body tr td:first-child.m-table__checkbox-col + td {
  padding-left: 25px;
  padding-right: 25px;
  width: 30px;
  min-width: 30px;
  max-width: 30px;
  text-align: center;
}

/* Style cho cột Mã tài sản - cột thứ 2 sau checkbox (thứ 3 nếu có checkbox) */
.m-table__header tr th:first-child.m-table__checkbox-col + th + th,
.m-table__body tr td:first-child.m-table__checkbox-col + td + td {
  padding-right: 30px;
}

/* Style cho cột Tên tài sản - cột thứ 3 sau checkbox (thứ 4 nếu có checkbox) */
.m-table__header tr th:first-child.m-table__checkbox-col + th + th + th,
.m-table__body tr td:first-child.m-table__checkbox-col + td + td + td {
  width: 300px;
  min-width: 300px;
  padding-left: 80px;
}

/* Style cho cột Loại tài sản - cột thứ 4 sau checkbox (thứ 5 nếu có checkbox) */
.m-table__header tr th:first-child.m-table__checkbox-col + th + th + th + th,
.m-table__body tr td:first-child.m-table__checkbox-col + td + td + td + td {
  width: 200px;
  min-width: 200px;
  padding-left: 10px;
}

/* Style cho cột Bộ phận sử dụng - cột thứ 5 sau checkbox (thứ 6 nếu có checkbox) */
.m-table__header tr th:first-child.m-table__checkbox-col + th + th + th + th + th,
.m-table__body tr td:first-child.m-table__checkbox-col + td + td + td + td + td {
  width: 260px !important;
  min-width: 200px !important;
  max-width: 200px !important;
  padding-left: 20px;
  box-sizing: border-box;
}

/* Style cho cột Số lượng - cột thứ 6 sau checkbox (thứ 7 nếu có checkbox) */
.m-table__header tr th:first-child.m-table__checkbox-col + th + th + th + th + th + th,
.m-table__body tr td:first-child.m-table__checkbox-col + td + td + td + td + td + td {
  width: 100px;
  min-width: 20px;
  padding-left: 16px;
  padding-right: 16px;
  text-align: right;
}

.m-table__cell.text-right {
  text-align: right;
}

.m-table__cell.text-center {
  text-align: center;
}

/* Footer - Căn chỉnh như ảnh */
.m-table__footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 16px;
  border-top: 1px solid #e0e0e0;
  background-color: #ffffff;
  min-height: 48px;
  box-sizing: border-box;
  gap: 16px;
  position: relative;
  overflow: visible;
}

.m-table__footer-left {
  display: flex;
  align-items: center;
  gap: 16px;
  flex: 1;
}

.m-table__summary {
  white-space: nowrap;
  font-size: 12px;
  font-family: 'Roboto', sans-serif;
  color: #001031;
}

.m-table__summary strong {
  color: #1aa4c8;
  font-weight: 700;
}

.m-table__page-size {
  min-width: 50px !important;
  width: 50px !important;
  max-width: 50px !important;
  position: relative;
  z-index: 10;
}

.m-table__page-size :deep(.m-dropdown) {
  width: 50px !important;
  min-width: 50px !important;
  max-width: 50px !important;
  position: relative;
}

.m-table__page-size :deep(.m-dropdown__trigger) {
  height: 32px;
  padding: 0 2px 0 4px;
  border: 1px solid #d0d0d0;
  border-radius: 4px;
  font-size: 12px;
  font-family: 'Roboto', sans-serif;
  background-color: #ffffff;
  color: #001031;
  box-sizing: border-box;
  width: 50px !important;
  min-width: 50px !important;
  max-width: 50px !important;
  gap: 2px;
}

.m-table__page-size :deep(.m-dropdown__icon) {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
}

.m-table__page-size :deep(.m-dropdown__value) {
  flex: 1;
  min-width: 0;
  text-align: left;
}

.m-table__page-size :deep(.m-dropdown__menu) {
  border: none;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 1001 !important;
}

.m-table__page-size--up :deep(.m-dropdown__menu) {
  top: auto;
  bottom: calc(100% + 4px);
  z-index: 1001 !important;
}

.m-table__page-size :deep(.m-dropdown__item) {
  padding: 0 8px;
  font-size: 12px;
  height: 32px;
  line-height: 32px;
}



.m-table__page-controls {
  display: flex;
  align-items: center;
  gap: 4px;
}

.m-table__page-btn {
  min-width: 32px;
  height: 32px;
  padding: 0 8px;
  border: 1px solid #d0d0d0;
  border-radius: 4px;
  background-color: #ffffff;
  color: #001031;
  font-size: 12px;
  font-family: 'Roboto', sans-serif;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.m-table__page-btn:not(.m-table__page-btn--nav) {
  background-color: transparent;
  border-radius: 3px;
  width: 20px;
  height: 20px;
  overflow: hidden;
  min-width: 20px;
  padding: 0;
  border: none;
}

.m-table__page-btn--active {
  background-color: #f5f5f5 !important;
  color: #001031;
  border: none;
}

.m-table__page-btn:not(.m-table__page-btn--nav):hover:not(:disabled):not(.m-table__page-btn--active) {
  background-color: #f5f5f5;
  border: none;
}

.m-table__page-btn--nav:hover:not(:disabled) {
  background-color: #f5f5f5;
  border: none !important;
}

.m-table__page-btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
  background-color: #f5f5f5;
}

.m-table__page-btn--nav {
  border: none !important;
  padding: 0;
  min-width: 32px;
}

.m-table__page-btn--nav svg {
  display: block;
  margin: 0 auto;
}

/* Footer Right - Summary values */
.m-table__footer-right {
  flex: 1;
  display: flex;
  align-items: center;
  overflow: hidden;
  padding-right: 6px;
}

.m-table__summary-wrapper {
  width: 100%;
  overflow: hidden;
}

.m-table__summary-table {
  width: 100%;
  border-collapse: collapse;
  table-layout: fixed;
  box-sizing: border-box;
}

.m-table__summary-table tr {
  display: table-row;
}

.m-table__summary-checkbox-col {
  width: 40px;
  padding: 0 16px;
}

.m-table__summary-cell {
  padding: 0 16px;
  text-align: left;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  box-sizing: border-box;
  overflow: hidden;
  white-space: nowrap;
}

.m-table__summary-cell.text-right {
  text-align: right;
}

.m-table__summary-cell.text-center {
  text-align: center;
}

.m-table__summary-value {
  font-weight: 700;
  color: #ff5722;
  display: inline-block;
}
</style>