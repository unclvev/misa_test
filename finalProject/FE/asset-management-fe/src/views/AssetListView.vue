<template>
  <div class="asset-list-view">
    <!-- Filters Section -->
    <div class="asset-list-view__filters">
      <div class="asset-list-view__search">
        <MSearch 
          v-model="filters.searchQuery" 
          placeholder="Tìm kiếm tài sản" 
          @input="handleSearch" 
        />
      </div>
      <div class="asset-list-view__filter">
        <MDropdown 
          v-model="filters.assetType" 
          :options="assetTypeOptions" 
          placeholder="Loại tài sản"
          icon="icon-filter-old"
          show-all-option
          all-option-label="Tất cả"
        />
      </div>
      <div class="asset-list-view__filter">
        <MDropdown 
          v-model="filters.department" 
          :options="departmentOptions" 
          placeholder="Bộ phận sử dụng"
          icon="icon-filter-old"
          show-all-option
          all-option-label="Tất cả"
        />
      </div>
      <div class="asset-list-view__actions">
        <button 
          class="asset-list-view__action-btn asset-list-view__action-btn--add" 
          @click="handleAddAsset"
          @mouseenter="isAddButtonHovered = true"
          @mouseleave="isAddButtonHovered = false"
        >
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <use :href="isAddButtonHovered ? '#icon-plus' : '#icon-plus-2'" />
          </svg>
          Thêm tài sản
        </button>
        <button class="asset-list-view__action-btn" title="Xuất Excel">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <use href="#icon-excel" />
          </svg>
        </button>
        <button 
          class="asset-list-view__action-btn asset-list-view__action-btn--delete" 
          :class="{ 'asset-list-view__action-btn--disabled': selectedRows.length === 0 }"
          :disabled="selectedRows.length === 0"
          title="Xóa nhiều"
          @click="handleBulkDelete"
        >
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <use href="#icon-delete" />
          </svg>
        </button>
      </div>
    </div>

    <!-- Table Section -->
    <div class="asset-list-view__table">
      <div class="custom-table-wrapper">
        <div v-if="loading" class="loading-overlay">
          <div class="loading-spinner">Đang tải...</div>
        </div>
        <div v-if="error && !loading" class="error-message">
          <strong>Lỗi:</strong> {{ error }}
          <button class="error-retry-btn" @click="loadAssetsData">Thử lại</button>
        </div>
        <a-table
          :columns="antdColumns"
          :data-source="paginatedData"
          :row-selection="rowSelection"
          :pagination="false"
          :scroll="{ y: tableScrollHeight }"
          :bordered="false"
          :row-key="(record) => record.id || record.assetCode || record.stt"
          :loading="loading"
          class="custom-ant-table"
          @row="(record) => ({ onClick: () => handleRowClick(record) })"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'actions'">
              <div class="table-actions">
                <button 
                  class="table-actions__btn" 
                  @click.stop="handleEdit(record)" 
                  title="Sửa"
                >
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                    <use href="#icon-edit" />
                  </svg>
                </button>
                <button 
                  class="table-actions__btn" 
                  @click.stop="handleDuplicate(record)" 
                  title="Nhân bản"
                >
                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none">
                    <use href="#icon-delete-table" />
                  </svg>
                </button>
              </div>
            </template>
          </template>
        </a-table>
        
        <!-- Summary Row with Pagination -->
        <div v-if="assetSummary" class="custom-summary-row">
          <div class="custom-summary-content">
            <!-- Pagination Controls -->
            <div class="custom-summary-pagination">
              <div class="custom-footer-summary">
                Tổng số: <strong>{{ totalRecords }}</strong> bản ghi
              </div>
              
              <MPageSizeDropdown
                :model-value="pagination.pageSize"
                :options="[10, 20, 50, 100, 200]"
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
                  <td v-for="(col, idx) in antdColumns" :key="idx" 
                      :class="[
                        'custom-summary-cell',
                        col.align === 'right' ? 'text-right' : col.align === 'center' ? 'text-center' : 'text-left',
                        col.class
                      ]"
                  >
                    <span v-if="col.key === 'quantity' || col.key === 'originalPrice' || col.key === 'depreciation' || col.key === 'remainingValue'" 
                          class="custom-summary-value">
                      {{ formatCurrency(assetSummary[col.key]) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>

    <!-- Edit Popup -->
    <MPopup 
      v-model="showEditPopup" 
      :title="popupTitle" 
      :close-on-backdrop="false"
      @save="handleSaveAsset" 
      @close="handleClosePopup"
    >
      <AssetForm 
        ref="assetFormRef"
        v-model="editingAsset" 
        :department-options="departmentCodeOptionsForForm"
        :asset-type-options="assetTypeCodeOptionsForForm"
      />
    </MPopup>

    <!-- Delete Dialogs -->
    <ConfirmDeleteDialog
      v-model="showDeleteDialog"
      :asset-code="deleteAssetCode"
      :asset-name="deleteAssetName"
      @confirm="handleConfirmDelete"
      @cancel="handleCancelDelete"
    />

    <ConfirmMultiDeleteDialog
      v-model="showMultiDeleteDialog"
      :count="selectedRows.length"
      @confirm="handleConfirmMultiDelete"
      @cancel="handleCancelMultiDelete"
    />

    <!-- Cancel and Save Dialogs -->
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

    <!-- Warning Dialogs -->
    <WarningCannotDeleteDialog
      v-model="showWarningCannotDeleteDialog"
      :message="warningMessage"
      @close="handleCloseWarningDialog"
    />

    <WarningMultiCannotDeleteDialog
      v-model="showWarningMultiCannotDeleteDialog"
      :count="warningCount"
      @confirm="handleCloseWarningMultiDialog"
    />

    <PartialDeleteResultDialog
      v-model="showPartialDeleteResultDialog"
      :deleted-count="partialDeleteResult.deletedCount"
      :total-count="partialDeleteResult.totalCount"
      :cannot-delete-count="partialDeleteResult.cannotDeleteCount"
      @confirm="handleClosePartialDeleteResultDialog"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted, nextTick, onUpdated, watch } from 'vue'
import MSearch from '../components/ui/MSearch.vue'
import MDropdown from '../components/ui/MDropdown.vue'
import MPageSizeDropdown from '../components/ui/MPageSizeDropdown.vue'
import MPopup from '../components/ui/MPopup.vue'
import AssetForm from '../components/AssetForm.vue'
import { useAssetApi } from '../composables/useAssetApi'
import { useAssetList } from '../composables/useAssetList'
import { formatCurrency } from '../utils/formatters'
import { useToast } from '../composables/useToast'
import ConfirmDeleteDialog from '../components/dialogs/ConfirmDeleteDialog.vue'
import ConfirmMultiDeleteDialog from '../components/dialogs/ConfirmMultiDeleteDialog.vue'
import ConfirmCancelDialog from '../components/dialogs/ConfirmCancelDialog.vue'
import ConfirmSaveDialog from '../components/dialogs/ConfirmSaveDialog.vue'
import WarningCannotDeleteDialog from '../components/dialogs/WarningCannotDeleteDialog.vue'
import WarningMultiCannotDeleteDialog from '../components/dialogs/WarningMultiCannotDeleteDialog.vue'
import PartialDeleteResultDialog from '../components/dialogs/PartialDeleteResultDialog.vue'

// Composables
// API composable - chỉ chứa logic gọi API
const {
  loading,
  error,
  loadAssets: loadAssetsFromApi,
  refreshData: refreshDataFromApi,
  getAssetById,
  getNextAssetCode,
  getDepartments,
  getAssetTypes,
  createAsset,
  updateAsset,
  deleteAsset,
  bulkDeleteAssets
} = useAssetApi()

// Toast composable
const { showSuccess } = useToast()

// UI composable - chứa logic UI (pagination, filtering, selection)
const {
  assets,
  filters,
  pagination,
  selectedRows,
  paginatedData,
  assetSummary,
  handleSearch: handleSearchUI,
  goToPage: goToPageUI,
  handlePageSizeChange: handlePageSizeChangeUI,
  setAssets,
  setSelectedRows
} = useAssetList()

// Computed - Total records (cần lưu từ API response)
const totalRecords = ref(0)

// Local state
const showEditPopup = ref(false)
const editingAsset = ref({})
const isEditMode = ref(false)
const assetFormRef = ref(null)
const departmentOptions = ref([])
const assetTypeOptions = ref([])
const departmentsData = ref([])
const assetTypesData = ref([])
const isAddButtonHovered = ref(false)
const showDeleteDialog = ref(false)
const showMultiDeleteDialog = ref(false)
const showWarningCannotDeleteDialog = ref(false)
const showWarningMultiCannotDeleteDialog = ref(false)
const showPartialDeleteResultDialog = ref(false)
const warningMessage = ref('')
const warningCount = ref(0)
const partialDeleteResult = ref({
  deletedCount: 0,
  totalCount: 0,
  cannotDeleteCount: 0
})
const deleteAssetCode = ref('')
const deleteAssetName = ref('')
const assetToDelete = ref(null)
const showCancelDialog = ref(false)
const showSaveDialog = ref(false)
const originalAsset = ref(null)

// ==================== COMPUTED PROPERTIES ====================

/**
 * Tiêu đề popup form (thay đổi theo chế độ edit/add)
 */
const popupTitle = computed(() => isEditMode.value ? 'Sửa tài sản' : 'Thêm tài sản')

/**
 * Tạo danh sách options cho dropdown mã bộ phận sử dụng trong form
 * Format: { label: departmentSymbol hoặc departmentCode, value: departmentCode, name: departmentName }
 */
const departmentCodeOptionsForForm = computed(() => {
  return departmentsData.value.map(dept => ({
    label: dept.departmentSymbol || dept.departmentCode,
    value: dept.departmentCode,
    name: dept.departmentName
  }))
})

/**
 * Tạo danh sách options cho dropdown mã loại tài sản trong form
 * Format: { label: typeSymbol hoặc typeCode, value: typeCode, name: typeName }
 */
const assetTypeCodeOptionsForForm = computed(() => {
  return assetTypesData.value.map(type => ({
    label: type.typeSymbol || type.typeCode.toString(),
    value: type.typeCode,
    name: type.typeName
  }))
})

/**
 * Tính tổng số trang dựa trên tổng số bản ghi và số bản ghi mỗi trang
 */
const totalPages = computed(() => {
  return Math.ceil(totalRecords.value / pagination.value.pageSize)
})

/**
 * Tính toán danh sách các số trang hiển thị trong pagination
 * Logic:
 * - Nếu tổng số trang <= 7: hiển thị tất cả
 * - Nếu trang hiện tại <= 3: hiển thị 1-4, ..., tổng
 * - Nếu trang hiện tại >= tổng - 2: hiển thị 1, ..., tổng-3 đến tổng
 * - Còn lại: hiển thị 1, ..., trang hiện tại-1, trang hiện tại, trang hiện tại+1, ..., tổng
 */
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

// Table scroll height
const tableScrollHeight = ref(400)
const summaryTableRef = ref(null)
const summaryCheckboxRef = ref(null)

/**
 * Tính toán chiều cao scroll của bảng
 * Dựa trên chiều cao container, header, và summary row
 * Đảm bảo bảng có thể scroll khi nội dung vượt quá chiều cao khả dụng
 */
const calculateTableScrollHeight = () => {
  nextTick(() => {
    const container = document.querySelector('.asset-list-view__table')
    if (!container) return
    
    const thead = container.querySelector('.ant-table-thead')
    const summaryHeight = container.querySelector('.custom-summary-row')?.offsetHeight || 0
    const containerHeight = container.offsetHeight
    const headerHeight = thead?.offsetHeight || 40
    const availableHeight = containerHeight - headerHeight - summaryHeight - 2
    tableScrollHeight.value = Math.max(200, availableHeight)
  })
}

/**
 * Đồng bộ độ rộng các cột trong summary row với các cột trong bảng chính
 * Đảm bảo các cột thẳng hàng với nhau
 * Đồng bộ cả checkbox column và các data columns
 */
const syncSummaryColumnWidths = () => {
  if (!summaryTableRef.value) return
  
  nextTick(() => {
    const antTable = document.querySelector('.asset-list-view__table .custom-ant-table .ant-table')
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
    
    for (let i = 0; i < antdColumns.value.length && i < summaryDataCells.length; i++) {
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

// ==================== RESPONSIVE ====================

/**
 * Theo dõi chiều rộng window để responsive
 */
const windowWidth = ref(window.innerWidth)

/**
 * Kiểm tra màn hình nhỏ (< 1400px)
 */
const isSmallScreen = computed(() => windowWidth.value < 1400)

/**
 * Kiểm tra màn hình trung bình (1400px - 1600px)
 */
const isMediumScreen = computed(() => windowWidth.value >= 1400 && windowWidth.value < 1600)

/**
 * Kiểm tra màn hình lớn (>= 1600px)
 */
const isLargeScreen = computed(() => windowWidth.value >= 1600)

/**
 * Định nghĩa các cột cho Ant Design Table với độ rộng responsive
 * Mỗi cột có 3 độ rộng: small, medium, large tùy theo kích thước màn hình
 * Bao gồm: STT, Mã tài sản, Tên tài sản, Loại tài sản, Bộ phận sử dụng,
 * Số lượng, Nguyên giá, HM/KH lũy kế, Giá trị còn lại, Chức năng
 */
const antdColumns = computed(() => {
  // Calculate responsive widths based on screen size
  const getWidth = (small, medium, large) => {
    if (isSmallScreen.value) return small
    if (isMediumScreen.value) return medium
    return large
  }

  return [
    {
      title: 'STT',
      dataIndex: 'stt',
      key: 'stt',
      align: 'center',
      width: getWidth(50, 55, 60)
    },
    {
      title: 'Mã tài sản',
      dataIndex: 'assetCode',
      key: 'assetCode',
      align: 'left',
      width: getWidth(110, 125, 140)
    },
    {
      title: 'Tên tài sản',
      dataIndex: 'assetName',
      key: 'assetName',
      align: 'left',
      width: getWidth(150, 175, 220)
    },
    {
      title: 'Loại tài sản',
      dataIndex: 'assetType',
      key: 'assetType',
      align: 'left',
      width: getWidth(120, 140, 160)
    },
    {
      title: 'Bộ phận sử dụng',
      dataIndex: 'department',
      key: 'department',
      align: 'left',
      width: getWidth(150, 175, 200)
    },
    {
      title: 'Số lượng',
      dataIndex: 'quantity',
      key: 'quantity',
      align: 'right',
      width: getWidth(80, 85, 90),
      customRender: ({ text }) => formatCurrency(text)
    },
    {
      title: 'Nguyên giá',
      dataIndex: 'originalPrice',
      key: 'originalPrice',
      align: 'right',
      width: getWidth(100, 110, 120),
      customRender: ({ text }) => formatCurrency(text)
    },
    {
      title: 'HM/KH lũy kế',
      dataIndex: 'depreciation',
      key: 'depreciation',
      align: 'right',
      width: getWidth(100, 110, 120),
      customRender: ({ text }) => formatCurrency(text)
    },
    {
      title: 'Giá trị còn lại',
      dataIndex: 'remainingValue',
      key: 'remainingValue',
      align: 'right',
      width: getWidth(110, 120, 130),
      customRender: ({ text }) => formatCurrency(text)
    },
    {
      title: 'Chức năng',
      key: 'actions',
      align: 'center',
      width: getWidth(80, 90, 100),
      fixed: 'right'
    }
  ]
})

/**
 * Cấu hình row selection cho Ant Design Table
 * NOTE: Logic indeterminate (ô vuông khi chọn 1,2 item) đã bị disable bằng CSS
 * Giờ checkbox header sẽ luôn hiển thị như checkbox bình thường (dấu tích) khi có item được chọn
 * 
 * - onChange: xử lý khi chọn/bỏ chọn một dòng
 * - onSelectAll: xử lý khi chọn/bỏ chọn tất cả, merge với selection hiện tại để tránh duplicate
 */
const rowSelection = computed(() => ({
  selectedRowKeys: selectedRows.value.map(row => row.id || row.assetCode || row.stt),
  onChange: (selectedRowKeys, selectedRowsData) => {
    setSelectedRows(selectedRowsData || [])
  },
  onSelectAll: (selected, selectedRowsData, changeRows) => {
    if (selected) {
      // Merge với selectedRows hiện tại, tránh duplicate
      const existingKeys = new Set(selectedRows.value.map(row => row.id || row.assetCode || row.stt))
      const newRows = changeRows.filter(row => !existingKeys.has(row.id || row.assetCode || row.stt))
      setSelectedRows([...selectedRows.value, ...newRows])
    } else {
      // Remove các rows đã bỏ chọn
      const changeRowKeys = new Set(changeRows.map(row => row.id || row.assetCode || row.stt))
      setSelectedRows(
        selectedRows.value.filter(
          row => !changeRowKeys.has(row.id || row.assetCode || row.stt)
        )
      )
    }
  }
}))


/**
 * Xử lý tìm kiếm tài sản
 * Reset về trang 1 và tải lại dữ liệu với từ khóa tìm kiếm mới
 */
const handleSearch = async () => {
  handleSearchUI() // Reset page to 1
  await loadAssetsData()
}

/**
 * Tải dữ liệu danh sách tài sản từ API
 * Kết hợp thông tin pagination và filters để gọi API
 * Cập nhật danh sách tài sản, tổng số bản ghi và thông tin pagination
 */
const loadAssetsData = async () => {
  const result = await loadAssetsFromApi({
    currentPage: pagination.value.currentPage,
    pageSize: pagination.value.pageSize,
    filters: filters.value
  })
  
  setAssets(result.assets)
  totalRecords.value = result.totalRecords
  pagination.value.currentPage = result.pageNumber
  pagination.value.pageSize = result.pageSize
}

/**
 * Chuyển đến trang cụ thể trong pagination
 * @param {number|string} page - Số trang cần chuyển đến (hoặc '...' nếu là placeholder)
 * Bỏ qua nếu page là '...', < 1, hoặc trùng với trang hiện tại
 */
const goToPage = async (page) => {
  if (page === '...' || page < 1 || page === pagination.value.currentPage) {
    return
  }
  goToPageUI(page)
  await loadAssetsData()
}

/**
 * Xử lý thay đổi số lượng bản ghi hiển thị trên mỗi trang
 * @param {number} value - Số lượng bản ghi mới (10, 20, 50, 100, 200)
 * Cập nhật pageSize và tải lại dữ liệu
 */
const handlePageSizeChange = async (value) => {
  handlePageSizeChangeUI(value)
  await loadAssetsData()
}

/**
 * Làm mới dữ liệu danh sách tài sản
 * Tải lại dữ liệu với các filter và pagination hiện tại
 */
const refreshData = async () => {
  await loadAssetsData()
}

/**
 * Xử lý khi click vào một dòng trong bảng
 * Hiện tại chưa có chức năng cụ thể
 */
const handleRowClick = () => {
  // Row click handler
}

/**
 * Xử lý mở form sửa tài sản
 * @param {Object} row - Đối tượng tài sản cần sửa từ bảng
 * Load đầy đủ thông tin tài sản từ API, format dữ liệu cho form,
 * lưu bản gốc để so sánh thay đổi, và mở popup edit
 */
const handleEdit = async (row) => {
  try {
    // Load full asset data from composable
    const assetData = await getAssetById(row.id)
    if (!assetData) {
      alert('Không tìm thấy tài sản')
      return
    }
    
    // Format date từ DateOnly/string sang format cho date picker (YYYY-MM-DD)
    const formatDate = (dateValue) => {
      if (!dateValue) return ''
      if (typeof dateValue === 'string') {
        // Nếu là string, lấy phần date (bỏ time nếu có)
        return dateValue.split('T')[0]
      }
      // Nếu là Date object
      if (dateValue instanceof Date) {
        return dateValue.toISOString().split('T')[0]
      }
      return dateValue
    }
    
    // Map API data to frontend format
    const assetFormData = {
      id: assetData.id,
      assetCode: assetData.assetCode || '',
      assetName: assetData.assetName || '',
      departmentCode: assetData.deptCode || '',
      departmentName: assetData.deptName || '',
      assetTypeCode: assetData.typeCode || 0,
      assetTypeName: assetData.typeName || '',
      quantity: assetData.quantity || 1,
      originalPrice: assetData.purchasePrice || 0,
      purchasePrice: assetData.purchasePrice || 0,
      purchaseDate: formatDate(assetData.purchaseDate),
      startUsageDate: formatDate(assetData.purchaseDate),
      trackingYear: assetData.startTrackingYear || null,
      yearsOfUse: assetData.yearsOfUse || 0,
      depreciationRate: assetData.depreciationRate || 0,
      // Xử lý annualDepreciationValue: mặc định là 0 nếu null/undefined
      annualDepreciationValue: (assetData.annualDepreciationValue !== null && assetData.annualDepreciationValue !== undefined) 
        ? assetData.annualDepreciationValue 
        : 0
    }
    
    editingAsset.value = assetFormData
    // Lưu bản gốc để so sánh thay đổi
    originalAsset.value = JSON.parse(JSON.stringify(assetFormData))
    isEditMode.value = true
    showEditPopup.value = true
    // Reset validation state
    nextTick(() => {
      if (assetFormRef.value) {
        assetFormRef.value.resetValidation()
      }
    })
  } catch (err) {
    console.error('Error loading asset:', err)
    alert(err.response?.data?.message || 'Không thể tải thông tin tài sản. Vui lòng thử lại.')
  }
}

/**
 * Xử lý mở form thêm tài sản mới
 * Lấy mã tài sản tiếp theo từ API, khởi tạo form với giá trị mặc định
 * (ngày mua = ngày hôm nay, số lượng = 1, các giá trị khác = 0)
 * Nếu lỗi khi lấy mã tài sản, vẫn mở form nhưng không có mã
 */
const handleAddAsset = async () => {
  try {
    // Lấy mã tài sản tiếp theo từ composable
    const nextAssetCode = await getNextAssetCode()
    
    // Lấy ngày hôm nay ở format YYYY-MM-DD
    const today = new Date()
    const todayStr = today.toISOString().split('T')[0]
    
    editingAsset.value = {
      assetCode: nextAssetCode,
      quantity: 1,
      originalPrice: 0,
      depreciationRate: 0,
      yearsOfUse: 0,
      annualDepreciationValue: 0,
      purchaseDate: todayStr,
      startUsageDate: todayStr
    }
    // Reset originalAsset khi thêm mới
    originalAsset.value = null
    isEditMode.value = false
    showEditPopup.value = true
    // Reset validation state
    nextTick(() => {
      if (assetFormRef.value) {
        assetFormRef.value.resetValidation()
      }
    })
  } catch (err) {
    console.error('Error getting next asset code:', err)
    // Lấy ngày hôm nay ở format YYYY-MM-DD
    const today = new Date()
    const todayStr = today.toISOString().split('T')[0]
    
    editingAsset.value = {
      quantity: 1,
      originalPrice: 0,
      depreciationRate: 0,
      yearsOfUse: 0,
      annualDepreciationValue: 0,
      purchaseDate: todayStr,
      startUsageDate: todayStr
    }
    // Reset originalAsset khi thêm mới
    originalAsset.value = null
    isEditMode.value = false
    showEditPopup.value = true
    // Reset validation state
    nextTick(() => {
      if (assetFormRef.value) {
        assetFormRef.value.resetValidation()
      }
    })
  }
}

/**
 * Xử lý nhân bản (duplicate) tài sản
 * @param {Object} row - Đối tượng tài sản cần nhân bản từ bảng
 * Load đầy đủ thông tin tài sản, tạo mã tài sản mới,
 * copy tất cả thông tin (trừ mã tài sản), lưu bản gốc để so sánh thay đổi,
 * và mở popup với chế độ thêm mới
 */
const handleDuplicate = async (row) => {
  try {
    // Load full asset data from composable
    const assetData = await getAssetById(row.id)
    if (!assetData) {
      alert('Không tìm thấy tài sản')
      return
    }
    
    // Lấy mã tài sản tiếp theo
    const nextAssetCode = await getNextAssetCode()
    
    // Format date từ DateOnly/string sang format cho date picker (YYYY-MM-DD)
    const formatDate = (dateValue) => {
      if (!dateValue) return ''
      if (typeof dateValue === 'string') {
        return dateValue.split('T')[0]
      }
      if (dateValue instanceof Date) {
        return dateValue.toISOString().split('T')[0]
      }
      return dateValue
    }
    
    // Đảm bảo typeCode không phải 0
    if (!assetData.typeCode || assetData.typeCode === 0) {
      alert('Không thể nhân bản: Mã loại tài sản không hợp lệ')
      return
    }
    
    // Đảm bảo deptCode không rỗng
    if (!assetData.deptCode || assetData.deptCode.trim() === '') {
      alert('Không thể nhân bản: Mã bộ phận sử dụng không hợp lệ')
      return
    }
    
    const duplicatedAsset = {
      assetCode: nextAssetCode,
      assetName: assetData.assetName || '',
      departmentCode: assetData.deptCode || '',
      departmentName: assetData.deptName || '',
      assetTypeCode: assetData.typeCode,
      assetTypeName: assetData.typeName || '',
      quantity: assetData.quantity || 1,
      originalPrice: assetData.purchasePrice || 0,
      purchasePrice: assetData.purchasePrice || 0,
      purchaseDate: formatDate(assetData.purchaseDate),
      startUsageDate: formatDate(assetData.purchaseDate),
      trackingYear: assetData.startTrackingYear || null,
      yearsOfUse: assetData.yearsOfUse || 0,
      depreciationRate: assetData.depreciationRate || 0,
      annualDepreciationValue: assetData.annualDepreciationValue || 0
    }
    
    editingAsset.value = duplicatedAsset
    // Lưu bản gốc để so sánh thay đổi (không tính autofill)
    originalAsset.value = JSON.parse(JSON.stringify(duplicatedAsset))
    isEditMode.value = false
    showEditPopup.value = true
    // Reset validation state
    nextTick(() => {
      if (assetFormRef.value) {
        assetFormRef.value.resetValidation()
      }
    })
  } catch (err) {
    console.error('Error duplicating asset:', err)
    alert(err.response?.data?.message || 'Không thể nhân bản tài sản. Vui lòng thử lại.')
  }
}

/**
 * Xử lý hiển thị dialog xác nhận xóa một tài sản
 * @param {Object} record - Đối tượng tài sản cần xóa
 * Lưu thông tin tài sản cần xóa và hiển thị dialog xác nhận
 */
const handleDelete = (record) => {
  assetToDelete.value = record
  deleteAssetCode.value = record.assetCode || ''
  deleteAssetName.value = record.assetName || ''
  showDeleteDialog.value = true
}

/**
 * Xử lý xác nhận xóa tài sản
 * Gọi API xóa tài sản, nếu thành công thì hiển thị thông báo và refresh dữ liệu
 * Nếu lỗi (ví dụ: tài sản đã có trong chứng từ ghi tăng), hiển thị warning dialog
 */
const handleConfirmDelete = async () => {
  if (!assetToDelete.value || !assetToDelete.value.id) {
    showDeleteDialog.value = false
    return
  }

  try {
    // Nếu là từ bulk delete (1 item), xóa 1 tài sản
    await deleteAsset(assetToDelete.value.id)
    showDeleteDialog.value = false
    
    // Clear selection nếu có
    if (selectedRows.value.length > 0) {
      setSelectedRows([])
    }
    
    assetToDelete.value = null
    deleteAssetCode.value = ''
    deleteAssetName.value = ''
    
    showSuccess('Xóa tài sản thành công!')
    
    await refreshData()
  } catch (err) {
    console.error('Error deleting asset:', err)
    showDeleteDialog.value = false
    
    // Đảm bảo đóng tất cả các dialog khác trước khi hiển thị warning
    showWarningMultiCannotDeleteDialog.value = false
    
    // Kiểm tra nếu lỗi là do tài sản có trong chứng từ ghi tăng
    const errorMessage = err.response?.data?.message || ''
    if (err.response?.status === 400 && errorMessage.includes('chứng từ ghi tăng')) {
      warningMessage.value = errorMessage
      // Chỉ hiển thị single warning dialog
      showWarningCannotDeleteDialog.value = true
    } else {
      alert(errorMessage || 'Không thể xóa tài sản. Vui lòng thử lại.')
    }
    
    assetToDelete.value = null
    deleteAssetCode.value = ''
    deleteAssetName.value = ''
  }
}

/**
 * Xử lý hủy bỏ xóa tài sản
 * Đóng dialog xác nhận xóa và xóa thông tin tài sản đã lưu
 */
const handleCancelDelete = () => {
  showDeleteDialog.value = false
  assetToDelete.value = null
  deleteAssetCode.value = ''
  deleteAssetName.value = ''
}

/**
 * Xử lý xóa nhiều tài sản
 * Kiểm tra số lượng tài sản đã chọn:
 * - Nếu chọn 1 tài sản: hiển thị dialog xác nhận với mã và tên tài sản
 * - Nếu chọn nhiều hơn 1: hiển thị dialog xác nhận với số lượng
 */
const handleBulkDelete = () => {
  if (selectedRows.value.length === 0) {
    alert('Vui lòng chọn ít nhất một tài sản để xóa.')
    return
  }

  // Nếu chọn 1 tài sản → hiển thị dialog với mã - tên
  if (selectedRows.value.length === 1) {
    const selectedAsset = selectedRows.value[0]
    assetToDelete.value = selectedAsset
    deleteAssetCode.value = selectedAsset.assetCode || ''
    deleteAssetName.value = selectedAsset.assetName || ''
    showDeleteDialog.value = true
  } else {
    // Nếu chọn nhiều hơn 1 → hiển thị dialog với số lượng
    showMultiDeleteDialog.value = true
  }
}

/**
 * Xử lý xác nhận xóa nhiều tài sản
 * Lấy danh sách IDs từ các tài sản đã chọn, gọi API bulk delete
 * Nếu thành công: hiển thị thông báo và refresh dữ liệu
 * Nếu có tài sản không thể xóa: hiển thị popup thông báo xóa từng phần
 */
const handleConfirmMultiDelete = async () => {
  try {
    // Lấy danh sách IDs từ selectedRows
    const ids = selectedRows.value
      .map(row => row.id)
      .filter(id => id != null) // Lọc bỏ các giá trị null/undefined

    if (ids.length === 0) {
      showMultiDeleteDialog.value = false
      alert('Không tìm thấy ID của các tài sản đã chọn.')
      return
    }

    // Gọi composable bulk delete
    const result = await bulkDeleteAssets(ids)
    
    // Parse response - hỗ trợ cả format mới và format cũ
    let deletedCount = 0
    let totalCount = ids.length
    let cannotDeleteCount = 0
    
    if (result) {
      // Format mới: { deletedCount, totalCount, cannotDeleteCount }
      if (typeof result.deletedCount === 'number') {
        deletedCount = result.deletedCount
        totalCount = result.totalCount ?? ids.length
        cannotDeleteCount = result.cannotDeleteCount ?? (totalCount - deletedCount)
      } 
      // Format cũ: { deleted }
      else if (typeof result.deleted === 'number') {
        deletedCount = result.deleted
        totalCount = ids.length
        cannotDeleteCount = totalCount - deletedCount
      }
      // Fallback: tính từ số lượng đã xóa
      else {
        deletedCount = 0
        totalCount = ids.length
        cannotDeleteCount = totalCount
      }
    }

    // Clear selection
    setSelectedRows([])
    showMultiDeleteDialog.value = false

    // Kiểm tra nếu có tài sản không thể xóa (bất kể có xóa được tài sản nào hay không)
    if (cannotDeleteCount > 0) {
      // Hiển thị popup thông báo xóa từng phần
      partialDeleteResult.value = {
        deletedCount: deletedCount,
        totalCount: totalCount,
        cannotDeleteCount: cannotDeleteCount
      }
      await nextTick()
      showPartialDeleteResultDialog.value = true
    } else if (deletedCount > 0) {
      // Tất cả đều xóa thành công
      showSuccess(`Đã xóa thành công ${deletedCount} tài sản!`)
    } else {
      // Không xóa được tài sản nào (có thể do lỗi)
      alert('Không thể xóa các tài sản. Vui lòng thử lại.')
    }

    // Refresh data
    await refreshData()
  } catch (err) {
    console.error('Error bulk deleting assets:', err)
    showMultiDeleteDialog.value = false
    
    // Đảm bảo đóng tất cả các dialog khác trước khi hiển thị warning
    showDeleteDialog.value = false
    showWarningCannotDeleteDialog.value = false
    showWarningMultiCannotDeleteDialog.value = false
    showPartialDeleteResultDialog.value = false
    
    // Kiểm tra nếu lỗi là do tài sản có trong chứng từ ghi tăng (fallback cho trường hợp lỗi khác)
    const errorMessage = err.response?.data?.message || ''
    if (err.response?.status === 400 && errorMessage.includes('chứng từ ghi tăng')) {
      // Parse số lượng tài sản không thể xóa từ message
      // Format: "Không thể xóa X tài sản vì các tài sản này đã có trong chứng từ ghi tăng: ..."
      const match = errorMessage.match(/Không thể xóa (\d+) tài sản/)
      if (match && match[1]) {
        warningCount.value = parseInt(match[1], 10)
        // Chỉ hiển thị multi warning dialog
        showWarningMultiCannotDeleteDialog.value = true
      } else {
        // Nếu không parse được số, hiển thị message trực tiếp
        warningMessage.value = errorMessage
        // Chỉ hiển thị single warning dialog
        showWarningCannotDeleteDialog.value = true
      }
    } else {
      alert(errorMessage || 'Không thể xóa các tài sản. Vui lòng thử lại.')
    }
  }
}

/**
 * Xử lý hủy bỏ xóa nhiều tài sản
 * Đóng dialog xác nhận xóa nhiều
 */
const handleCancelMultiDelete = () => {
  showMultiDeleteDialog.value = false
}

/**
 * Xử lý đóng warning dialog khi không thể xóa 1 tài sản
 * Đóng dialog và xóa thông báo lỗi
 */
const handleCloseWarningDialog = () => {
  showWarningCannotDeleteDialog.value = false
  warningMessage.value = ''
}

/**
 * Xử lý đóng warning dialog khi không thể xóa nhiều tài sản
 * Đóng dialog và reset số lượng
 */
const handleCloseWarningMultiDialog = () => {
  showWarningMultiCannotDeleteDialog.value = false
  warningCount.value = 0
}

/**
 * Xử lý đóng dialog thông báo kết quả xóa từng phần
 * Đóng dialog và reset dữ liệu
 */
const handleClosePartialDeleteResultDialog = () => {
  showPartialDeleteResultDialog.value = false
  partialDeleteResult.value = {
    deletedCount: 0,
    totalCount: 0,
    cannotDeleteCount: 0
  }
}

/**
 * Xử lý lưu tài sản (thêm mới hoặc cập nhật)
 * Validate form, format dữ liệu, gọi API create hoặc update
 * Nếu thành công: đóng popup, hiển thị thông báo thành công, refresh dữ liệu
 * Nếu lỗi: hiển thị thông báo lỗi chi tiết
 */
const handleSaveAsset = async () => {
  try {
    // Validate form fields (departmentCode, assetTypeCode)
    if (assetFormRef.value && !assetFormRef.value.validate()) {
      return // Form validation failed, errors are already displayed
    }
    
    // Validate other required fields
    if (!editingAsset.value.assetName || editingAsset.value.assetName.trim() === '') {
      alert('Vui lòng nhập tên tài sản')
      return
    }
    
    if (!editingAsset.value.purchaseDate) {
      alert('Vui lòng chọn ngày mua')
      return
    }
    
    if (!editingAsset.value.quantity || editingAsset.value.quantity <= 0) {
      alert('Vui lòng nhập số lượng lớn hơn 0')
      return
    }
    
    if (!editingAsset.value.originalPrice || editingAsset.value.originalPrice <= 0) {
      alert('Vui lòng nhập nguyên giá lớn hơn 0')
      return
    }

    // Format date - đảm bảo format YYYY-MM-DD cho DateOnly
    const formatDateForAPI = (dateValue) => {
      if (!dateValue) {
        // Nếu không có ngày, lấy ngày hiện tại
        const today = new Date()
        return today.toISOString().split('T')[0]
      }
      
      if (typeof dateValue === 'string') {
        // Nếu là string, lấy phần date (bỏ time nếu có)
        const dateStr = dateValue.split('T')[0]
        // Validate format YYYY-MM-DD
        if (/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) {
          return dateStr
        }
        // Nếu không đúng format, thử parse
        const parsed = new Date(dateValue)
        if (!isNaN(parsed.getTime())) {
          return parsed.toISOString().split('T')[0]
        }
      }
      
      // Nếu là Date object
      if (dateValue instanceof Date) {
        if (isNaN(dateValue.getTime())) {
          throw new Error('Ngày mua không hợp lệ')
        }
        return dateValue.toISOString().split('T')[0]
      }
      
      return dateValue
    }

    // Validate và format dữ liệu
    const typeCode = Number(editingAsset.value.assetTypeCode || editingAsset.value.typeCode)
    if (!typeCode || typeCode <= 0 || typeCode > 255 || isNaN(typeCode)) {
      alert('Mã loại tài sản không hợp lệ')
      return
    }

    const quantity = parseInt(editingAsset.value.quantity)
    if (!quantity || quantity <= 0 || isNaN(quantity)) {
      alert('Số lượng phải là số nguyên dương')
      return
    }

    const purchasePrice = parseFloat(editingAsset.value.originalPrice || editingAsset.value.purchasePrice)
    if (!purchasePrice || purchasePrice < 0.01 || isNaN(purchasePrice)) {
      alert('Nguyên giá phải lớn hơn hoặc bằng 0.01')
      return
    }

    const purchaseDate = formatDateForAPI(editingAsset.value.purchaseDate)
    if (!purchaseDate || !/^\d{4}-\d{2}-\d{2}$/.test(purchaseDate)) {
      alert('Ngày mua không hợp lệ. Vui lòng chọn lại ngày mua.')
      return
    }

    // Map frontend data to API RequestAssetDto format
    const assetData = {
      assetCode: editingAsset.value.assetCode?.trim() || '',
      assetName: editingAsset.value.assetName?.trim() || '',
      deptCode: (editingAsset.value.departmentCode || editingAsset.value.deptCode || '').trim(),
      typeCode: typeCode,
      quantity: quantity,
      purchasePrice: purchasePrice,
      purchaseDate: purchaseDate // Phải là string format YYYY-MM-DD
    }

    // Log để debug
    console.log('Sending asset data:', JSON.stringify(assetData, null, 2))

    if (isEditMode.value) {
      // Update mode
      if (!editingAsset.value.id) {
        alert('Không tìm thấy ID của tài sản cần cập nhật')
        return
      }
      
      await updateAsset(editingAsset.value.id, assetData)
      showSuccess('Cập nhật tài sản thành công!')
    } else {
      // Create mode
      await createAsset(assetData)
      showSuccess('Thêm tài sản thành công!')
    }
    
    // Đóng popup edit trước
    showEditPopup.value = false
    editingAsset.value = {}
    originalAsset.value = null
    isEditMode.value = false
    // Reset validation state
    nextTick(() => {
      if (assetFormRef.value) {
        assetFormRef.value.resetValidation()
      }
    })
    
    // Refresh data
    await refreshData()
  } catch (err) {
    console.error('Error saving asset:', err)
    console.error('Error response:', err.response?.data)
    console.error('Error status:', err.response?.status)
    
    // Hiển thị lỗi chi tiết hơn
    let errorMessage = 'Không thể lưu tài sản. Vui lòng thử lại.'
    
    if (err.response?.data) {
      const errorData = err.response.data
      if (errorData.message) {
        errorMessage = errorData.message
      } else if (errorData.error) {
        errorMessage = errorData.error
      } else if (errorData.errors) {
        // Validation errors từ ModelState
        const validationErrors = Object.values(errorData.errors).flat().join('\n')
        errorMessage = `Lỗi validation:\n${validationErrors}`
      } else if (typeof errorData === 'string') {
        errorMessage = errorData
      }
    } else if (err.message) {
      errorMessage = err.message
    }
    
    alert(errorMessage)
  }
}

/**
 * Kiểm tra xem người dùng đã có thay đổi dữ liệu trong form hay chưa
 * (không tính các trường autofill như departmentName, assetTypeName, trackingYear, annualDepreciationValue)
 * 
 * @returns {boolean} - true nếu có thay đổi, false nếu chưa có thay đổi
 * 
 * Logic:
 * - Nếu là thêm mới (originalAsset = null): kiểm tra có dữ liệu người dùng nhập không (loại trừ giá trị mặc định)
 * - Nếu là sửa/nhân bản: so sánh với bản gốc, chỉ so sánh các trường người dùng có thể chỉnh sửa
 */
const hasChanges = () => {
  if (!originalAsset.value) {
    // Nếu là thêm mới, kiểm tra có dữ liệu người dùng nhập không (loại trừ autofill và giá trị mặc định)
    const current = editingAsset.value
    
    // Lấy ngày hôm nay để so sánh
    const today = new Date()
    const todayStr = today.toISOString().split('T')[0]
    
    // Kiểm tra các trường người dùng có thể nhập/chỉnh sửa (loại trừ autofill và giá trị mặc định)
    const hasUserInput = !!(
      (current.assetName && current.assetName.trim() !== '') ||
      (current.departmentCode && current.departmentCode !== '') ||
      (current.assetTypeCode && current.assetTypeCode !== 0) ||
      (current.quantity && current.quantity !== 1) ||
      (current.originalPrice && current.originalPrice !== 0) ||
      (current.purchaseDate && current.purchaseDate !== todayStr) ||
      (current.startUsageDate && current.startUsageDate !== todayStr) ||
      (current.yearsOfUse && current.yearsOfUse !== 0) ||
      (current.depreciationRate && current.depreciationRate !== 0)
    )
    
    return hasUserInput
  }
  
  // So sánh với bản gốc (chỉ các trường người dùng có thể chỉnh sửa, loại trừ autofill)
  const current = editingAsset.value
  const original = originalAsset.value
  
  // Chỉ so sánh các trường người dùng có thể chỉnh sửa
  // Loại trừ: departmentName, assetTypeName, trackingYear, annualDepreciationValue (autofill)
  return (
    (current.assetName || '') !== (original.assetName || '') ||
    (current.departmentCode || '') !== (original.departmentCode || '') ||
    (current.assetTypeCode || 0) !== (original.assetTypeCode || 0) ||
    (current.quantity || 0) !== (original.quantity || 0) ||
    (current.originalPrice || 0) !== (original.originalPrice || 0) ||
    (current.purchaseDate || '') !== (original.purchaseDate || '') ||
    (current.startUsageDate || '') !== (original.startUsageDate || '') ||
    (current.yearsOfUse || 0) !== (original.yearsOfUse || 0) ||
    (current.depreciationRate || 0) !== (original.depreciationRate || 0)
  )
}

/**
 * Xử lý khi người dùng đóng popup form (click nút Hủy hoặc X)
 * Kiểm tra xem có thay đổi hay không:
 * - Có thay đổi: hiển thị dialog xác nhận lưu (ConfirmSaveDialog)
 * - Không có thay đổi: hiển thị dialog xác nhận hủy (ConfirmCancelDialog)
 * Giữ popup mở để hiển thị dialog
 */
const handleClosePopup = () => {
  // Ngăn popup đóng - giữ popup mở để hiển thị dialog
  // Đảm bảo popup vẫn mở
  if (!showEditPopup.value) {
    showEditPopup.value = true
  }
  
  // Kiểm tra có thay đổi không
  if (hasChanges()) {
    // Có thay đổi → hiển thị dialog xác nhận lưu
    showSaveDialog.value = true
  } else {
    // Không có thay đổi → hiển thị dialog xác nhận hủy
    showCancelDialog.value = true
  }
}

/**
 * Xử lý xác nhận hủy bỏ (từ ConfirmCancelDialog)
 * Đóng dialog và popup, reset tất cả dữ liệu form và validation state
 */
const handleConfirmCancel = () => {
  // Xác nhận hủy → đóng popup
  showCancelDialog.value = false
  showEditPopup.value = false
  editingAsset.value = {}
  originalAsset.value = null
  isEditMode.value = false
  // Reset validation state
  nextTick(() => {
    if (assetFormRef.value) {
      assetFormRef.value.resetValidation()
    }
  })
}

/**
 * Xử lý khi người dùng chọn "Không" trong dialog xác nhận hủy
 * Đóng dialog, giữ nguyên popup để người dùng tiếp tục chỉnh sửa
 */
const handleCancelCancel = () => {
  // Không hủy → đóng dialog, giữ nguyên popup
  showCancelDialog.value = false
}

/**
 * Xử lý khi người dùng chọn "Lưu" trong dialog xác nhận lưu
 * Đóng dialog và gọi hàm lưu tài sản
 */
const handleConfirmSave = async () => {
  // Xác nhận lưu → lưu và đóng popup
  showSaveDialog.value = false
  await handleSaveAsset()
}

/**
 * Xử lý khi người dùng chọn "Không lưu" trong dialog xác nhận lưu
 * Đóng dialog và popup mà không lưu dữ liệu, reset tất cả state
 */
const handleDontSave = () => {
  // Không lưu → đóng popup mà không lưu
  showSaveDialog.value = false
  showEditPopup.value = false
  editingAsset.value = {}
  originalAsset.value = null
  isEditMode.value = false
  // Reset validation state
  nextTick(() => {
    if (assetFormRef.value) {
      assetFormRef.value.resetValidation()
    }
  })
}

/**
 * Xử lý khi người dùng chọn "Hủy bỏ" trong dialog xác nhận lưu
 * Đóng dialog, giữ nguyên popup để người dùng tiếp tục chỉnh sửa
 */
const handleCancelSave = () => {
  // Hủy bỏ → đóng dialog, giữ nguyên popup
  showSaveDialog.value = false
}

/**
 * Xử lý đóng dialog thông báo thành công
 * Đóng dialog và xóa thông báo
 */

/**
 * Xử lý khi window resize để responsive
 * Cập nhật windowWidth, tính lại chiều cao bảng và đồng bộ độ rộng cột summary
 */
const handleResize = () => {
  windowWidth.value = window.innerWidth
  calculateTableScrollHeight()
  syncSummaryColumnWidths()
}

/**
 * Tải danh sách bộ phận sử dụng từ API
 * Lưu dữ liệu gốc và tạo options cho dropdown filter
 */
const loadDepartments = async () => {
  try {
    const departments = await getDepartments()
    departmentsData.value = departments
    departmentOptions.value = departments.map(dept => ({
      label: dept.departmentName,
      value: dept.departmentCode
    }))
  } catch (err) {
    console.error('Error loading departments:', err)
  }
}

/**
 * Tải danh sách loại tài sản từ API
 * Lưu dữ liệu gốc và tạo options cho dropdown filter
 */
const loadAssetTypes = async () => {
  try {
    const assetTypes = await getAssetTypes()
    assetTypesData.value = assetTypes
    assetTypeOptions.value = assetTypes.map(type => ({
      label: type.typeName,
      value: type.typeCode
    }))
  } catch (err) {
    console.error('Error loading asset types:', err)
  }
}

onMounted(async () => {
  await Promise.all([loadDepartments(), loadAssetTypes()])
  await loadAssetsData()
  
  // Calculate table height and sync columns
  calculateTableScrollHeight()
  syncSummaryColumnWidths()
  setTimeout(() => {
    calculateTableScrollHeight()
    syncSummaryColumnWidths()
  }, 100)
  
  // Handle window resize for responsive columns
  window.addEventListener('resize', handleResize)
})

onUpdated(() => {
  calculateTableScrollHeight()
  syncSummaryColumnWidths()
})

// Watch filters and pagination changes
watch(() => filters.value.assetType, async () => {
  pagination.value.currentPage = 1
  await loadAssetsData()
})

watch(() => filters.value.department, async () => {
  pagination.value.currentPage = 1
  await loadAssetsData()
})

onUnmounted(() => {
  window.removeEventListener('resize', handleResize)
})
</script>

<style scoped>
.asset-list-view {
  padding: 24px;
  background-color: #f4f7ff;
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.asset-list-view__filters {
  display: flex;
  gap: 8px;
  margin-bottom: 8px;
}

.asset-list-view__search {
  width: 219px;
}

.asset-list-view__filter {
  width: 219px;
  min-width: 219px;
  max-width: 219px;
}

.asset-list-view__actions {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-left: auto;
}

.asset-list-view__action-btn {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #ffffff;
  border: none;
  color: #666666;
  cursor: pointer;
  border-radius: 3px;
  transition: all 0.2s;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.asset-list-view__action-btn--add {
  background-color: #1aa4c8;
  box-shadow: inset 0 2px 6px rgba(0, 0, 0, 0.16);
  width: 110px;
  height: 36px;
  color: #ffffff;
  justify-content: flex-start;
  font-size: 13px;
  font-weight: 500;
  font-family: 'Roboto', sans-serif;
  padding-left: 4px;
  border: 1px solid #1aa4c8;
}

.asset-list-view__action-btn--add svg {
  margin-right: -4px;
}

.asset-list-view__action-btn--add:hover {
  background-color: #ffffff !important;
  border: 1px solid #1aa4c8 !important;
  color: #1aa4c8 !important;
}

.asset-list-view__action-btn--add svg :deep(rect) {
  display: none;
}

.asset-list-view__action-btn--add svg :deep(path) {
  stroke: #ffffff;
  fill: none;
}

.asset-list-view__action-btn--add:hover svg :deep(path) {
  stroke: #1aa4c8;
  fill: none;
}

.asset-list-view__action-btn:not(.asset-list-view__action-btn--add):hover {
  background-color: #f5f5f5;
  color: #212121;
}

.asset-list-view__action-btn--delete svg :deep(rect) {
  display: none;
}

.asset-list-view__action-btn--delete svg :deep(path),
.asset-list-view__action-btn--delete svg :deep(g) {
  stroke: #ff3131;
  fill: none;
}

.asset-list-view__action-btn--disabled {
  opacity: 0.4;
  cursor: not-allowed;
  pointer-events: none;
}

.asset-list-view__action-btn--disabled:hover {
  background-color: #ffffff !important;
  color: #666666 !important;
}

.asset-list-view__table {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
}

.custom-table-wrapper {
  background-color: #ffffff;
  border-radius: 6px;
  border: none;
  overflow: visible;
  margin-bottom: 17px;
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  box-shadow: 0 3px 10px rgba(0, 0, 0, .16);
  position: relative;
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
  border-radius: 6px 6px 0 0;
  border: 0.5px solid #afafaf;
  border-bottom: none;
  margin: 0;
  width: 100%;
}

:deep(.custom-ant-table .ant-table) {
  background-color: #ffffff;
  border-radius: 6px 6px 0 0;
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
  white-space: nowrap;
  height: 40px;
}

:deep(.custom-ant-table .ant-table-thead > tr > th:first-child) {
  border-top-left-radius: 6px;
}

:deep(.custom-ant-table .ant-table-thead > tr > th:last-child) {
  border-top-right-radius: 6px;
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
  border: 0.5px solid #afafaf;
  border-top: 0.5px solid #afafaf;
  border-radius: 0 0 6px 6px;
  padding: 0;
  width: 100%;
  box-sizing: border-box;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  overflow: visible;
  margin-top: auto;
  margin: 0;
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
  border-radius: 0 0 6px 6px;
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

.table-actions {
  display: flex;
  gap: 2px;
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
  opacity: 0;
  visibility: hidden;
}

.table-actions__btn:hover {
  background-color: #f5f5f5;
  color: #212121;
}

/* Table actions styling */
:deep(.custom-ant-table .ant-table-tbody > tr:hover) .table-actions__btn {
  opacity: 1;
  visibility: visible;
}

:deep(.custom-ant-table .ant-table-tbody > tr) {
  width: 1260px;
  min-width: 1260px;
}

/* STT column styling */
:deep(.custom-ant-table .ant-table-thead > tr > th:first-child),
:deep(.custom-ant-table .ant-table-tbody > tr > td:first-child) {
  padding-left: 25px;
  padding-right: 25px;
  width: 30px;
  min-width: 30px;
  max-width: 30px;
  text-align: center;
}

.custom-summary-value {
  font-weight: 700;
  color: #001031;
  display: inline-block;
}

/* Responsive styles for smaller screens */
@media (max-width: 1600px) {
  :deep(.custom-ant-table .ant-table-thead > tr > th),
  :deep(.custom-ant-table .ant-table-tbody > tr > td) {
    padding: 10px 12px;
    font-size: 12px;
  }

  .asset-list-view__filters {
    flex-wrap: wrap;
  }

  .asset-list-view__search {
    width: 180px;
  }

  .asset-list-view__filter {
    width: 219px;
    min-width: 219px;
    max-width: 219px;
  }
}

@media (max-width: 1400px) {
  .asset-list-view {
    padding: 16px;
  }

  :deep(.custom-ant-table .ant-table-thead > tr > th),
  :deep(.custom-ant-table .ant-table-tbody > tr > td) {
    padding: 8px 10px;
    font-size: 12px;
  }

  :deep(.custom-ant-table .ant-table-thead > tr > th:first-child),
  :deep(.custom-ant-table .ant-table-tbody > tr > td:first-child) {
    padding-left: 15px;
    padding-right: 15px;
  }

  .asset-list-view__search {
    width: 150px;
  }

  .asset-list-view__filter {
    width: 219px;
    min-width: 219px;
    max-width: 219px;
  }

  .custom-table-footer {
    padding: 6px 12px;
    flex-wrap: wrap;
    gap: 8px;
  }

  .custom-footer-left {
    gap: 8px;
  }
}

@media (max-width: 1200px) {
  .asset-list-view {
    padding: 12px;
  }

  :deep(.custom-ant-table .ant-table-thead > tr > th),
  :deep(.custom-ant-table .ant-table-tbody > tr > td) {
    padding: 6px 8px;
    font-size: 11px;
  }

  .asset-list-view__action-btn--add {
    width: 90px;
    font-size: 12px;
    padding-left: 2px;
  }

  .custom-table-footer {
    padding: 4px 8px;
    min-height: 40px;
  }

  .custom-footer-summary {
    font-size: 11px;
  }
}
</style>