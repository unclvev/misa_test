<template>
    <div class="m-ant-table-wrapper" :class="{ 'm-ant-table-wrapper--bordered': showBorders }" ref="tableContainerRef">
      <div v-if="loading" class="loading-overlay">
        <div class="loading-spinner">Đang tải...</div>
      </div>
      <div v-if="error && !loading" class="error-message">
        <strong>Lỗi:</strong> {{ error }}
        <button class="error-retry-btn" @click="$emit('retry')">Thử lại</button>
      </div>
      <div class="table-wrapper">
        <a-table
          :columns="columns"
          :data-source="data"
          :row-selection="rowSelection"
          :pagination="false"
          :scroll="{ y: tableScrollHeight }"
          :bordered="false"
          :row-key="rowKey"
          :loading="loading"
          class="custom-ant-table"
          @row="handleTableRow"
        >
          <template #bodyCell="{ column, record }">
            <slot name="bodyCell" :column="column" :record="record" />
          </template>
        </a-table>
      </div>
      
      <!-- Filter Row -->
      <div v-if="showFilterRow" style="display: none;">
        <table>
          <tbody>
            <tr class="custom-filter-row" ref="filterRowRef">
              <td class="custom-filter-cell custom-filter-checkbox"></td>
              <td 
                v-for="col in columns" 
                :key="col.key"
                :class="[
                  'custom-filter-cell', 
                  col.align === 'right' ? 'text-right' : col.align === 'center' ? 'text-center' : 'text-left'
                ]"
              >
                <div class="filter-input-wrapper" v-if="col.key !== 'actions'">
                  <input
                    :value="filterValues[col.key]"
                    @input="handleFilterInput(col.key, $event)"
                    type="text"
                    :placeholder="`Lọc ${col.title?.toLowerCase() || ''}`"
                    class="filter-input"
                  />
                  <svg 
                    width="16" 
                    height="16" 
                    viewBox="0 0 24 24" 
                    fill="none" 
                    xmlns="http://www.w3.org/2000/svg"
                    class="filter-icon"
                  >
                    <path 
                      d="M4 6h16M4 12h16M4 18h16" 
                      stroke="#666666" 
                      stroke-width="2" 
                      stroke-linecap="round"
                    />
                    <circle cx="18" cy="6" r="3" fill="#1aa4c8" opacity="0.2"/>
                    <circle cx="18" cy="12" r="3" fill="#1aa4c8" opacity="0.2"/>
                    <circle cx="18" cy="18" r="3" fill="#1aa4c8" opacity="0.2"/>
                  </svg>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <!-- Summary Row with Pagination -->
      <div v-if="summary" ref="summaryWrapperRef" class="custom-summary-row">
        <div class="custom-summary-content">
          <!-- Pagination Controls -->
          <div class="custom-summary-pagination">
            <div class="custom-footer-summary">
              Tổng số: <strong>{{ totalRecords }}</strong> bản ghi
            </div>
            
            <MPageSizeDropdown
              :model-value="pagination.pageSize"
              :options="pageSizeOptions"
              :disabled="loading"
              class="custom-page-size custom-page-size--up"
              @update:model-value="handlePageSizeChange"
            />
            
            <div class="custom-page-controls">
              <button
                class="custom-page-btn custom-page-btn--nav"
                :disabled="pagination.currentPage === 1 || loading"
                @click="goToPage(pagination.currentPage - 1)"
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                  <use href="#icon-left-2" />
                </svg>
              </button>
              
              <button
                v-for="page in visiblePages"
                :key="page"
                :class="[
                  'custom-page-btn',
                  { 'custom-page-btn--active': page === pagination.currentPage }
                ]"
                :disabled="page === '...' || loading"
                @click="goToPage(page)"
              >
                {{ page === '...' ? '...' : page }}
              </button>
              
              <button
                class="custom-page-btn custom-page-btn--nav"
                :disabled="pagination.currentPage >= totalPages || loading"
                @click="goToPage(pagination.currentPage + 1)"
              >
                <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                  <use href="#icon-right-2" />
                </svg>
              </button>
            </div>
          </div>
          
          <!-- Summary Table -->
          <table ref="summaryTableRef" class="custom-summary-table">
            <tbody>
              <tr>
                <td ref="summaryCheckboxRef" class="custom-summary-checkbox-col"></td>
                <td v-for="(col, idx) in columns" :key="idx" 
                    :class="[
                      'custom-summary-cell',
                      col.align === 'right' ? 'text-right' : col.align === 'center' ? 'text-center' : 'text-left',
                      col.class
                    ]"
                >
                  <slot name="summaryCell" :column="col" :summary="summary">
                    <span v-if="summary[col.key]" class="custom-summary-value">
                      {{ formatSummaryValue(summary[col.key], col) }}
                    </span>
                  </slot>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </template>
  
  <script setup>
  import { ref, computed, onMounted, onUnmounted, nextTick, onUpdated, watch } from 'vue'
  import MPageSizeDropdown from './MPageSizeDropdown.vue'
  import { formatCurrency } from '../../utils/formatters'
  
  const props = defineProps({
    columns: {
      type: Array,
      required: true
    },
    data: {
      type: Array,
      default: () => []
    },
    loading: {
      type: Boolean,
      default: false
    },
    error: {
      type: String,
      default: null
    },
    rowSelection: {
      type: Object,
      default: null
    },
    rowKey: {
      type: [String, Function],
      default: (record) => record.id || record.assetCode || record.stt
    },
    pagination: {
      type: Object,
      required: true,
      default: () => ({
        currentPage: 1,
        pageSize: 20
      })
    },
    totalRecords: {
      type: Number,
      default: 0
    },
    summary: {
      type: Object,
      default: null
    },
    showFilterRow: {
      type: Boolean,
      default: false
    },
    filterValues: {
      type: Object,
      default: () => ({})
    },
    pageSizeOptions: {
      type: Array,
      default: () => [10, 20, 50, 100, 200]
    },
    showBorders: {
      type: Boolean,
      default: false
    }
  })
  
  const emit = defineEmits([
    'update:pagination',
    'update:filterValues',
    'row-click',
    'page-change',
    'page-size-change',
    'filter-change',
    'retry'
  ])
  
  const tableContainerRef = ref(null)
  const filterRowRef = ref(null)
  const summaryTableRef = ref(null)
  const summaryWrapperRef = ref(null)
  const summaryCheckboxRef = ref(null)
  const tableScrollHeight = ref(400)
  const windowWidth = ref(window.innerWidth)
  
  const totalPages = computed(() => {
    return Math.ceil(props.totalRecords / props.pagination.pageSize)
  })
  
  const visiblePages = computed(() => {
    const pages = []
    const total = totalPages.value
    const current = props.pagination.currentPage
  
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
  
  const handleTableRow = (record) => {
    return {
      onClick: (event) => {
        // Không trigger row-click nếu click vào checkbox
        if (event.target.closest('.ant-checkbox-wrapper') || 
            event.target.closest('.ant-table-selection-column')) {
          return
        }
        emit('row-click', record)
      }
    }
  }
  
  const goToPage = (page) => {
    if (page === '...' || page < 1 || page === props.pagination.currentPage) {
      return
    }
    emit('page-change', page)
    emit('update:pagination', {
      ...props.pagination,
      currentPage: page
    })
  }
  
  const handlePageSizeChange = (newSize) => {
    emit('page-size-change', newSize)
    emit('update:pagination', {
      ...props.pagination,
      pageSize: newSize,
      currentPage: 1
    })
  }
  
  const handleFilterInput = (key, event) => {
    const newFilterValues = {
      ...props.filterValues,
      [key]: event.target.value
    }
    emit('update:filterValues', newFilterValues)
    emit('filter-change', newFilterValues)
  }
  
  const formatSummaryValue = (value, column) => {
    if (column.customRender) {
      return column.customRender({ text: value })
    }
    if (typeof value === 'number' && (column.key?.includes('Price') || column.key?.includes('Value') || column.key?.includes('depreciation'))) {
      return formatCurrency(value)
    }
    return value
  }
  
  const insertFilterRow = () => {
    if (!props.showFilterRow || !filterRowRef.value) return
    
    nextTick(() => {
      const antTable = tableContainerRef.value?.querySelector('.custom-ant-table .ant-table')
      if (!antTable) {
        setTimeout(() => insertFilterRow(), 50)
        return
      }
      
      const thead = antTable.querySelector('.ant-table-thead')
      if (!thead) {
        setTimeout(() => insertFilterRow(), 50)
        return
      }
      
      const firstHeaderRow = thead.querySelector('tr:first-child')
      if (!firstHeaderRow) {
        setTimeout(() => insertFilterRow(), 50)
        return
      }
      
      const filterRow = filterRowRef.value
      const existingFilterRow = thead.querySelector('.custom-filter-row')
      if (existingFilterRow && existingFilterRow !== filterRow) {
        existingFilterRow.remove()
      }
      
      if (filterRow.parentNode && filterRow.parentNode.tagName === 'TBODY') {
        filterRow.remove()
      }
      
      if (filterRow.parentNode !== thead) {
        if (firstHeaderRow.nextSibling) {
          thead.insertBefore(filterRow, firstHeaderRow.nextSibling)
        } else {
          thead.appendChild(filterRow)
        }
      }
      
      filterRow.style.display = 'table-row'
    })
  }
  
  const syncSummaryColumnWidths = () => {
    if (!summaryTableRef.value) return
    
    nextTick(() => {
      const antTable = tableContainerRef.value?.querySelector('.custom-ant-table .ant-table')
      if (!antTable) return
      
      const headerCells = antTable.querySelectorAll('.ant-table-thead th')
      const summaryCells = summaryTableRef.value.querySelectorAll('td')
      
      if (headerCells.length === 0 || summaryCells.length === 0) return
      
      const headerCheckbox = antTable.querySelector('.ant-table-selection-column')
      if (headerCheckbox && summaryCheckboxRef.value) {
        const width = headerCheckbox.offsetWidth
        if (width > 0) {
          summaryCheckboxRef.value.style.width = width + 'px'
          summaryCheckboxRef.value.style.minWidth = width + 'px'
          summaryCheckboxRef.value.style.maxWidth = width + 'px'
        }
      }
      
      const headerStartIndex = 1
      const summaryStartIndex = 1
      const summaryDataCells = Array.from(summaryCells).slice(summaryStartIndex)
      
      for (let i = 0; i < props.columns.length && i < summaryDataCells.length; i++) {
        const headerCell = headerCells[headerStartIndex + i]
        const summaryCell = summaryDataCells[i]
        
        if (headerCell && summaryCell) {
          const width = headerCell.offsetWidth
          if (width > 0) {
            const headerStyle = window.getComputedStyle(headerCell)
            summaryCell.style.width = width + 'px'
            summaryCell.style.minWidth = width + 'px'
            summaryCell.style.maxWidth = width + 'px'
            summaryCell.style.paddingLeft = '20px'
            summaryCell.style.paddingRight = headerStyle.paddingRight
          }
        }
      }
      
      const tableWidth = antTable.offsetWidth
      if (tableWidth > 0 && summaryTableRef.value.offsetWidth !== tableWidth) {
        summaryTableRef.value.style.width = tableWidth + 'px'
      }
    })
  }
  
  const syncFilterRowColumnWidths = () => {
    if (!props.showFilterRow || !filterRowRef.value) return
    
    nextTick(() => {
      const antTable = tableContainerRef.value?.querySelector('.custom-ant-table .ant-table')
      if (!antTable) return
      
      const thead = antTable.querySelector('.ant-table-thead')
      if (!thead) return
      
      const headerRow = thead.querySelector('tr:first-child')
      const filterRow = thead.querySelector('.custom-filter-row')
      
      if (!headerRow || !filterRow) return
      
      const headerCells = headerRow.querySelectorAll('th')
      const filterCells = filterRow.querySelectorAll('td')
      
      if (headerCells.length === 0 || filterCells.length === 0) return
      
      for (let i = 0; i < headerCells.length && i < filterCells.length; i++) {
        const headerCell = headerCells[i]
        const filterCell = filterCells[i]
        
        if (headerCell && filterCell) {
          const width = headerCell.offsetWidth
          if (width > 0) {
            filterCell.style.width = width + 'px'
            filterCell.style.minWidth = width + 'px'
            filterCell.style.maxWidth = width + 'px'
            filterCell.style.boxSizing = 'border-box'
          }
        }
      }
    })
  }
  
  const calculateTableScrollHeight = () => {
    nextTick(() => {
      if (tableContainerRef.value) {
        const container = tableContainerRef.value
        const thead = container.querySelector('.ant-table-thead')
        if (!thead) return
        
        const headerRow = thead.querySelector('tr:first-child')
        const filterRow = thead.querySelector('.custom-filter-row')
        const headerHeight = headerRow?.offsetHeight || 40
        const filterHeight = filterRow?.offsetHeight || 0
        const totalHeaderHeight = headerHeight + filterHeight
        
        const summaryHeight = container.querySelector('.custom-summary-row')?.offsetHeight || 0
        const containerHeight = container.offsetHeight
        const availableHeight = containerHeight - totalHeaderHeight - summaryHeight - 2
        tableScrollHeight.value = Math.max(200, availableHeight)
      }
    })
  }
  
  const handleResize = () => {
    windowWidth.value = window.innerWidth
    calculateTableScrollHeight()
  }
  
  watch(() => [props.columns, props.data, props.summary], () => {
    insertFilterRow()
    syncSummaryColumnWidths()
    syncFilterRowColumnWidths()
    calculateTableScrollHeight()
  }, { deep: true })
  
  watch(() => windowWidth.value, () => {
    nextTick(() => {
      insertFilterRow()
      syncSummaryColumnWidths()
      syncFilterRowColumnWidths()
      calculateTableScrollHeight()
    })
  })
  
  onMounted(() => {
    insertFilterRow()
    syncSummaryColumnWidths()
    syncFilterRowColumnWidths()
    calculateTableScrollHeight()
    setTimeout(() => {
      insertFilterRow()
      syncSummaryColumnWidths()
      syncFilterRowColumnWidths()
      calculateTableScrollHeight()
    }, 100)
    
    window.addEventListener('resize', handleResize)
  })
  
  onUpdated(() => {
    insertFilterRow()
    syncSummaryColumnWidths()
    syncFilterRowColumnWidths()
    calculateTableScrollHeight()
  })
  
  onUnmounted(() => {
    window.removeEventListener('resize', handleResize)
  })
  </script>
  
  <style scoped>
  .m-ant-table-wrapper {
    background-color: #f4f7ff;
    border-radius: 3px;
    border: none;
    overflow: visible;
    margin-bottom: 17px;
    flex: 1;
    display: flex;
    flex-direction: column;
    min-height: 0;
    box-shadow: 0 3px 5px rgba(10, 83, 132, 0.161);
    position: relative;
  }
  
  .table-wrapper {
    display: flex;
    flex-direction: column;
    flex: 1;
    min-height: 0;
    background-color: #ffffff;
    overflow-x: hidden;
    overflow-y: hidden;
  }
  
  .loading-overlay {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(255, 255, 255, 0.8);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 10;
    border-radius: 3px;
  }
  
  .loading-spinner {
    font-size: 14px;
    color: #1aa4c8;
    font-weight: 500;
  }
  
  .error-message {
    padding: 16px;
    background-color: #ffebee;
    color: #c62828;
    border-radius: 3px;
    margin: 8px;
    font-size: 13px;
    font-family: 'Roboto', sans-serif;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
  }
  
  .error-retry-btn {
    padding: 6px 16px;
    background-color: #1aa4c8;
    color: white;
    border: none;
    border-radius: 3px;
    cursor: pointer;
    font-size: 12px;
    font-weight: 500;
    transition: background-color 0.2s;
  }
  
  .error-retry-btn:hover {
    background-color: #1588a8;
  }
  
  :deep(.custom-ant-table) {
    background-color: #ffffff;
    height: 100%;
    display: flex;
    flex-direction: column;
  }
  
  :deep(.custom-ant-table .ant-table-container) {
    flex: 1;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    min-height: 0;
    border-radius: 3px;
  }
  
  :deep(.custom-ant-table .ant-table) {
    background-color: #ffffff;
    border-radius: 3px;
    flex: 1;
    display: flex;
    flex-direction: column;
    border: none;
    overflow: hidden;
  }
  
  :deep(.custom-ant-table .ant-table table) {
    border-collapse: separate;
    border-spacing: 0;
  }
  
  :deep(.custom-ant-table .ant-table table td),
  :deep(.custom-ant-table .ant-table table th) {
    border-left: none !important;
  }
  
  .m-ant-table-wrapper--bordered :deep(.custom-ant-table .ant-table table th:not(:last-child)),
  .m-ant-table-wrapper--bordered :deep(.custom-ant-table .ant-table table td:not(:last-child)) {
    border-right: 1px solid #e0e0e0 !important;
  }
  
  :deep(.custom-ant-table .ant-table-body) {
    flex: 1;
    overflow-y: auto !important;
    overflow-x: auto !important;
  }
  
  :deep(.custom-ant-table .ant-table-body-wrapper) {
    flex: 1;
    overflow-y: auto !important;
    overflow-x: auto !important;
    min-height: 0;
  }
  
  :deep(.custom-ant-table .ant-table-body-wrapper::-webkit-scrollbar) {
    width: 8px;
    height: 8px;
  }
  
  :deep(.custom-ant-table .ant-table-body-wrapper::-webkit-scrollbar-track) {
    background: #f1f1f1;
    border-radius: 4px;
  }
  
  :deep(.custom-ant-table .ant-table-body-wrapper::-webkit-scrollbar-thumb) {
    background: #888;
    border-radius: 4px;
  }
  
  :deep(.custom-ant-table .ant-table-body-wrapper::-webkit-scrollbar-thumb:hover) {
    background: #555;
  }
  
  :deep(.custom-ant-table .ant-table-body-wrapper) {
    scrollbar-width: thin;
    scrollbar-color: #888 #f1f1f1;
  }
  
  :deep(.custom-ant-table .ant-table-thead > tr:first-child > th:first-child) {
    border-top-left-radius: 3px;
  }
  
  :deep(.custom-ant-table .ant-table-thead > tr:first-child > th:last-child) {
    border-top-right-radius: 3px;
  }
  
  :deep(.custom-ant-table .ant-table-thead > tr > th) {
    background-color: #f5f5f5;
    padding: 12px 16px;
    text-align: left;
    font-weight: 700;
    font-size: 13px;
    font-family: 'Roboto', sans-serif;
    color: #001031;
    border-bottom: none;
    border-left: none;
    border-top: none;
    white-space: nowrap;
    height: 40px;
  }
  
  .m-ant-table-wrapper--bordered :deep(.custom-ant-table .ant-table-thead > tr:first-child > th) {
    border-bottom: 1px solid #e0e0e0;
  }
  
  :deep(.custom-ant-table .ant-table-thead > tr > th.text-right) {
    text-align: right;
  }
  
  :deep(.custom-ant-table .ant-table-thead > tr > th.text-center) {
    text-align: center;
  }
  
  :deep(.custom-ant-table .ant-table-tbody > tr > td) {
    padding: 16px 16px;
    font-size: 13px;
    font-family: 'Roboto', sans-serif;
    color: #001031;
    border-bottom: none;
    border-left: none;
    border-top: none;
    font-weight: 400;
    height: 39px;
    min-height: 39px;
    max-height: 39px;
  }
  
  :deep(.custom-ant-table .ant-table-tbody > tr) {
    border-bottom: none;
    border-right: none;
    border-left: none;
    border-top: none;
    transition: background-color 0.2s;
    cursor: pointer;
    height: 39px;
    min-height: 39px;
    max-height: 39px;
  }
  
  :deep(.custom-ant-table .ant-table-tbody > tr:hover) {
    background-color: rgba(26, 164, 200, 0.2) !important;
  }
  
  :deep(.custom-ant-table .ant-table-tbody > tr:hover > td) {
    background-color: rgba(26, 164, 200, 0.2) !important;
  }
  
  :deep(.custom-ant-table .ant-table-tbody > tr.ant-table-row-selected) {
    background-color: #e3f2fd;
  }
  
  :deep(.custom-ant-table .ant-table-selection-column) {
    width: 40px;
    padding-left: 16px;
    pointer-events: auto !important;
  }
  
  :deep(.custom-ant-table .ant-checkbox-wrapper) {
    opacity: 1 !important;
    visibility: visible !important;
    display: inline-flex !important;
    align-items: center;
    pointer-events: auto !important;
  }
  
  :deep(.custom-ant-table .ant-checkbox) {
    opacity: 1 !important;
    visibility: visible !important;
    pointer-events: auto !important;
    cursor: pointer;
  }
  
  :deep(.custom-ant-table .ant-checkbox-inner) {
    opacity: 1 !important;
    visibility: visible !important;
    border: 1px solid #d9d9d9;
    width: 16px;
    height: 16px;
    cursor: pointer;
  }
  
  :deep(.custom-ant-table .ant-checkbox-input) {
    opacity: 1 !important;
    pointer-events: auto !important;
    cursor: pointer;
  }
  
  :deep(.custom-ant-table .ant-checkbox-checked .ant-checkbox-inner) {
    background-color: #1aa4c8;
    border-color: #1aa4c8;
  }
  
  :deep(.custom-ant-table .ant-checkbox:hover .ant-checkbox-inner) {
    border-color: #1aa4c8;
  }
  
  :deep(.custom-ant-table .ant-checkbox-input:focus + .ant-checkbox-inner) {
    border-color: #1aa4c8;
    box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
  }
  
  .custom-filter-row {
    display: table-row;
  }
  
  .custom-filter-cell {
    padding: 8px 16px;
    background-color: #ffffff;
    border-bottom: 1px solid #e0e0e0;
    height: 48px;
    vertical-align: middle;
    box-sizing: border-box;
    border-left: none;
    border-top: none;
  }
  
  .m-ant-table-wrapper--bordered .custom-filter-cell:not(:last-child) {
    border-right: 1px solid #e0e0e0;
  }
  
  .custom-filter-checkbox {
    width: 40px;
    min-width: 40px;
    max-width: 40px;
    padding-left: 16px;
    padding-right: 16px;
  }
  
  .filter-input-wrapper {
    display: flex;
    align-items: center;
    gap: 8px;
    position: relative;
    width: 100%;
  }
  
  .filter-input {
    flex: 1;
    padding: 6px 32px 6px 12px;
    border: 1px solid #d9d9d9;
    border-radius: 3px;
    font-size: 13px;
    font-family: 'Roboto', sans-serif;
    color: #001031;
    background-color: #ffffff;
    transition: all 0.2s;
    min-width: 0;
    width: 100%;
    box-sizing: border-box;
  }
  
  .filter-input:focus {
    outline: none;
    border-color: #1aa4c8;
    box-shadow: 0 0 0 2px rgba(26, 164, 200, 0.2);
  }
  
  .filter-input::placeholder {
    color: #999999;
    font-style: italic;
  }
  
  .filter-icon {
    position: absolute;
    right: 8px;
    top: 50%;
    transform: translateY(-50%);
    pointer-events: none;
    flex-shrink: 0;
    width: 16px;
    height: 16px;
  }
  
  .custom-summary-row {
    background-color: #ffffff;
    border: none;
    border-radius: 0 0 3px 3px;
    padding: 0;
    width: 100%;
    box-sizing: border-box;
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    overflow: visible;
  }
  
  .custom-summary-content {
    display: flex;
    align-items: center;
    width: 100%;
    box-sizing: border-box;
    gap: 0;
    flex-wrap: nowrap;
    position: relative;
    overflow: visible;
  }
  
  .custom-summary-pagination {
    display: flex;
    align-items: center;
    justify-content: flex-start;
    padding: 8px 16px;
    background-color: #ffffff;
    min-height: 48px;
    box-sizing: border-box;
    gap: 16px;
    position: absolute;
    left: 0;
    top: 0;
    z-index: 2;
    flex-shrink: 0;
    white-space: nowrap;
    overflow: visible;
  }
  
  .custom-footer-summary {
    white-space: nowrap;
    font-size: 12px;
    font-family: 'Roboto', sans-serif;
    color: #001031;
  }
  
  .custom-footer-summary strong {
    color: #1aa4c8;
    font-weight: 700;
  }
  
  .custom-page-size {
    min-width: 50px !important;
    width: 50px !important;
    max-width: 50px !important;
    position: relative;
    z-index: 1000;
  }
  
  .custom-page-controls {
    display: flex;
    align-items: center;
    gap: 4px;
  }
  
  .custom-page-btn {
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
  
  .custom-page-btn:not(.custom-page-btn--nav) {
    background-color: transparent;
    border-radius: 3px;
    width: 20px;
    height: 20px;
    overflow: hidden;
    min-width: 20px;
    padding: 0;
    border: none;
  }
  
  .custom-page-btn--active {
    background-color: #f5f5f5 !important;
    color: #001031;
    border: none;
  }
  
  .custom-page-btn:not(.custom-page-btn--nav):hover:not(:disabled):not(.custom-page-btn--active) {
    background-color: #f5f5f5;
    border: none;
  }
  
  .custom-page-btn--nav:hover:not(:disabled) {
    background-color: #f5f5f5;
    border: none !important;
  }
  
  .custom-page-btn:disabled {
    opacity: 0.4;
    cursor: not-allowed;
    background-color: #f5f5f5;
  }
  
  .custom-page-btn--nav {
    border: none !important;
    padding: 0;
    min-width: 32px;
  }
  
  .custom-page-btn--nav svg {
    display: block;
    margin: 0 auto;
  }
  
  .custom-summary-table {
    flex: 1;
    border-collapse: collapse;
    table-layout: fixed;
    box-sizing: border-box;
    background-color: #ffffff;
    min-width: 0;
    overflow: hidden;
    width: 100%;
    border-radius: 0 0 3px 3px;
  }
  
  .custom-summary-table tr:last-child td:first-child {
    border-bottom-left-radius: 3px;
  }
  
  .custom-summary-table tr:last-child td:last-child {
    border-bottom-right-radius: 3px;
  }
  
  .custom-summary-checkbox-col {
    width: 40px;
    padding: 16px 16px 12px 16px;
    box-sizing: border-box;
  }
  
  .custom-summary-cell {
    padding: 16px 16px 12px 16px;
    text-align: left;
    font-size: 13px;
    font-family: 'Roboto', sans-serif;
    box-sizing: border-box;
    overflow: hidden;
    white-space: nowrap;
    background-color: #ffffff;
    border-bottom: none;
    border-right: none;
    height: 48px;
  }
  
  .custom-summary-cell.text-right {
    text-align: right;
  }
  
  .custom-summary-cell.text-center {
    text-align: center;
  }
  
  .custom-summary-value {
    font-weight: 700;
    color: #001031;
    display: inline-block;
  }
</style>

