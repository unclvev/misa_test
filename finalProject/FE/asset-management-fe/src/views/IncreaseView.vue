  <template>
    <div class="increase-view">
        <!-- Custom Header -->
      <header class="increase-header">
        <div class="increase-header__left">
          <span class="increase-header__org">Bệnh viện tỉnh MISA</span>
          <span class="increase-header__year-label">Dữ liệu năm</span>
          <div class="increase-header__year-wrapper">
            <div class="increase-header__year-input">
              <a-input-number v-model:value="selectedYear" :min="1970" :max="2099" />
            </div>
          </div>
        </div>

        <div class="increase-header__right">
          <div class="increase-header__actions">
            <button class="increase-header__icon-btn" title="Thông báo">
              <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
                <use href="#icon-lock" />
              </svg>
            </button>
            <button class="increase-header__icon-btn" title="Ứng dụng">
              <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
                <use href="#icon-he-thong" />
              </svg>
            </button>
            <button class="increase-header__icon-btn" title="Trợ giúp">
              <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
                <use href="#icon-ho-tro" />
              </svg>
            </button>
            <button class="increase-header__icon-btn" title="Tài khoản">
              <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
                <use href="#icon-user" />
              </svg>
            </button>
            <button class="increase-header__icon-btn increase-header__icon-btn--dropdown" title="Dropdown">
              <svg width="26" height="26" viewBox="0 0 24 24" fill="currentColor" preserveAspectRatio="xMidYMid meet">
                <use href="#icon-dropdown" />
              </svg>
            </button>
          </div>
        </div>
      </header>

      <!-- Edit Form -->
      <EditIncreaseVoucherForm
        v-if="isEditing"
        :voucher="editingVoucher"
        @close="handleCloseEdit"
        @save="handleSaveEdit"
        @delete="handleDeleteEdit"
        @cancel="handleCloseEdit"
      />
      
      <!-- List View -->
      <template v-else>
      <!-- Header Section -->
      <div class="increase-view__header">
        <div class="increase-view__title-section">
          <h1 class="increase-view__title">Ghi tăng tài sản</h1>
          <button class="increase-view__refresh-btn" @click="handleRefresh" title="Làm mới">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <use href="#icon-refresh" />
            </svg>
          </button>
        </div>
        <div class="increase-view__actions">
          <button 
            class="increase-view__action-btn increase-view__action-btn--add" 
            @click="handleAddVoucher"
            @mouseenter="isAddButtonHovered = true"
            @mouseleave="isAddButtonHovered = false"
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <use :href="isAddButtonHovered ? '#icon-plus' : '#icon-plus-2'" />
            </svg>
            Thêm chứng từ
          </button>
          <button class="increase-view__action-btn" title="Xuất Excel">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <use href="#icon-excel" />
            </svg>
          </button>
          <button 
            class="increase-view__action-btn increase-view__action-btn--delete" 
            :class="{ 'increase-view__action-btn--disabled': selectedRows.length === 0 }"
            :disabled="selectedRows.length === 0"
            title="Xóa nhiều"
            @click="handleBulkDelete"
          >
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <use href="#icon-delete" />
            </svg>
          </button>
          <button class="increase-view__action-btn" title="Chat">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
              <use href="#icon-lock" />
            </svg>
          </button>
          <button class="increase-view__action-btn" title="Trợ giúp">
            <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor" xmlns="http://www.w3.org/2000/svg">
              <use href="#icon-ho-tro" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Table Section -->
      <div class="increase-view__table">
        <div class="custom-table-wrapper">
          <div v-if="loading" class="loading-overlay">
            <div class="loading-spinner">Đang tải...</div>
          </div>
          <div v-if="error && !loading" class="error-message">
            <strong>Lỗi:</strong> {{ error }}
            <button class="error-retry-btn" @click="loadVouchers">Thử lại</button>
          </div>
          <a-table
            :columns="antdColumns"
            :data-source="paginatedData"
            :row-selection="rowSelection"
            :pagination="false"
            :scroll="{ y: tableScrollHeight }"
            :bordered="false"
            :row-key="(record) => record.id || record.voucherNumber || record.stt"
            :loading="loading"
            class="custom-ant-table"
            @row="(record) => ({ onClick: () => handleRowClick(record) })"
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'voucherNumber'">
                <span class="voucher-number-link" @click.stop="handleVoucherClick(record)">
                  {{ record.voucherNumber }}
                </span>
              </template>
              <template v-if="column.key === 'actions'">
                <div class="table-actions">
                  <button 
                    class="table-actions__btn" 
                    @click.stop="handlePrint(record)" 
                    title="In"
                  >
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor">
                      <use href="#icon-copy-file" />
                    </svg>
                  </button>
                  <span class="table-actions__divider"></span>
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
                    @click.stop="handleDelete(record)" 
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
                    v-for="col in antdColumns" 
                    :key="col.key"
                    :class="[
                      'custom-filter-cell',
                      col.align === 'right' ? 'text-right' : col.align === 'center' ? 'text-center' : 'text-left'
                    ]"
                  >
                    <div v-if="col.key !== 'actions'" class="filter-input-wrapper">
                      <span v-if="col.key === 'totalOriginalPrice'" class="filter-equals-sign">=</span>
                      <input
                        :value="filterValues[col.key]"
                        @input="handleFilterInput(col.key, $event)"
                        type="text"
                        :placeholder="`Lọc ${col.title?.toLowerCase() || ''}`"
                        class="filter-input"
                        :class="{ 'filter-input--with-equals': col.key === 'totalOriginalPrice' }"
                      />
                      <svg 
                        v-if="col.key !== 'totalOriginalPrice'"
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
          <div v-if="voucherSummary" class="custom-summary-row">
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
                      <span v-if="col.key === 'totalOriginalPrice'" 
                            class="custom-summary-value">
                        {{ formatCurrency(voucherSummary[col.key]) }}
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

      <!-- Dialogs -->
      <ConfirmDeleteVoucherDialog
        v-model="showDeleteDialog"
        :voucher-number="deleteVoucherNumber"
        @confirm="handleConfirmDelete"
        @cancel="handleCancelDelete"
      />

      <ConfirmMultiDeleteVoucherDialog
        v-model="showMultiDeleteDialog"
        :count="selectedRows.length"
        @confirm="handleConfirmMultiDelete"
        @cancel="handleCancelMultiDelete"
      />

      <PartialDeleteVoucherResultDialog
        v-model="showPartialDeleteResultDialog"
        :deleted-count="partialDeleteResult.deletedCount"
        :total-count="partialDeleteResult.totalCount"
        :cannot-delete-count="partialDeleteResult.cannotDeleteCount"
        @confirm="handleClosePartialDeleteResultDialog"
      />
    </div>
  </template>

  <script setup>
  import { ref, computed, onMounted, onUnmounted, watch, nextTick, onUpdated } from 'vue'
  import { useRouter } from 'vue-router'
  import { InputNumber as AInputNumber } from 'ant-design-vue'
  import MPageSizeDropdown from '../components/ui/MPageSizeDropdown.vue'
  import EditIncreaseVoucherForm from '../components/EditIncreaseVoucherForm.vue'
  import { formatCurrency } from '../utils/formatters'
  import { useIncreaseVoucherApi } from '../composables/useIncreaseVoucherApi'
  import { useToast } from '../composables/useToast'
  import ConfirmDeleteVoucherDialog from '../components/dialogs/ConfirmDeleteVoucherDialog.vue'
  import ConfirmMultiDeleteVoucherDialog from '../components/dialogs/ConfirmMultiDeleteVoucherDialog.vue'
  import PartialDeleteVoucherResultDialog from '../components/dialogs/PartialDeleteVoucherResultDialog.vue'

  const router = useRouter()

  // Year selection for header
  const currentYear = new Date().getFullYear()
  const selectedYear = ref(currentYear)

  // API composable
  const {
    loading,
    error,
    loadVouchers: loadVouchersFromApi,
    refreshData: refreshDataFromApi,
    getNextVoucherNo,
    getVoucherById,
    createVoucher,
    updateVoucher,
    deleteVoucher,
    bulkDeleteVouchers
  } = useIncreaseVoucherApi()

  // Toast composable
  const { showSuccess } = useToast()

  // Data
  const vouchers = ref([])
  const selectedRows = ref([])
  const windowWidth = ref(window.innerWidth)
  const isAddButtonHovered = ref(false)
  const isEditing = ref(false)
  const editingVoucher = ref(null)

  // Dialog states
  const showDeleteDialog = ref(false)
  const showMultiDeleteDialog = ref(false)
  const showPartialDeleteResultDialog = ref(false)
  const deleteVoucherNumber = ref('')
  const voucherToDelete = ref(null)
  const partialDeleteResult = ref({
    deletedCount: 0,
    totalCount: 0,
    cannotDeleteCount: 0
  })

  // Filter states
  const filterValues = ref({
    voucherNumber: '',
    voucherDate: '',
    increaseDate: '',
    totalOriginalPrice: '',
    content: ''
  })

  // Pagination
  const pagination = ref({
    currentPage: 1,
    pageSize: 20
  })

  const totalRecords = ref(0)

  const paginatedData = computed(() => vouchers.value)

  const voucherSummary = computed(() => {
    if (vouchers.value.length === 0) return null
    const total = vouchers.value.reduce((sum, v) => sum + (v.totalOriginalPrice || 0), 0)
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
  const totalPages = computed(() => {
    return Math.ceil(totalRecords.value / pagination.value.pageSize)
  })

  /**
   * Tính toán danh sách các số trang hiển thị trong pagination
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

  /**
   * Tính toán chiều cao scroll của bảng
   */
  const calculateTableScrollHeight = () => {
    nextTick(() => {
      const container = document.querySelector('.increase-view__table')
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
      const antTable = document.querySelector('.increase-view__table .custom-ant-table .ant-table')
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

  /**
   * Chèn filter row vào thead của Ant Design table
   */
  const insertFilterRow = () => {
    if (!filterRowRef.value) return
    
    nextTick(() => {
      const antTable = document.querySelector('.increase-view__table .custom-ant-table .ant-table')
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
      const antTable = document.querySelector('.increase-view__table .custom-ant-table .ant-table')
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


  // Responsive width calculation
  const isSmallScreen = computed(() => windowWidth.value < 1400)
  const isMediumScreen = computed(() => windowWidth.value >= 1400 && windowWidth.value < 1600)
  const isLargeScreen = computed(() => windowWidth.value >= 1600)

  // Ant Design Table columns
  const antdColumns = computed(() => {
    const getWidth = (small, medium, large) => {
      if (isSmallScreen.value) return small
      if (isMediumScreen.value) return medium
      return large
    }

    return [
      {
        title: 'Số chứng từ',
        dataIndex: 'voucherNumber',
        key: 'voucherNumber',
        align: 'left',
        width: getWidth(120, 140, 160)
      },
      {
        title: 'Ngày chứng từ',
        dataIndex: 'voucherDate',
        key: 'voucherDate',
        align: 'left',
        width: getWidth(120, 140, 160)
      },
      {
        title: 'Ngày ghi tăng',
        dataIndex: 'increaseDate',
        key: 'increaseDate',
        align: 'left',
        width: getWidth(120, 140, 160)
      },
      {
        title: 'Tổng nguyên giá',
        dataIndex: 'totalOriginalPrice',
        key: 'totalOriginalPrice',
        align: 'right',
        width: getWidth(140, 160, 180),
        customRender: ({ text }) => formatCurrency(text)
      },
      {
        title: 'Nội dung',
        dataIndex: 'content',
        key: 'content',
        align: 'left',
        width: getWidth(200, 250, 300)
      },
      {
        title: 'Chức năng',
        key: 'actions',
        align: 'center',
        width: getWidth(120, 140, 160),
        fixed: 'right'
      }
    ]
  })


  /**
   * Cấu hình row selection cho Ant Design Table
   * - onChange: xử lý khi chọn/bỏ chọn một dòng
   * - onSelectAll: xử lý khi chọn/bỏ chọn tất cả, merge với selection hiện tại để tránh duplicate
   */
  const rowSelection = computed(() => ({
    selectedRowKeys: selectedRows.value.map(row => row.id || row.voucherNumber || row.stt),
    onChange: (selectedRowKeys, selectedRowsData) => {
      selectedRows.value = selectedRowsData || []
    },
    onSelectAll: (selected, selectedRowsData, changeRows) => {
      if (selected) {
        // Merge với selectedRows hiện tại, tránh duplicate
        const existingKeys = new Set(selectedRows.value.map(row => row.id || row.voucherNumber || row.stt))
        const newRows = changeRows.filter(row => !existingKeys.has(row.id || row.voucherNumber || row.stt))
        selectedRows.value = [...selectedRows.value, ...newRows]
      } else {
        // Remove các rows đã bỏ chọn
        const changeRowKeys = new Set(changeRows.map(row => row.id || row.voucherNumber || row.stt))
        selectedRows.value = selectedRows.value.filter(
          row => !changeRowKeys.has(row.id || row.voucherNumber || row.stt)
        )
      }
    }
  }))


  /**
   * Tải dữ liệu danh sách chứng từ từ API
   * Kết hợp thông tin pagination và filters để gọi API
   * Cập nhật danh sách vouchers, tổng số bản ghi và thông tin pagination
   */
  const loadVouchers = async () => {
    const result = await loadVouchersFromApi({
      currentPage: pagination.value.currentPage,
      pageSize: pagination.value.pageSize,
      filters: filterValues.value
    })
    
    vouchers.value = result.vouchers
    totalRecords.value = result.totalRecords
    pagination.value.currentPage = result.pageNumber
    pagination.value.pageSize = result.pageSize
  }

  // Handlers
  const handleRefresh = () => {
    loadVouchers()
  }

  const handleRowClick = () => {
    // Row click handler
  }

  const handleVoucherClick = (record) => {
    console.log('Voucher clicked:', record)
    // TODO: Navigate to voucher detail
  }

  const handleAddVoucher = async () => {
    try {
      // Lấy số chứng từ tiếp theo từ composable
      const nextVoucherNo = await getNextVoucherNo()
      
      // Tạo voucher mới với số chứng từ tự động
      editingVoucher.value = {
        voucherNumber: nextVoucherNo,
        voucherDate: '',
        increaseDate: '',
        content: '',
        details: []
      }
      isEditing.value = true
    } catch (err) {
      console.error('Error getting next voucher number:', err)
      // Nếu không lấy được, vẫn mở form với voucherNumber rỗng
      editingVoucher.value = {
        voucherNumber: '',
        voucherDate: '',
        increaseDate: '',
        content: '',
        details: []
      }
      isEditing.value = true
    }
  }

  const handleEdit = async (record) => {
    if (!record.id) {
      alert('Không tìm thấy ID chứng từ')
      return
    }

    // Load full voucher data from composable
    try {
      const voucherData = await getVoucherById(record.id)
      
      if (voucherData && voucherData.id) {
        // Map API response to frontend format
        editingVoucher.value = {
          id: voucherData.id,
          voucherNumber: voucherData.voucherNo || voucherData.voucherNumber || '',
          voucherDate: formatDateForInput(voucherData.voucherDate),
          increaseDate: formatDateForInput(voucherData.increaseDate),
          content: voucherData.note || '',
          details: voucherData.details || []
        }
        isEditing.value = true
      } else {
        alert('Không tìm thấy chứng từ hoặc dữ liệu không hợp lệ')
      }
    } catch (err) {
      console.error('Error loading voucher:', err)
      
      let errorMessage = 'Đã xảy ra lỗi khi lấy thông tin chứng từ'
      
      if (err.code === 'ERR_NETWORK' || err.message?.includes('Network Error')) {
        errorMessage = 'Không thể kết nối đến server. Vui lòng kiểm tra xem backend API có đang chạy không.'
      } else if (err.response) {
        if (err.response.status === 404) {
          errorMessage = 'Không tìm thấy chứng từ với ID này'
        } else if (err.response.status === 500) {
          errorMessage = err.response.data?.message || err.response.data?.error || 'Lỗi server khi lấy thông tin chứng từ'
        } else {
          errorMessage = err.response.data?.message || err.response.data?.error || `Lỗi ${err.response.status}: ${err.response.statusText}`
        }
      } else if (err.request) {
        errorMessage = 'Không nhận được phản hồi từ server. Vui lòng kiểm tra kết nối mạng.'
      } else {
        errorMessage = err.message || 'Đã xảy ra lỗi khi tải dữ liệu'
      }
      
      alert(errorMessage)
    }
  }

  /**
   * Format date từ API (YYYY-MM-DD hoặc DateOnly) sang format hiển thị (YYYY-MM-DD cho date picker)
   */
  const formatDateForInput = (dateValue) => {
    if (!dateValue) return ''
    
    // Nếu là string
    if (typeof dateValue === 'string') {
      // Nếu đã là YYYY-MM-DD, giữ nguyên
      if (dateValue.match(/^\d{4}-\d{2}-\d{2}$/)) {
        return dateValue
      }
      // Nếu là format khác, parse thành Date rồi format lại
      const date = new Date(dateValue)
      if (!isNaN(date.getTime())) {
        const year = date.getFullYear()
        const month = String(date.getMonth() + 1).padStart(2, '0')
        const day = String(date.getDate()).padStart(2, '0')
        return `${year}-${month}-${day}`
      }
    }
    
    // Nếu là Date object
    if (dateValue instanceof Date) {
      if (!isNaN(dateValue.getTime())) {
        const year = dateValue.getFullYear()
        const month = String(dateValue.getMonth() + 1).padStart(2, '0')
        const day = String(dateValue.getDate()).padStart(2, '0')
        return `${year}-${month}-${day}`
      }
    }
    
    // Nếu là object có year, month, day (DateOnly serialized)
    if (typeof dateValue === 'object' && dateValue !== null) {
      if ('year' in dateValue && 'month' in dateValue && 'day' in dateValue) {
        const year = dateValue.year
        const month = String(dateValue.month).padStart(2, '0')
        const day = String(dateValue.day).padStart(2, '0')
        return `${year}-${month}-${day}`
      }
    }
    
    return ''
  }

  const handleCloseEdit = () => {
    isEditing.value = false
    editingVoucher.value = null
  }

  const handleSaveEdit = async (formData) => {
    try {
      // Validate required fields
      if (!formData.voucherDate) {
        alert('Vui lòng chọn ngày chứng từ')
        return
      }
      
      if (!formData.increaseDate) {
        alert('Vui lòng chọn ngày ghi tăng')
        return
      }

      // Format date - đảm bảo format YYYY-MM-DD
      const formatDateForAPI = (dateValue) => {
        if (!dateValue) return ''
        
        if (typeof dateValue === 'string') {
          // Nếu đã là YYYY-MM-DD, giữ nguyên
          if (dateValue.match(/^\d{4}-\d{2}-\d{2}$/)) {
            return dateValue
          }
          // Nếu là format khác, parse thành Date rồi format lại
          const date = new Date(dateValue)
          if (!isNaN(date.getTime())) {
            const year = date.getFullYear()
            const month = String(date.getMonth() + 1).padStart(2, '0')
            const day = String(date.getDate()).padStart(2, '0')
            return `${year}-${month}-${day}`
          }
        }
        
        if (dateValue instanceof Date) {
          if (!isNaN(dateValue.getTime())) {
            return dateValue.toISOString().split('T')[0]
          }
        }
        
        return dateValue
      }

      // Convert frontend format to API format
      const assets = formData.assets || []
      const apiData = {
        voucherNo: (formData.voucherNumber || '').trim(),
        voucherDate: formatDateForAPI(formData.voucherDate),
        increaseDate: formatDateForAPI(formData.increaseDate),
        note: (formData.content || '').trim() || null,
        // Chỉ gửi details nếu có assets, nếu không thì gửi mảng rỗng hoặc null
        details: assets.length > 0 
          ? assets.map(asset => ({
              assetId: asset.id || asset.assetId
            }))
          : []
      }

      // Log để debug
      console.log('Sending voucher data:', JSON.stringify(apiData, null, 2))

      if (editingVoucher.value?.id) {
        // Update existing voucher - phải có ít nhất 1 asset
        if (apiData.details.length === 0) {
          alert('Chứng từ phải có ít nhất 1 tài sản khi cập nhật')
          return
        }
        await updateVoucher(editingVoucher.value.id, apiData)
        showSuccess('Cập nhật chứng từ thành công!')
      } else {
        // Create new voucher - có thể không có assets (sẽ thêm sau)
        await createVoucher(apiData)
        showSuccess('Thêm chứng từ thành công!')
      }
      
      // Refresh voucher list
      await loadVouchers()
      
      // Close edit form
      handleCloseEdit()
    } catch (err) {
      console.error('Error saving voucher:', err)
      console.error('Error response:', err.response?.data)
      console.error('Error status:', err.response?.status)
      
      // Hiển thị lỗi chi tiết hơn
      let errorMessage = 'Không thể lưu chứng từ. Vui lòng thử lại.'
      
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

  const handleDeleteEdit = async () => {
    if (!editingVoucher.value?.id) {
      // Nếu không có id, chỉ đóng form (đang ở chế độ thêm mới)
      handleCloseEdit()
      return
    }

    // Hiển thị dialog xác nhận xóa
    voucherToDelete.value = editingVoucher.value
    deleteVoucherNumber.value = editingVoucher.value.voucherNumber || ''
    showDeleteDialog.value = true
  }

  const handleDelete = (record) => {
    voucherToDelete.value = record
    deleteVoucherNumber.value = record.voucherNumber || ''
    showDeleteDialog.value = true
  }

  const handleConfirmDelete = async () => {
    if (!voucherToDelete.value || !voucherToDelete.value.id) {
      showDeleteDialog.value = false
      return
    }

    try {
      await deleteVoucher(voucherToDelete.value.id)
      showDeleteDialog.value = false
      
      // Clear selection nếu có
      if (selectedRows.value.length > 0) {
        selectedRows.value = selectedRows.value.filter(row => row.id !== voucherToDelete.value.id)
      }
      
      voucherToDelete.value = null
      deleteVoucherNumber.value = ''
      
      showSuccess('Xóa chứng từ thành công!')
      
      // Refresh data
      await loadVouchers()
      
      // Nếu đang ở chế độ edit, đóng form
      if (isEditing.value && editingVoucher.value?.id === voucherToDelete.value?.id) {
        handleCloseEdit()
      }
    } catch (err) {
      console.error('Error deleting voucher:', err)
      showDeleteDialog.value = false
      
      const errorMessage = err.response?.data?.message || 'Không thể xóa chứng từ. Vui lòng thử lại.'
      alert(errorMessage)
      
      voucherToDelete.value = null
      deleteVoucherNumber.value = ''
    }
  }

  const handleCancelDelete = () => {
    showDeleteDialog.value = false
    voucherToDelete.value = null
    deleteVoucherNumber.value = ''
  }

  const handlePrint = (record) => {
    console.log('Print voucher:', record)
    // TODO: Print voucher
  }

  const handleBulkDelete = () => {
    if (selectedRows.value.length === 0) {
      alert('Vui lòng chọn ít nhất một chứng từ để xóa.')
      return
    }

    // Nếu chọn 1 chứng từ → hiển thị dialog với số chứng từ
    if (selectedRows.value.length === 1) {
      const selectedVoucher = selectedRows.value[0]
      voucherToDelete.value = selectedVoucher
      deleteVoucherNumber.value = selectedVoucher.voucherNumber || ''
      showDeleteDialog.value = true
    } else {
      // Nếu chọn nhiều hơn 1 → hiển thị dialog với số lượng
      showMultiDeleteDialog.value = true
    }
  }

  const handleConfirmMultiDelete = async () => {
    try {
      // Lấy danh sách IDs từ selectedRows
      const ids = selectedRows.value
        .map(row => row.id)
        .filter(id => id != null)

      if (ids.length === 0) {
        showMultiDeleteDialog.value = false
        alert('Không tìm thấy ID của các chứng từ đã chọn.')
        return
      }

      // Gọi composable bulk delete
      const result = await bulkDeleteVouchers(ids)
      
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
      selectedRows.value = []
      showMultiDeleteDialog.value = false

      // Kiểm tra nếu có chứng từ không thể xóa (bất kể có xóa được chứng từ nào hay không)
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
        showSuccess(`Đã xóa thành công ${deletedCount} chứng từ!`)
      } else {
        // Không xóa được chứng từ nào (có thể do lỗi)
        alert('Không thể xóa các chứng từ. Vui lòng thử lại.')
      }

      // Refresh data
      await loadVouchers()
    } catch (err) {
      console.error('Error bulk deleting vouchers:', err)
      showMultiDeleteDialog.value = false
      
      const errorMessage = err.response?.data?.message || 'Không thể xóa các chứng từ. Vui lòng thử lại.'
      alert(errorMessage)
    }
  }

  const handleCancelMultiDelete = () => {
    showMultiDeleteDialog.value = false
  }

  const handleClosePartialDeleteResultDialog = () => {
    showPartialDeleteResultDialog.value = false
    partialDeleteResult.value = {
      deletedCount: 0,
      totalCount: 0,
      cannotDeleteCount: 0
    }
  }

  /**
   * Chuyển đến trang cụ thể trong pagination
   */
  const goToPage = async (page) => {
    if (page === '...' || page < 1 || page === pagination.value.currentPage) {
      return
    }
    pagination.value.currentPage = page
    await loadVouchers()
  }

  /**
   * Xử lý thay đổi số lượng bản ghi hiển thị trên mỗi trang
   */
  const handlePageSizeChange = async (newSize) => {
    pagination.value.pageSize = newSize
    pagination.value.currentPage = 1
    await loadVouchers()
  }

  /**
   * Xử lý input filter
   */
  const handleFilterInput = (key, event) => {
    filterValues.value = {
      ...filterValues.value,
      [key]: event.target.value
    }
    handleFilterChange(filterValues.value)
  }

  /**
   * Filter handler
   * Khi filter thay đổi, reset về trang 1 và tải lại dữ liệu
   */
  const handleFilterChange = async (newFilterValues) => {
    pagination.value.currentPage = 1
    await loadVouchers()
  }

  /**
   * Xử lý khi window resize để responsive
   */
  const handleResize = () => {
    windowWidth.value = window.innerWidth
    calculateTableScrollHeight()
    syncSummaryColumnWidths()
    syncFilterRowColumnWidths()
  }

  onMounted(async () => {
    await loadVouchers()
    
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

  // Watch filter changes
  watch(() => filterValues.value.voucherNumber, async () => {
    pagination.value.currentPage = 1
    await loadVouchers()
  })

  watch(() => filterValues.value.voucherDate, async () => {
    pagination.value.currentPage = 1
    await loadVouchers()
  })

  watch(() => filterValues.value.increaseDate, async () => {
    pagination.value.currentPage = 1
    await loadVouchers()
  })

  watch(() => filterValues.value.content, async () => {
    pagination.value.currentPage = 1
    await loadVouchers()
  })

  watch(() => filterValues.value.totalOriginalPrice, async () => {
    pagination.value.currentPage = 1
    await loadVouchers()
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
  /* Custom Header Styles */
  .increase-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 24px 0 0;
    height: 44px;
    background-color: #ffffff;
    border-bottom: 1px solid #e0e0e0;
    position: sticky;
    top: 0;
    z-index: 10;
    margin-bottom: 16px;
    margin-left: -24px;
    margin-right: -24px;
    width: calc(100% + 48px);
  }

  .increase-header__left {
    display: flex;
    align-items: center;
    gap: 16px;
    padding-left: 24px;
  }

  .increase-header__org {
    font-size: 14px;
    font-weight: 500;
    font-family: 'Roboto', sans-serif;
    color: #001031;
  }

  .increase-header__year-label {
    font-size: 13px;
    color: #666666;
    font-family: 'Roboto', sans-serif;
  }

  .increase-header__year-wrapper {
    display: flex;
    align-items: center;
    height: 32px;
    background-color: transparent;
    overflow: visible;
    gap: 0;
    border-radius: 4px;
    border: 1px solid #d9d9d9;
  }

  .increase-header__year-input {
    height: 100%;
    border: none !important;
    background-color: transparent !important;
    width: 75px;
    flex: 0 0 auto;
    display: flex;
    align-items: center;
  }

  .increase-header__year-input :deep(.ant-input-number) {
    width: 100%;
    height: 100% !important;
    border: none !important;
    background-color: transparent !important;
    box-shadow: none !important;
    display: flex;
    align-items: center;
  }

  .increase-header__year-input :deep(.ant-input-number:hover .ant-input-number-handler-wrap),
  .increase-header__year-input :deep(.ant-input-number:not(:hover) .ant-input-number-handler-wrap) {
    opacity: 1 !important;
    visibility: visible !important;
  }

  .increase-header__year-input :deep(.ant-input-number-input-wrap) {
    height: 100%;
    display: flex;
    align-items: center;
    flex: 1;
  }

  .increase-header__year-input :deep(.ant-input-number-input) {
    height: 100% !important;
    border: none !important;
    background-color: transparent !important;
    font-size: 13px !important;
    font-weight: 600 !important;
    color: #333333 !important;
    font-family: 'Roboto', sans-serif !important;
    text-align: left !important;
    padding: 0 4px 0 12px !important;
    line-height: 32px !important;
  }

  .increase-header__year-input :deep(.ant-input-number-handler-wrap) {
    display: flex !important;
    flex-direction: column !important;
    border-left: none !important;
    background-color: transparent !important;
    width: auto !important;
    margin-left: 0 !important;
    gap: 0 !important;
    padding: 0 !important;
    opacity: 1 !important;
    visibility: visible !important;
    pointer-events: auto !important;
    z-index: 1 !important;
  }

  .increase-header__year-input :deep(.ant-input-number-handler) {
    background-color: transparent !important;
    border: none !important;
    height: 16px !important;
    width: 20px !important;
    padding: 0 !important;
    margin-left: 0 !important;
    margin-right: 0 !important;
    opacity: 1 !important;
    visibility: visible !important;
    cursor: pointer !important;
    pointer-events: auto !important;
    z-index: 1 !important;
  }

  .increase-header__year-input :deep(.ant-input-number-handler:first-child) {
    border-bottom: none !important;
    margin-bottom: -4px !important;
  }

  .increase-header__year-input :deep(.ant-input-number-handler:last-child) {
    border: none !important;
    margin-top: -8px !important
  }

  .increase-header__year-input :deep(.ant-input-number-handler:hover) {
    background-color: rgba(0, 0, 0, 0.05) !important;
    cursor: pointer !important;
  }

  .increase-header__year-input :deep(.ant-input-number-handler:active) {
    background-color: rgba(0, 0, 0, 0.1) !important;
    cursor: pointer !important;
  }

  .increase-header__year-input :deep(.ant-input-number-handler-up),
  .increase-header__year-input :deep(.ant-input-number-handler-down) {
    color: #333333 !important;
  }

  .increase-header__year-input :deep(.ant-input-number-handler-up .anticon),
  .increase-header__year-input :deep(.ant-input-number-handler-down .anticon) {
    display: none !important;
  }

  .increase-header__year-input :deep(.ant-input-number-handler-up),
  .increase-header__year-input :deep(.ant-input-number-handler-down) {
    cursor: pointer !important;
    pointer-events: auto !important;
    user-select: none !important;
    -webkit-user-select: none !important;
  }

  .increase-header__year-input :deep(.ant-input-number-handler-up) {
    position: relative;
  }

  .increase-header__year-input :deep(.ant-input-number-handler-up)::before {
    content: '';
    position: absolute;
    top: 40%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 16px;
    height: 16px;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M.3,2.882,2.886.292a1,1,0,0,1,1.41,0l2.59,2.59a1,1,0,0,1-.71,1.71H1A1,1,0,0,1,.3,2.882Z' transform='translate(8.41 9.704)' fill='%23333'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    background-position: center;
    pointer-events: none;
  }

  .increase-header__year-input :deep(.ant-input-number-handler-down) {
    position: relative;
  }

  .increase-header__year-input :deep(.ant-input-number-handler-down)::before {
    content: '';
    position: absolute;
    top: 60%;
    left: 50%;
    transform: translate(-50%, -50%);
    width: 16px;
    height: 16px;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24'%3E%3Cpath d='M.3,1.71,2.886,4.3a1,1,0,0,0,1.41,0l2.59-2.59A1,1,0,0,0,6.176,0H1A1,1,0,0,0,.3,1.71Z' transform='translate(8.41 9.704)' fill='%23333'/%3E%3C/svg%3E");
    background-size: contain;
    background-repeat: no-repeat;
    background-position: center;
    pointer-events: none;
  }

  .increase-header__right {
    display: flex;
    align-items: center;
    gap: 16px;
    flex: 1;
    justify-content: flex-end;
  }

  .increase-header__actions {
    display: flex;
    align-items: center;
    gap: 4px !important;
  }

  .increase-header__icon-btn {
    width: 40px;
    height: 40px;
    border: none;
    background-color: transparent;
    color: #666666;
    cursor: pointer;
    border-radius: 4px;
    transition: all 0.2s;
    padding: 0;
    margin: 0;
    flex-shrink: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    position: relative;
  }

  .increase-header__icon-btn svg {
    display: block;
    margin: 0;
    padding: 0;
    flex-shrink: 0;
    line-height: 1;
    width: 24px;
    height: 24px;
  }

  .increase-header__icon-btn:hover {
    background-color: #f5f5f5;
    color: #212121;
  }

  .increase-header__icon-btn--dropdown {
    margin-left: -24px !important;
  }

  .increase-header__icon-btn--dropdown:hover {
    background-color: transparent !important;
    color: #666666 !important;
  }

  .increase-view {
    padding: 0 24px 24px 24px;
    background-color: #f4f7ff;
    height: 100%;
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }

  .increase-view > :deep(.edit-voucher-form) {
    height: 100%;
    margin: -24px;
    border-radius: 0;
  }

  .increase-view__header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
  }

  .increase-view__title-section {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .increase-view__title {
    font-size: 24px;
    font-weight: 700;
    color: #001031;
    font-family: 'Roboto', sans-serif;
    margin: 0;
  }

  .increase-view__refresh-btn {
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

  .increase-view__refresh-btn:hover {
    background-color: #f5f5f5;
    color: #212121;
  }

  .increase-view__actions {
    display: flex;
    align-items: center;
    gap: 8px;
  }

  .increase-view__action-btn {
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

  .increase-view__action-btn--add {
    background-color: #1aa4c8;
    box-shadow: inset 0 2px 6px rgba(0, 0, 0, 0.16);
    width: 130px;
    height: 36px;
    color: #ffffff;
    justify-content: flex-start;
    font-size: 13px;
    font-weight: 500;
    font-family: 'Roboto', sans-serif;
    padding-left: 4px;
    border: 1px solid #1aa4c8;
  }

  .increase-view__action-btn--add svg {
    margin-right: -4px;
  }

  .increase-view__action-btn--add:hover {
    background-color: #ffffff !important;
    border: 1px solid #1aa4c8 !important;
    color: #1aa4c8 !important;
  }

  .increase-view__action-btn--add svg :deep(rect) {
    display: none;
  }

  .increase-view__action-btn--add svg :deep(path) {
    stroke: #ffffff;
    fill: none;
  }

  .increase-view__action-btn--add:hover svg :deep(path) {
    stroke: #1aa4c8;
    fill: none;
  }

  .increase-view__action-btn:not(.increase-view__action-btn--add):hover {
    background-color: #f5f5f5;
    color: #212121;
  }

  .increase-view__action-btn--delete svg :deep(rect) {
    display: none;
  }

  .increase-view__action-btn--delete svg :deep(path),
  .increase-view__action-btn--delete svg :deep(g) {
    stroke: #ff3131;
    fill: none;
  }

  .increase-view__action-btn--disabled {
    opacity: 0.4;
    cursor: not-allowed;
    pointer-events: none;
  }

  .increase-view__action-btn--disabled:hover {
    background-color: #ffffff !important;
    color: #666666 !important;
  }

  .increase-view__table {
    flex: 1;
    display: flex;
    flex-direction: column;
    min-height: 0;
  }

  .custom-table-wrapper {
    background-color: #f4f7ff;
    border-radius: 3px;
    border: 1px solid #e0e0e0;
    overflow: visible;
    margin-bottom: 17px;
    flex: 1;
    display: flex;
    flex-direction: column;
    min-height: 0;
    box-shadow: 0 3px 5px rgba(10, 83, 132, 0.161);
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
    border-top: 3px solid #e0e0e0;
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

  .voucher-number-link {
    color: #1aa4c8;
    cursor: pointer;
    text-decoration: underline;
  }

  .voucher-number-link:hover {
    color: #1588a8;
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

  .custom-summary-value {
    font-weight: 700;
    color: #001031;
    display: inline-block;
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

  .filter-equals-sign {
    position: absolute;
    left: 8px;
    color: #666666;
    font-size: 13px;
    font-family: 'Roboto', sans-serif;
    z-index: 1;
    pointer-events: none;
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
  
  </style>
