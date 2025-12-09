<template>
  <Teleport to="body">
    <transition name="popup">
      <div v-if="modelValue" class="select-assets-popup" @click.self="handleBackdropClick">
        <div class="select-assets-popup__container">
          <!-- Header -->
          <div class="select-assets-popup__header">
            <h3 class="select-assets-popup__title">Chọn tài sản ghi tăng</h3>
            <button class="select-assets-popup__close" @click="handleClose" title="Đóng">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <use href="#icon-close" />
              </svg>
            </button>
          </div>

          <!-- Body -->
          <div class="select-assets-popup__body">
            <!-- Table Section -->
            <div class="select-assets-popup__table-wrapper" ref="tableContainerRef">
              <div class="custom-table-wrapper">
                <a-table
                  :columns="assetColumns"
                  :data-source="paginatedAssets"
                  :row-selection="rowSelection"
                  :pagination="false"
                  :scroll="{ y: tableScrollHeight }"
                  :bordered="false"
                  :row-key="(record) => record.id || record.assetCode"
                  :loading="loadingAssets"
                  class="custom-ant-table"
                >
                </a-table>
                
                <!-- Filter Row (hidden, will be inserted into thead) -->
                <div style="display: none;">
                  <table>
                    <tbody>
                      <tr class="custom-filter-row" ref="filterRowRef">
                        <td class="custom-filter-cell custom-filter-checkbox"></td>
                        <td 
                          v-for="col in assetColumns" 
                          :key="col.key"
                          :class="[
                            'custom-filter-cell',
                            col.align === 'right' ? 'text-right' : col.align === 'center' ? 'text-center' : 'text-left'
                          ]"
                        >
                          <div v-if="col.key !== 'actions' && col.key !== 'stt'" class="filter-input-wrapper">
                            <span v-if="col.key === 'originalPrice'" class="filter-equals-sign">=</span>
                            <input
                              v-model="filterValues[col.key]"
                              type="text"
                              :placeholder="`Lọc ${col.title?.toLowerCase() || ''}`"
                              class="filter-input"
                              :class="{ 'filter-input--with-equals': col.key === 'originalPrice' }"
                              @input="handleFilterChange"
                            />
                            <svg 
                              v-if="col.key !== 'originalPrice'"
                              width="16" 
                              height="16" 
                              viewBox="0 0 24 24" 
                              fill="none" 
                              xmlns="http://www.w3.org/2000/svg"
                              class="filter-icon"
                            >
                              <use href="#icon-filter-old" />
                            </svg>
                          </div>
                        </td>
                      </tr>
                    </tbody>
                  </table>
                </div>
                
                <!-- Summary Row with Pagination -->
                <div ref="summaryWrapperRef" class="custom-summary-row">
                  <div class="custom-summary-content">
                    <!-- Pagination Controls -->
                    <div class="custom-summary-pagination">
                      <div class="custom-footer-summary">
                        Tổng số: <strong>{{ totalAssetRecords }}</strong> bản ghi
                      </div>
                      
                      <MPageSizeDropdown
                        :model-value="pagination.pageSize"
                        :options="[10, 20, 50, 100, 200]"
                        class="custom-page-size custom-page-size--up"
                        @update:model-value="handlePageSizeChange"
                      />
                      
                      <div class="custom-page-controls">
                        <button
                          class="custom-page-btn custom-page-btn--nav"
                          :disabled="pagination.currentPage === 1"
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
                          :disabled="page === '...'"
                          @click="goToPage(page)"
                        >
                          {{ page === '...' ? '...' : page }}
                        </button>
                        
                        <button
                          class="custom-page-btn custom-page-btn--nav"
                          :disabled="pagination.currentPage >= totalPages"
                          @click="goToPage(pagination.currentPage + 1)"
                        >
                          <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                            <use href="#icon-right-2" />
                          </svg>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Footer -->
          <div class="select-assets-popup__footer">
            <MButton variant="outline" @click="handleClose">Hủy</MButton>
            <MButton variant="main" @click="handleConfirm">Xác nhận</MButton>
          </div>
        </div>
      </div>
    </transition>
  </Teleport>
</template>

<script setup>
import { ref, computed, onMounted, onUpdated, onUnmounted, nextTick, watch } from 'vue'
import MButton from './ui/MButton.vue'
import MPageSizeDropdown from './ui/MPageSizeDropdown.vue'
import { formatCurrency } from '../utils/formatters'
import { useAssetApi } from '../composables/useAssetApi'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  selectedAssetIds: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['update:modelValue', 'close', 'confirm'])

// Composables
const { loadAvailableAssetsForVoucher } = useAssetApi()

// Assets data from API
const allAssets = ref([])
const selectedAssets = ref([])
const tableContainerRef = ref(null)
const tableScrollHeight = ref(300)
const filterRowRef = ref(null)
const summaryWrapperRef = ref(null)
const loadingAssets = ref(false)
const totalAssetRecords = ref(0)

// Filter states
const filterValues = ref({
  assetCode: '',
  assetName: '',
  department: '',
  originalPrice: ''
})

// Debounce timer for filter
let filterDebounceTimer = null

// Pagination
const pagination = ref({
  currentPage: 1,
  pageSize: 20
})

const paginatedAssets = computed(() => allAssets.value)

const totalPages = computed(() => {
  return Math.ceil(totalAssetRecords.value / pagination.value.pageSize)
})

const visiblePages = computed(() => {
  const pages = []
  const total = totalPages.value
  const current = pagination.value.currentPage

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

// Asset table columns
const assetColumns = computed(() => [
  {
    title: 'STT',
    dataIndex: 'stt',
    key: 'stt',
    align: 'center',
    width: 60
  },
  {
    title: 'Mã tài sản',
    dataIndex: 'assetCode',
    key: 'assetCode',
    align: 'left',
    width: 150
  },
  {
    title: 'Tên tài sản',
    dataIndex: 'assetName',
    key: 'assetName',
    align: 'left',
    width: 250
  },
  {
    title: 'Bộ phận sử dụng',
    dataIndex: 'department',
    key: 'department',
    align: 'left',
    width: 200
  },
  {
    title: 'Nguyên giá',
    dataIndex: 'originalPrice',
    key: 'originalPrice',
    align: 'right',
    width: 150,
    customRender: ({ text }) => formatCurrency(text)
  }
])

// Row selection
const rowSelection = computed(() => ({
  selectedRowKeys: selectedAssets.value.map(asset => asset.id || asset.assetCode),
  onChange: (selectedRowKeys, selectedRowsData) => {
    selectedAssets.value = selectedRowsData || []
  },
  onSelectAll: (selected, selectedRowsData, changeRows) => {
    if (selected) {
      const existingKeys = new Set(selectedAssets.value.map(asset => asset.id || asset.assetCode))
      const newRows = changeRows.filter(asset => !existingKeys.has(asset.id || asset.assetCode))
      selectedAssets.value = [...selectedAssets.value, ...newRows]
    } else {
      const changeRowKeys = new Set(changeRows.map(asset => asset.id || asset.assetCode))
      selectedAssets.value = selectedAssets.value.filter(
        asset => !changeRowKeys.has(asset.id || asset.assetCode)
      )
    }
  }
}))

// Initialize selected assets from props
watch(() => props.selectedAssetIds, (newIds) => {
  if (newIds && newIds.length > 0) {
    selectedAssets.value = allAssets.value.filter(asset => 
      newIds.includes(asset.id || asset.assetCode)
    )
  } else {
    selectedAssets.value = []
  }
}, { immediate: true })

// Handlers
const handleClose = () => {
  emit('update:modelValue', false)
  emit('close')
}

const handleBackdropClick = () => {
  // Don't close on backdrop click for this popup
}

const handleConfirm = () => {
  emit('confirm', selectedAssets.value)
  handleClose()
}

/**
 * Load assets from API với filter và pagination
 */
const loadAssets = async () => {
  loadingAssets.value = true
  
  try {
    // Build searchText từ các filter
    let searchText = null
    const searchParts = []
    
    if (filterValues.value.assetCode?.trim()) {
      searchParts.push(filterValues.value.assetCode.trim())
    }
    if (filterValues.value.assetName?.trim()) {
      searchParts.push(filterValues.value.assetName.trim())
    }
    
    if (searchParts.length > 0) {
      searchText = searchParts.join(' ')
    }
    
    // Build filter params
    const filterParams = {
      pageNumber: pagination.value.currentPage,
      pageSize: pagination.value.pageSize,
      searchText: searchText || null,
      departmentCode: filterValues.value.department?.trim() || null,
      typeCode: null // Không filter theo typeCode trong popup này
    }
    
    const result = await loadAvailableAssetsForVoucher({
      pageNumber: pagination.value.currentPage,
      pageSize: pagination.value.pageSize,
      filters: {
        searchText: searchText,
        departmentCode: filterValues.value.department?.trim() || null
      }
    })
    
    allAssets.value = result.assets
    totalAssetRecords.value = result.totalRecords
  } catch (err) {
    console.error('Error loading assets:', err)
    allAssets.value = []
    totalAssetRecords.value = 0
    
    const errorMessage = err.response?.data?.message || err.response?.data?.error || 'Không thể tải danh sách tài sản. Vui lòng thử lại.'
    alert(errorMessage)
  } finally {
    loadingAssets.value = false
  }
}

/**
 * Handle filter change with debounce
 */
const handleFilterChange = () => {
  // Reset về trang 1 khi filter thay đổi
  pagination.value.currentPage = 1
  
  // Debounce filter để tránh gọi API quá nhiều
  if (filterDebounceTimer) {
    clearTimeout(filterDebounceTimer)
  }
  
  filterDebounceTimer = setTimeout(() => {
    loadAssets()
  }, 500) // 500ms debounce
}

const goToPage = (page) => {
  if (page === '...' || page < 1 || page > totalPages.value) return
  pagination.value.currentPage = page
  loadAssets()
}

const handlePageSizeChange = (newSize) => {
  pagination.value.pageSize = newSize
  pagination.value.currentPage = 1
  loadAssets()
}

// Sync functions
const insertFilterRow = () => {
  if (!filterRowRef.value) return
  
  nextTick(() => {
    const antTable = document.querySelector('.select-assets-popup__table-wrapper .custom-ant-table .ant-table')
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
    
    // Sync filter row column widths
    syncFilterRowColumnWidths()
  })
}

const syncFilterRowColumnWidths = () => {
  if (!filterRowRef.value) return
  
  nextTick(() => {
    const antTable = document.querySelector('.select-assets-popup__table-wrapper .custom-ant-table .ant-table')
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
    const container = document.querySelector('.select-assets-popup__table-wrapper')
    if (!container) return
    
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
  })
}

watch(() => [assetColumns.value, paginatedAssets.value], () => {
  nextTick(() => {
    insertFilterRow()
    syncFilterRowColumnWidths()
    calculateTableScrollHeight()
  })
}, { deep: true })

/**
 * Xử lý khi window resize để responsive
 */
const handleResize = () => {
  calculateTableScrollHeight()
  syncFilterRowColumnWidths()
}

watch(() => props.modelValue, (isOpen) => {
  if (isOpen) {
    document.body.style.overflow = 'hidden'
    // Load assets khi popup mở
    loadAssets()
    nextTick(() => {
      insertFilterRow()
      syncFilterRowColumnWidths()
      calculateTableScrollHeight()
      setTimeout(() => {
        insertFilterRow()
        syncFilterRowColumnWidths()
        calculateTableScrollHeight()
      }, 100)
    })
  } else {
    document.body.style.overflow = ''
    // Reset filter và pagination khi đóng popup
    filterValues.value = {
      assetCode: '',
      assetName: '',
      department: '',
      originalPrice: ''
    }
    pagination.value = {
      currentPage: 1,
      pageSize: 20
    }
  }
})

onMounted(() => {
  if (props.modelValue) {
    loadAssets()
    insertFilterRow()
    syncFilterRowColumnWidths()
    calculateTableScrollHeight()
    setTimeout(() => {
      insertFilterRow()
      syncFilterRowColumnWidths()
      calculateTableScrollHeight()
    }, 100)
  }
  
  // Handle window resize for responsive columns
  window.addEventListener('resize', handleResize)
})

onUpdated(() => {
  insertFilterRow()
  syncFilterRowColumnWidths()
  calculateTableScrollHeight()
})

onUnmounted(() => {
  document.body.style.overflow = ''
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.select-assets-popup {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 16px;
}

.select-assets-popup__container {
  background-color: #ffffff;
  border-radius: 4px;
  width: 100%;
  max-width: 1200px;
  max-height: 90vh;
  height: 80vh;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.select-assets-popup__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #e0e0e0;
  flex-shrink: 0;
}

.select-assets-popup__title {
  font-size: 20px;
  font-weight: 700;
  color: #001031;
  font-family: 'Roboto', sans-serif;
  margin: 0;
}

.select-assets-popup__close {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  cursor: pointer;
  color: #666666;
  border-radius: 3px;
  transition: all 0.2s;
}

.select-assets-popup__close:hover {
  background-color: #f5f5f5;
  color: #212121;
}

.select-assets-popup__body {
  flex: 1;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  padding: 24px;
  min-height: 0;
}

.select-assets-popup__table-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 400px;
  flex: 1;
}

.custom-table-wrapper {
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

.select-assets-popup__footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 8px;
  padding: 16px 24px;
  border-top: 1px solid #e0e0e0;
  flex-shrink: 0;
}

/* Ant Design Table Styles */
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
  border-right: 1px solid #e0e0e0;
  white-space: nowrap;
  height: 40px;
}

:deep(.custom-ant-table .ant-table-thead > tr > th:last-child) {
  border-right: none;
}

:deep(.custom-ant-table .ant-table-thead > tr:first-child > th) {
  border-bottom: 1px solid #e0e0e0;
}

:deep(.custom-ant-table .ant-table-tbody > tr > td) {
  padding: 16px 16px;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  color: #001031;
  border-bottom: none;
  border-left: none;
  border-top: none;
  border-right: 1px solid #e0e0e0;
  font-weight: 400;
  height: 39px;
  min-height: 39px;
  max-height: 39px;
}

:deep(.custom-ant-table .ant-table-tbody > tr > td:last-child) {
  border-right: none;
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
}

/* Filter row styles */
.custom-filter-row {
  display: table-row;
}

.custom-filter-cell {
  padding: 8px 16px;
  background-color: #f5f5f5;
  border-bottom: 1px solid #e0e0e0;
  height: 48px;
  vertical-align: middle;
  box-sizing: border-box;
  border-left: none;
  border-top: none;
  border-right: 1px solid #e0e0e0;
}

.custom-filter-cell:last-child {
  border-right: none;
}

:deep(.custom-ant-table .ant-table-thead .custom-filter-row .custom-filter-cell) {
  border-right: 1px solid #e0e0e0;
}

:deep(.custom-ant-table .ant-table-thead .custom-filter-row .custom-filter-cell:last-child) {
  border-right: none;
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

.filter-input--with-equals {
  padding-left: 24px;
}

.filter-equals-sign {
  position: absolute;
  left: 8px;
  color: #666666;
  font-size: 13px;
  font-family: 'Roboto', sans-serif;
  z-index: 1;
  pointer-events: none;
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

/* Summary row styles */
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
  color: #001031;
  font-weight: 700;
}

.custom-page-size {
  min-width: 50px !important;
  width: 50px !important;
  max-width: 50px !important;
  position: relative;
  z-index: 1000;
}

:deep(.custom-page-size .m-dropdown) {
  width: 50px !important;
  min-width: 50px !important;
  max-width: 50px !important;
  position: relative;
  z-index: 1000;
}

:deep(.custom-page-size .m-dropdown__trigger) {
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

:deep(.custom-page-size .m-dropdown__icon) {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
}

:deep(.custom-page-size .m-dropdown__value) {
  flex: 1;
  min-width: 0;
  text-align: left;
}

:deep(.custom-page-size .m-dropdown__menu) {
  border: none;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  z-index: 10000 !important;
  position: absolute !important;
  visibility: visible !important;
  opacity: 1 !important;
  display: block !important;
}

:deep(.custom-page-size .m-dropdown__item) {
  padding: 0 8px;
  font-size: 12px;
  height: 32px;
  line-height: 32px;
}

.custom-page-size--up {
  z-index: 1001;
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

/* Transition */
.popup-enter-active,
.popup-leave-active {
  transition: opacity 0.3s;
}

.popup-enter-from,
.popup-leave-to {
  opacity: 0;
}
</style>

