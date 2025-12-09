<template>
  <div class="edit-increase-voucher-view">
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

    <div v-if="loading" class="loading-overlay">
      <div class="loading-spinner">Đang tải...</div>
    </div>
    <div v-else-if="error" class="error-message">
      <strong>Lỗi:</strong> {{ error }}
      <button class="error-retry-btn" @click="$router.push('/increase')">Quay lại</button>
    </div>
    <EditIncreaseVoucherForm
      v-else
      :voucher="voucher"
      @close="handleClose"
      @save="handleSave"
      @delete="handleDelete"
      @cancel="handleCancel"
    />

    <!-- Dialogs -->
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

    <ConfirmDeleteVoucherDialog
      v-model="showDeleteDialog"
      :voucher-number="voucher?.voucherNumber || ''"
      @confirm="handleConfirmDelete"
      @cancel="handleCancelDelete"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { InputNumber as AInputNumber } from 'ant-design-vue'
import EditIncreaseVoucherForm from '../components/EditIncreaseVoucherForm.vue'
import { useIncreaseVoucherApi } from '../composables/useIncreaseVoucherApi'
import { useToast } from '../composables/useToast'
import ConfirmCancelDialog from '../components/dialogs/ConfirmCancelDialog.vue'
import ConfirmSaveDialog from '../components/dialogs/ConfirmSaveDialog.vue'
import ConfirmDeleteVoucherDialog from '../components/dialogs/ConfirmDeleteVoucherDialog.vue'

const route = useRoute()
const router = useRouter()

// Composables
const { showSuccess } = useToast()
const { getVoucherById, updateVoucher, deleteVoucher } = useIncreaseVoucherApi()

// Year selection for header
const currentYear = new Date().getFullYear()
const selectedYear = ref(currentYear)

// Lấy voucher ID từ route params
const voucherId = route.params.id

// Loading và error states
const loading = ref(false)
const error = ref(null)

// Voucher data
const voucher = ref({
  id: voucherId,
  voucherNumber: '',
  voucherDate: '',
  increaseDate: '',
  content: ''
})

// Dialog states
const showCancelDialog = ref(false)
const showSaveDialog = ref(false)
const showDeleteDialog = ref(false)
const originalVoucher = ref(null)

/**
 * Format date từ API (YYYY-MM-DD) sang format hiển thị (YYYY-MM-DD cho date picker)
 */
const formatDateForInput = (dateValue) => {
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
  
  return ''
}

/**
 * Map API response to frontend format
 */
const mapApiToFrontend = (apiData) => {
  return {
    id: apiData.id,
    voucherNumber: apiData.voucherNo || apiData.voucherNumber || '',
    voucherDate: formatDateForInput(apiData.voucherDate),
    increaseDate: formatDateForInput(apiData.increaseDate),
    content: apiData.note || '',
    details: apiData.details || []
  }
}

// Load voucher data từ API
onMounted(async () => {
  if (!voucherId) {
    error.value = 'Không tìm thấy ID chứng từ'
    return
  }

  loading.value = true
  error.value = null

  try {
    const voucherData = await getVoucherById(voucherId)
    
    if (voucherData) {
      voucher.value = mapApiToFrontend(voucherData)
      // Lưu bản gốc để so sánh thay đổi
      originalVoucher.value = JSON.parse(JSON.stringify(voucher.value))
    } else {
      error.value = 'Không tìm thấy chứng từ'
    }
  } catch (err) {
    console.error('Error loading voucher:', err)
    
    if (err.code === 'ERR_NETWORK' || err.message?.includes('Network Error')) {
      error.value = 'Không thể kết nối đến server. Vui lòng kiểm tra xem backend API có đang chạy không.'
    } else if (err.response) {
      if (err.response.status === 404) {
        error.value = 'Không tìm thấy chứng từ với ID này'
      } else {
        error.value = err.response.data?.message || err.response.data?.error || `Lỗi ${err.response.status}: ${err.response.statusText}`
      }
    } else if (err.request) {
      error.value = 'Không nhận được phản hồi từ server. Vui lòng kiểm tra kết nối mạng.'
    } else {
      error.value = err.message || 'Đã xảy ra lỗi khi tải dữ liệu'
    }
  } finally {
    loading.value = false
  }
})

const handleClose = () => {
  // Kiểm tra có thay đổi không
  if (hasChanges()) {
    // Có thay đổi → hiển thị dialog xác nhận lưu
    showSaveDialog.value = true
  } else {
    // Không có thay đổi → hiển thị dialog xác nhận hủy
    showCancelDialog.value = true
  }
}

const handleCancel = () => {
  // Áp dụng cùng logic như handleClose/AssetList: kiểm tra thay đổi
  if (hasChanges()) {
    showSaveDialog.value = true
  } else {
    showCancelDialog.value = true
  }
}

/**
 * Kiểm tra xem người dùng đã có thay đổi dữ liệu trong form hay chưa
 */
const hasChanges = () => {
  if (!originalVoucher.value) {
    // Nếu là thêm mới, kiểm tra có dữ liệu người dùng nhập không
    const current = voucher.value
    const hasUserInput = !!(
      (current.voucherNumber && current.voucherNumber.trim() !== '') ||
      (current.voucherDate && current.voucherDate !== '') ||
      (current.increaseDate && current.increaseDate !== '') ||
      (current.content && current.content.trim() !== '') ||
      (current.details && current.details.length > 0)
    )
    return hasUserInput
  }
  
  // So sánh với bản gốc
  const current = voucher.value
  const original = originalVoucher.value
  
  return (
    (current.voucherNumber || '') !== (original.voucherNumber || '') ||
    (current.voucherDate || '') !== (original.voucherDate || '') ||
    (current.increaseDate || '') !== (original.increaseDate || '') ||
    (current.content || '') !== (original.content || '') ||
    JSON.stringify(current.details || []) !== JSON.stringify(original.details || [])
  )
}

const handleConfirmCancel = () => {
  showCancelDialog.value = false
  router.push('/increase')
}

const handleCancelCancel = () => {
  showCancelDialog.value = false
}

const handleConfirmSave = async () => {
  showSaveDialog.value = false
  await handleSave(voucher.value)
}

const handleDontSave = () => {
  showSaveDialog.value = false
  router.push('/increase')
}

const handleCancelSave = () => {
  showSaveDialog.value = false
}

const handleSave = async (data) => {
  if (!voucherId) {
    error.value = 'Không tìm thấy ID chứng từ'
    return
  }

  loading.value = true
  error.value = null

  try {
    // Convert frontend format to API format
    const apiData = {
      voucherNo: data.voucherNumber || '',
      voucherDate: data.voucherDate || '',
      increaseDate: data.increaseDate || '',
      note: data.content || '',
      details: (data.assets || []).map(asset => ({
        assetId: asset.id || asset.assetId,
        createdAt: new Date().toISOString()
      }))
    }

    await updateVoucher(voucherId, apiData)
    
    showSuccess('Cập nhật chứng từ thành công!')
    
    // Navigate back to list after successful save
    setTimeout(() => {
      router.push('/increase')
    }, 1500)
  } catch (err) {
    console.error('Error saving voucher:', err)
    
    if (err.code === 'ERR_NETWORK' || err.message?.includes('Network Error')) {
      error.value = 'Không thể kết nối đến server. Vui lòng kiểm tra xem backend API có đang chạy không.'
    } else if (err.response) {
      error.value = err.response.data?.message || err.response.data?.error || `Lỗi ${err.response.status}: ${err.response.statusText}`
      // Show error to user (you might want to use a toast/notification here)
      alert(error.value)
    } else if (err.request) {
      error.value = 'Không nhận được phản hồi từ server. Vui lòng kiểm tra kết nối mạng.'
      alert(error.value)
    } else {
      error.value = err.message || 'Đã xảy ra lỗi khi lưu chứng từ'
      alert(error.value)
    }
  } finally {
    loading.value = false
  }
}

const handleDelete = () => {
  // Hiển thị dialog xác nhận xóa
  showDeleteDialog.value = true
}

const handleConfirmDelete = async () => {
  if (!voucherId) {
    error.value = 'Không tìm thấy ID chứng từ'
    showDeleteDialog.value = false
    return
  }

  loading.value = true
  error.value = null
  showDeleteDialog.value = false

  try {
    await deleteVoucher(voucherId)
    
    showSuccess('Xóa chứng từ thành công!')
    
    // Navigate back to list after successful delete
    setTimeout(() => {
      router.push('/increase')
    }, 1500)
  } catch (err) {
    console.error('Error deleting voucher:', err)
    
    if (err.code === 'ERR_NETWORK' || err.message?.includes('Network Error')) {
      error.value = 'Không thể kết nối đến server. Vui lòng kiểm tra xem backend API có đang chạy không.'
    } else if (err.response) {
      if (err.response.status === 404) {
        error.value = 'Không tìm thấy chứng từ với ID này'
      } else {
        error.value = err.response.data?.message || err.response.data?.error || `Lỗi ${err.response.status}: ${err.response.statusText}`
      }
    } else if (err.request) {
      error.value = 'Không nhận được phản hồi từ server. Vui lòng kiểm tra kết nối mạng.'
    } else {
      error.value = err.message || 'Đã xảy ra lỗi khi xóa chứng từ'
    }
    
    alert(error.value)
  } finally {
    loading.value = false
  }
}

const handleCancelDelete = () => {
  showDeleteDialog.value = false
}
</script>

<style scoped>
/* Custom Header Styles */
.increase-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  height: 44px;
  background-color: #ffffff;
  border-bottom: 1px solid #e0e0e0;
  position: sticky;
  top: 0;
  z-index: 10;
}

.increase-header__left {
  display: flex;
  align-items: center;
  gap: 16px;
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
  background-color: #e6f7ff;
  overflow: visible;
  gap: 0;
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

.edit-increase-voucher-view {
  height: 100%;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  position: relative;
  padding: 0 24px 24px;
  background-color: #f4f7ff;
}

.edit-increase-voucher-view > :deep(.edit-voucher-form) {
  flex: 1;
  margin: -24px;
  border-radius: 0;
  background-color: transparent;
  padding: 24px;
}

.edit-increase-voucher-view :deep(.custom-table-wrapper) {
  background-color: #ffffff;
  border-radius: 6px;
  border: none;
  overflow: visible;
  margin-bottom: 17px;
  flex: 1;
  display: flex;
  flex-direction: column;
  min-height: 0;
  box-shadow: 0 3px 10px rgba(0, 0, 0, 0.16);
  position: relative;
}

.edit-increase-voucher-view :deep(.custom-ant-table .ant-table-container) {
  border-radius: 6px 6px 0 0;
  border: 0.5px solid #afafaf;
  border-bottom: none;
}

.edit-increase-voucher-view :deep(.custom-ant-table .ant-table) {
  border-radius: 6px 6px 0 0;
  border: none;
}

.edit-increase-voucher-view :deep(.custom-summary-row) {
  border: 0.5px solid #afafaf;
  border-top: 0.5px solid #afafaf;
  border-radius: 0 0 6px 6px;
  margin-top: 0;
}

.edit-increase-voucher-view :deep(.custom-summary-table) {
  border-radius: 0 0 6px 6px;
}

.edit-increase-voucher-view :deep(.custom-summary-pagination) {
  border-top: 0.5px solid #afafaf;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: rgba(255, 255, 255, 0.9);
  z-index: 1000;
}

.loading-spinner {
  font-size: 16px;
  color: #1aa4c8;
  font-family: 'Roboto', sans-serif;
}

.error-message {
  padding: 24px;
  background-color: #fff3cd;
  border: 1px solid #ffc107;
  border-radius: 4px;
  color: #856404;
  font-family: 'Roboto', sans-serif;
  display: flex;
  align-items: center;
  gap: 16px;
}

.error-retry-btn {
  padding: 8px 16px;
  background-color: #1aa4c8;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-family: 'Roboto', sans-serif;
  font-size: 14px;
  transition: background-color 0.2s;
}

.error-retry-btn:hover {
  background-color: #1589a8;
}
</style>

