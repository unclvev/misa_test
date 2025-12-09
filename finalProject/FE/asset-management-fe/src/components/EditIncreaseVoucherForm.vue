<template>
  <div class="edit-voucher-form">
    <!-- Header -->
    <div class="edit-voucher-form__header">
      <h2 class="edit-voucher-form__title">{{ isEditMode ? 'Sửa chứng từ ghi tăng' : 'Thêm chứng từ ghi tăng' }}</h2>
      <button class="edit-voucher-form__close" @click="handleClose" title="Đóng">
        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <use href="#icon-close" />
        </svg>
      </button>
    </div>

    <!-- Form Content -->
    <div class="edit-voucher-form__content">
      <!-- Thông tin chứng từ -->
      <div class="edit-voucher-form__section">
        <h3 class="edit-voucher-form__section-title">Thông tin chứng từ</h3>
        <div class="edit-voucher-form__fields">
          <div class="edit-voucher-form__field">
            <label class="edit-voucher-form__label">
              Mã chứng từ
              <span class="edit-voucher-form__required">*</span>
            </label>
            <MTextField
              v-model="formData.voucherNumber"
              placeholder="Nhập mã chứng từ"
              :disabled="isEditMode"
            />
          </div>
          <div class="edit-voucher-form__field">
            <label class="edit-voucher-form__label">
              Ngày chứng từ
              <span class="edit-voucher-form__required">*</span>
            </label>
            <MDatePicker
              v-model="formData.voucherDate"
              placeholder="Chọn ngày"
            />
          </div>
          <div class="edit-voucher-form__field">
            <label class="edit-voucher-form__label">
              Ngày ghi tăng
              <span class="edit-voucher-form__required">*</span>
            </label>
            <MDatePicker
              v-model="formData.increaseDate"
              placeholder="Chọn ngày"
            />
          </div>
          <div class="edit-voucher-form__field edit-voucher-form__field--wide">
            <label class="edit-voucher-form__label">Ghi chú</label>
            <MTextField
              v-model="formData.content"
              placeholder="Nhập ghi chú"
            />
          </div>
        </div>
      </div>

      <!-- Thông tin tài sản ghi tăng -->
      <div class="edit-voucher-form__section">
        <div class="edit-voucher-form__section-header">
          <h3 class="edit-voucher-form__section-title">Thông tin tài sản ghi tăng</h3>
          <MButton variant="main" @click="handleSelectAssets">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <use href="#icon-plus" />
            </svg>
            Chọn tài sản
          </MButton>
        </div>

        <!-- Select Assets Popup -->
        <SelectAssetsPopup
          v-model="showSelectAssetsPopup"
          :selected-asset-ids="selectedAssetIds"
          @confirm="handleAssetsSelected"
          @close="showSelectAssetsPopup = false"
        />

        <!-- Assets Table -->
        <div class="edit-voucher-form__table-wrapper">
          <div class="custom-table-wrapper">
            <a-table
              :columns="assetColumns"
              :data-source="paginatedAssets"
              :row-selection="rowSelection"
              :pagination="false"
              :scroll="{ y: tableScrollHeight }"
              :bordered="false"
              :row-key="(record) => record.id || record.assetCode || record.stt"
              :loading="loadingAssets"
              class="custom-ant-table"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'actions'">
                  <div class="table-actions">
                    <button 
                      class="table-actions__btn" 
                      @click.stop="handleDeleteAsset(record)" 
                      title="Xóa"
                    >
                      <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                        <use href="#icon-delete" />
                      </svg>
                    </button>
                  </div>
                </template>
              </template>
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
                        <input
                          :value="assetFilterValues[col.key]"
                          @input="handleAssetFilterInput(col.key, $event)"
                          type="text"
                          :placeholder="`Lọc ${col.title?.toLowerCase() || ''}`"
                          class="filter-input"
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
            <div v-if="assetsSummary" class="custom-summary-row">
              <div class="custom-summary-content">
                <!-- Pagination Controls -->
                <div class="custom-summary-pagination">
                  <div class="custom-footer-summary">
                    Tổng số: <strong>{{ totalAssetRecords }}</strong> bản ghi
                  </div>
                  
                  <MPageSizeDropdown
                    :model-value="assetPagination.pageSize"
                    :options="[10, 20, 50, 100, 200]"
                    :disabled="false"
                    class="custom-page-size custom-page-size--up"
                    @update:model-value="handleAssetPageSizeChange"
                  />
                  
                  <div class="custom-page-controls">
                    <button
                      class="custom-page-btn custom-page-btn--nav"
                      :disabled="assetPagination.currentPage === 1"
                      @click="goToAssetPage(assetPagination.currentPage - 1)"
                    >
                      <svg width="20" height="20" viewBox="0 0 24 24" fill="currentColor">
                        <use href="#icon-left-2" />
                      </svg>
                    </button>
                    
                    <button
                      v-for="page in visibleAssetPages"
                      :key="page"
                      :class="[
                        'custom-page-btn',
                        { 'custom-page-btn--active': page === assetPagination.currentPage }
                      ]"
                      :disabled="page === '...'"
                      @click="goToAssetPage(page)"
                    >
                      {{ page === '...' ? '...' : page }}
                    </button>
                    
                    <button
                      class="custom-page-btn custom-page-btn--nav"
                      :disabled="assetPagination.currentPage >= totalAssetPages"
                      @click="goToAssetPage(assetPagination.currentPage + 1)"
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
                      <td v-for="(col, idx) in assetColumns" :key="idx" 
                          :class="[
                            'custom-summary-cell',
                            col.align === 'right' ? 'text-right' : col.align === 'center' ? 'text-center' : 'text-left',
                            col.class
                          ]"
                      >
                        <span v-if="col.key === 'originalPrice'" 
                              class="custom-summary-value">
                          {{ formatCurrency(assetsSummary.totalOriginalPrice) }}
                        </span>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Footer Actions -->
    <div class="edit-voucher-form__footer">
      <MButton variant="outline" @click="handleCancel">Hủy</MButton>
      <MButton v-if="isEditMode" variant="sub" @click="handleDelete">Xóa</MButton>
      <MButton variant="main" @click="handleSave">Lưu</MButton>
    </div>

    <!-- Dialogs -->
    <ConfirmDeleteAssetFromVoucherDialog
      v-model="showDeleteAssetDialog"
      :asset-code="assetToDelete?.assetCode || ''"
      :asset-name="assetToDelete?.assetName || ''"
      @confirm="handleConfirmDeleteAsset"
      @cancel="handleCancelDeleteAsset"
    />
    <ConfirmCancelDialog
      v-model="showCancelDialog"
      @confirm="handleConfirmCancel"
      @cancel="handleCancelCancel"
    />
    <ConfirmSaveDialog
      v-model="showSaveDialog"
      @save="handleConfirmSave"
      @dont-save="handleDontSave"
      @cancel="handleCancelSave"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick, onUpdated, onUnmounted, watch } from 'vue'
import MButton from './ui/MButton.vue'
import MTextField from './ui/MTextField.vue'
import MDatePicker from './ui/MDatePicker.vue'
import MDropdown from './ui/MDropdown.vue'
import MPageSizeDropdown from './ui/MPageSizeDropdown.vue'
import SelectAssetsPopup from './SelectAssetsPopup.vue'
import ConfirmDeleteAssetFromVoucherDialog from './dialogs/ConfirmDeleteAssetFromVoucherDialog.vue'
import ConfirmCancelDialog from './dialogs/ConfirmCancelDialog.vue'
import ConfirmSaveDialog from './dialogs/ConfirmSaveDialog.vue'
import { formatCurrency } from '../utils/formatters'
import { useAssetApi } from '../composables/useAssetApi'
import { useIncreaseVoucherApi } from '../composables/useIncreaseVoucherApi'

const props = defineProps({
  voucher: {
    type: Object,
    default: () => ({})
  }
})

const emit = defineEmits(['close', 'save', 'delete', 'cancel'])

// Composables
const { getAssetById } = useAssetApi()
const { deleteVoucherDetail } = useIncreaseVoucherApi()

// Check if this is edit mode (has id) or add mode
const isEditMode = computed(() => {
  return !!(props.voucher?.id)
})

/**
 * Get current date in YYYY-MM-DD format
 */
const getCurrentDate = () => {
  const today = new Date()
  const year = today.getFullYear()
  const month = String(today.getMonth() + 1).padStart(2, '0')
  const day = String(today.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

// Loading state for assets
const loadingAssets = ref(false)

// Dialog states
const showDeleteAssetDialog = ref(false)
const assetToDelete = ref(null)
const showCancelDialog = ref(false)
const showSaveDialog = ref(false)

// Form data - auto-fill current date for new vouchers
const isNewVoucher = !props.voucher?.id
const formData = ref({
  voucherNumber: props.voucher.voucherNumber || '',
  voucherDate: props.voucher.voucherDate || (isNewVoucher ? getCurrentDate() : ''),
  increaseDate: props.voucher.increaseDate || (isNewVoucher ? getCurrentDate() : ''),
  content: props.voucher.content || ''
})

// Assets data
const assets = ref([])

const selectedAssets = ref([])
const showSelectAssetsPopup = ref(false)

// Track initial form & asset state for cancel confirmation
const initialState = ref({
  formData: null,
  assets: []
})

// Asset filter states
const assetFilterValues = ref({
  assetCode: '',
  assetName: '',
  department: '',
  originalPrice: ''
})

// Asset pagination
const assetPagination = ref({
  currentPage: 1,
  pageSize: 20
})

const normalizeAssets = (assetList) => {
  return (assetList || []).map(asset => ({
    id: asset.id || asset.assetId || null,
    assetCode: asset.assetCode || '',
    originalPrice: asset.originalPrice || 0
  }))
}

const setInitialState = () => {
  initialState.value = {
    formData: JSON.parse(JSON.stringify(formData.value)),
    assets: normalizeAssets(assets.value)
  }
}

const hasFormChanges = () => {
  if (!initialState.value.formData) {
    // Fallback for initial render before data is loaded
    return (
      (formData.value.voucherNumber && formData.value.voucherNumber.trim() !== '') ||
      (formData.value.voucherDate && formData.value.voucherDate !== '') ||
      (formData.value.increaseDate && formData.value.increaseDate !== '') ||
      (formData.value.content && formData.value.content.trim() !== '') ||
      (assets.value && assets.value.length > 0)
    )
  }

  const fieldsToCompare = ['voucherNumber', 'voucherDate', 'increaseDate', 'content']
  for (const field of fieldsToCompare) {
    if ((formData.value[field] || '') !== (initialState.value.formData[field] || '')) {
      return true
    }
  }

  const currentAssets = normalizeAssets(assets.value)
  if (currentAssets.length !== initialState.value.assets.length) {
    return true
  }

  const initialAssetsJSON = JSON.stringify(initialState.value.assets)
  const currentAssetsJSON = JSON.stringify(currentAssets)
  return initialAssetsJSON !== currentAssetsJSON
}

/**
 * Filter assets based on filter values
 */
const filteredAssets = computed(() => {
  if (!assets.value || assets.value.length === 0) {
    return []
  }

  const filters = assetFilterValues.value
  let filtered = [...assets.value]

  // Filter by asset code
  if (filters.assetCode && filters.assetCode.trim() !== '') {
    const searchCode = filters.assetCode.trim().toLowerCase()
    filtered = filtered.filter(asset => {
      const assetCode = (asset.assetCode || '').toLowerCase()
      return assetCode.includes(searchCode)
    })
  }

  // Filter by asset name
  if (filters.assetName && filters.assetName.trim() !== '') {
    const searchName = filters.assetName.trim().toLowerCase()
    filtered = filtered.filter(asset => {
      const assetName = (asset.assetName || '').toLowerCase()
      return assetName.includes(searchName)
    })
  }

  // Filter by department
  if (filters.department && filters.department.trim() !== '') {
    const searchDept = filters.department.trim().toLowerCase()
    filtered = filtered.filter(asset => {
      const department = (asset.department || '').toLowerCase()
      return department.includes(searchDept)
    })
  }

  // Filter by original price
  if (filters.originalPrice && filters.originalPrice.trim() !== '') {
    const searchPrice = filters.originalPrice.trim().toLowerCase().replace(/,/g, '')
    filtered = filtered.filter(asset => {
      const assetPrice = asset.originalPrice ?? 0
      const priceText = String(assetPrice).toLowerCase()
      return priceText.includes(searchPrice)
    })
  }

  return filtered
})

const totalAssetRecords = computed(() => filteredAssets.value.length)

const paginatedAssets = computed(() => {
  const start = (assetPagination.value.currentPage - 1) * assetPagination.value.pageSize
  const end = start + assetPagination.value.pageSize
  const paginated = filteredAssets.value.slice(start, end)
  
  // Update STT based on pagination
  paginated.forEach((asset, index) => {
    asset.stt = start + index + 1
  })
  
  return paginated
})

const assetsSummary = computed(() => {
  if (filteredAssets.value.length === 0) return null
  const total = filteredAssets.value.reduce((sum, a) => sum + (a.originalPrice || 0), 0)
  return {
    totalOriginalPrice: total
  }
})

// Table scroll height
const tableScrollHeight = ref(400)
const summaryTableRef = ref(null)
const summaryCheckboxRef = ref(null)
const filterRowRef = ref(null)

/**
 * Tính tổng số trang dựa trên tổng số bản ghi và số bản ghi mỗi trang
 */
const totalAssetPages = computed(() => {
  return Math.ceil(totalAssetRecords.value / assetPagination.value.pageSize)
})

/**
 * Tính toán danh sách các số trang hiển thị trong pagination
 */
const visibleAssetPages = computed(() => {
  const pages = []
  const total = totalAssetPages.value
  const current = assetPagination.value.currentPage

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

/**
 * Tính toán chiều cao scroll của bảng
 */
const calculateTableScrollHeight = () => {
  nextTick(() => {
    const container = document.querySelector('.edit-voucher-form__table-wrapper')
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

/**
 * Đồng bộ độ rộng các cột trong summary row với các cột trong bảng chính
 */
const syncSummaryColumnWidths = () => {
  if (!summaryTableRef.value) return
  
  nextTick(() => {
    const antTable = document.querySelector('.edit-voucher-form__table-wrapper .custom-ant-table .ant-table')
    if (!antTable) return
    
    const headerCells = antTable.querySelectorAll('.ant-table-thead th')
    const summaryCells = summaryTableRef.value.querySelectorAll('td')
    
    if (headerCells.length === 0 || summaryCells.length === 0) return
    
    // Sync checkbox column
    const headerCheckbox = antTable.querySelector('.ant-table-selection-column')
    if (headerCheckbox && summaryCheckboxRef.value) {
      const width = headerCheckbox.offsetWidth
      if (width > 0) {
        summaryCheckboxRef.value.style.width = width + 'px'
        summaryCheckboxRef.value.style.minWidth = width + 'px'
        summaryCheckboxRef.value.style.maxWidth = width + 'px'
      }
    }
    
    // Sync data columns
    const headerStartIndex = 1
    const summaryStartIndex = 1
    const summaryDataCells = Array.from(summaryCells).slice(summaryStartIndex)
    
    for (let i = 0; i < assetColumns.value.length && i < summaryDataCells.length; i++) {
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
    
    // Sync table width
    const tableWidth = antTable.offsetWidth
    if (tableWidth > 0 && summaryTableRef.value.offsetWidth !== tableWidth) {
      summaryTableRef.value.style.width = tableWidth + 'px'
    }
  })
}

/**
 * Chèn filter row vào thead của Ant Design table
 */
const insertFilterRow = () => {
  if (!filterRowRef.value) return
  
  nextTick(() => {
    const antTable = document.querySelector('.edit-voucher-form__table-wrapper .custom-ant-table .ant-table')
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

/**
 * Đồng bộ độ rộng các cột trong filter row với các cột trong header
 */
const syncFilterRowColumnWidths = () => {
  if (!filterRowRef.value) return
  
  nextTick(() => {
    const antTable = document.querySelector('.edit-voucher-form__table-wrapper .custom-ant-table .ant-table')
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
  },
  {
    title: 'Chức năng',
    key: 'actions',
    align: 'center',
    width: 100,
    fixed: 'right'
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

// Handlers
const handleClose = () => {
  emit('close')
}

const handleCancel = () => {
  if (hasFormChanges()) {
    showSaveDialog.value = true
  } else {
    showCancelDialog.value = true
  }
}

const handleConfirmCancel = () => {
  showCancelDialog.value = false
  emit('cancel')
}

const handleCancelCancel = () => {
  showCancelDialog.value = false
}

const handleConfirmSave = () => {
  showSaveDialog.value = false
  emit('save', {
    ...formData.value,
    assets: assets.value
  })
}

const handleDontSave = () => {
  showSaveDialog.value = false
  emit('cancel')
}

const handleCancelSave = () => {
  showSaveDialog.value = false
}

/**
 * Load assets from voucher details
 * Fetch full asset information for each asset in details
 */
const loadAssetsFromVoucher = async () => {
  if (!props.voucher?.details || props.voucher.details.length === 0) {
    assets.value = []
    return
  }

  loadingAssets.value = true

  try {
    // Fetch full asset details for each asset in voucher details
    const assetPromises = props.voucher.details.map(async (detail) => {
      try {
        const assetData = await getAssetById(detail.assetId)

        return {
          id: assetData.id,
          assetId: detail.assetId,
          detailId: detail.id, // Lưu detailId để xóa
          stt: 0, // Will be set later
          assetCode: assetData.assetCode || detail.assetCode || '',
          assetName: assetData.assetName || detail.assetName || '',
          department: assetData.department?.departmentName || assetData.deptName || '',
          originalPrice: assetData.purchasePrice || 0
        }
      } catch (err) {
        console.error(`Error loading asset ${detail.assetId}:`, err)
        // Return partial data if asset fetch fails
        return {
          id: detail.assetId,
          assetId: detail.assetId,
          detailId: detail.id, // Lưu detailId để xóa
          stt: 0,
          assetCode: detail.assetCode || '',
          assetName: detail.assetName || '',
          department: '',
          originalPrice: 0
        }
      }
    })

    const loadedAssets = await Promise.all(assetPromises)
    
    // STT will be automatically calculated in paginatedAssets computed based on pagination
    assets.value = loadedAssets
  } catch (err) {
    console.error('Error loading assets:', err)
    assets.value = []
  } finally {
    loadingAssets.value = false
  }
}

// Watch for voucher prop changes
watch(() => props.voucher, async (newVoucher) => {
  if (newVoucher) {
    const isNew = !newVoucher.id
    // Update form data - auto-fill current date for new vouchers
    formData.value = {
      voucherNumber: newVoucher.voucherNumber || '',
      voucherDate: newVoucher.voucherDate || (isNew ? getCurrentDate() : ''),
      increaseDate: newVoucher.increaseDate || (isNew ? getCurrentDate() : ''),
      content: newVoucher.content || ''
    }

    // Load assets from voucher details
    await loadAssetsFromVoucher()
    setInitialState()
  }
}, { immediate: true, deep: true })

// Watch for filter changes - reset pagination to page 1
watch(() => assetFilterValues.value, () => {
  assetPagination.value.currentPage = 1
}, { deep: true })

const handleSave = () => {
  emit('save', {
    ...formData.value,
    assets: assets.value
  })
}

const handleDelete = () => {
  emit('delete')
}

const handleSelectAssets = () => {
  showSelectAssetsPopup.value = true
}

const handleAssetsSelected = (selectedAssetsList) => {
  // Add selected assets to the form, avoiding duplicates
  const existingIds = new Set(assets.value.map(a => a.id || a.assetCode))
  
  selectedAssetsList.forEach(asset => {
    const assetId = asset.id || asset.assetCode
    if (!existingIds.has(assetId)) {
      const newAsset = {
        ...asset,
        stt: 0 // Will be calculated in paginatedAssets computed
      }
      assets.value.push(newAsset)
      existingIds.add(assetId)
    }
  })
  
  // STT will be automatically calculated in paginatedAssets computed based on pagination
}

const selectedAssetIds = computed(() => {
  return assets.value.map(a => a.id || a.assetCode)
})

const handleDeleteAsset = (asset) => {
  // Lưu tài sản cần xóa và hiển thị dialog
  assetToDelete.value = asset
  showDeleteAssetDialog.value = true
}

const handleConfirmDeleteAsset = async () => {
  if (!assetToDelete.value) {
    showDeleteAssetDialog.value = false
    return
  }

  const asset = assetToDelete.value
  const detailId = asset.detailId
  const voucherId = props.voucher?.id

  // Nếu có detailId và voucherId (đang ở chế độ edit), gọi composable xóa
  if (detailId && voucherId) {
    try {
      await deleteVoucherDetail(voucherId, detailId)
      
      // Xóa khỏi danh sách local
      const assetId = asset.id || asset.assetId
      const index = assets.value.findIndex(a => (a.id === assetId) || (a.assetId === assetId))
      if (index > -1) {
        assets.value.splice(index, 1)
        // STT will be automatically calculated in paginatedAssets computed
      }
      
      showDeleteAssetDialog.value = false
      assetToDelete.value = null
    } catch (err) {
      console.error('Error deleting voucher detail:', err)
      showDeleteAssetDialog.value = false
      const errorMessage = err.response?.data?.message || err.response?.data?.error || 'Không thể xóa tài sản. Vui lòng thử lại.'
      alert(errorMessage)
      assetToDelete.value = null
    }
  } else {
    // Nếu không có detailId (đang ở chế độ add), chỉ xóa khỏi danh sách local
    const assetId = asset.id || asset.assetId
    const index = assets.value.findIndex(a => (a.id === assetId) || (a.assetId === assetId))
    if (index > -1) {
      assets.value.splice(index, 1)
      // STT will be automatically calculated in paginatedAssets computed
    }
    
    showDeleteAssetDialog.value = false
    assetToDelete.value = null
  }
}

const handleCancelDeleteAsset = () => {
  showDeleteAssetDialog.value = false
  assetToDelete.value = null
}

/**
 * Xử lý input filter
 */
const handleAssetFilterInput = (key, event) => {
  assetFilterValues.value = {
    ...assetFilterValues.value,
    [key]: event.target.value
  }
  handleAssetFilterChange(assetFilterValues.value)
}

/**
 * Filter handler - reset to page 1 when filter changes
 */
const handleAssetFilterChange = (newFilterValues) => {
  // Reset to first page when filter changes
  assetPagination.value.currentPage = 1
}

/**
 * Chuyển đến trang cụ thể trong pagination
 */
const goToAssetPage = (page) => {
  if (page === '...' || page < 1 || page === assetPagination.value.currentPage) {
    return
  }
  assetPagination.value.currentPage = page
}

/**
 * Xử lý thay đổi số lượng bản ghi hiển thị trên mỗi trang
 */
const handleAssetPageSizeChange = (newSize) => {
  assetPagination.value.pageSize = newSize
  assetPagination.value.currentPage = 1
}


/**
 * Xử lý khi window resize để responsive
 */
const handleResize = () => {
  calculateTableScrollHeight()
  syncSummaryColumnWidths()
  syncFilterRowColumnWidths()
}

onMounted(() => {
  // Insert filter row and calculate table height
  insertFilterRow()
  calculateTableScrollHeight()
  syncSummaryColumnWidths()
  syncFilterRowColumnWidths()
  setTimeout(() => {
    insertFilterRow()
    calculateTableScrollHeight()
    syncSummaryColumnWidths()
    syncFilterRowColumnWidths()
  }, 100)
  
  // Handle window resize for responsive columns
  window.addEventListener('resize', handleResize)
})

onUpdated(() => {
  insertFilterRow()
  calculateTableScrollHeight()
  syncSummaryColumnWidths()
  syncFilterRowColumnWidths()
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})

</script>

<style scoped>
.edit-voucher-form {
  background-color: #ffffff;
  border-radius: 4px;
  display: flex;
  flex-direction: column;
  height: 100%;
  overflow: hidden;
}

.edit-voucher-form__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 24px;
  border-bottom: 1px solid #e0e0e0;
}

.edit-voucher-form__title {
  font-size: 20px;
  font-weight: 700;
  color: #001031;
  font-family: 'Roboto', sans-serif;
  margin: 0;
}

.edit-voucher-form__close {
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

.edit-voucher-form__close:hover {
  background-color: #f5f5f5;
  color: #212121;
}

.edit-voucher-form__content {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.edit-voucher-form__section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.edit-voucher-form__section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.edit-voucher-form__section-title {
  font-size: 16px;
  font-weight: 700;
  color: #001031;
  font-family: 'Roboto', sans-serif;
  margin: 0;
}

.edit-voucher-form__section-title + .m-button {
  display: flex;
  align-items: center;
  gap: 4px;
}

.edit-voucher-form__fields {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
}

.edit-voucher-form__field--wide {
  grid-column: span 4;
}

.edit-voucher-form__field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.edit-voucher-form__label {
  font-size: 13px;
  font-weight: 500;
  color: #001031;
  font-family: 'Roboto', sans-serif;
}

.edit-voucher-form__required {
  color: #f44336;
  margin-left: 4px;
}

.edit-voucher-form__table-wrapper {
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
  background-color: rgb(245, 245, 245) !important;
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
  height: 48px;
}

.custom-summary-cell:last-child {
  border-right: none;
}

.custom-summary-cell.text-right {
  text-align: right;
}

.custom-summary-cell.text-center {
  text-align: center;
}

/* Filter row styles */
.custom-filter-row {
  display: table-row;
}

.custom-filter-cell {
  padding: 8px 16px;
  background-color: rgb(245, 245, 245) !important;
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

.table-actions {
  display: flex;
  gap: 0;
  justify-content: center;
  align-items: center;
  width: 100%;
}

.table-actions__btn {
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  background-color: transparent;
  color: #666666;
  cursor: pointer;
  border-radius: 3px;
  transition: all 0.2s;
  padding: 0;
  opacity: 1;
  visibility: visible;
}

.table-actions__btn:hover {
  background-color: #f5f5f5;
  color: #212121;
}

.table-actions__divider {
  width: 1px;
  height: 16px;
  background-color: #000000;
  margin: 0 2px;
  opacity: 1;
  visibility: visible;
}

.edit-voucher-form__footer {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 8px;
  padding: 16px 24px;
  border-top: 1px solid #e0e0e0;
}

.custom-summary-value {
  font-weight: 700;
  color: #001031;
  display: inline-block;
}
</style>

