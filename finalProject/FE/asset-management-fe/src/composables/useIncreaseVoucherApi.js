import { ref } from 'vue'
import increaseVoucherApi from '../api/increaseVoucherApi.js'

/**
 * Composable để quản lý API calls liên quan đến chứng từ ghi tăng
 * Chỉ chứa logic gọi API, không chứa logic UI
 */
export function useIncreaseVoucherApi() {
  // State
  const loading = ref(false)
  const error = ref(null)

  /**
   * Map API data to frontend format
   * @param {Array} apiData - Dữ liệu từ API
   * @param {number} currentPage - Trang hiện tại
   * @param {number} pageSize - Kích thước trang
   * @returns {Array} Dữ liệu đã được map
   */
  const mapApiToFrontend = (apiData, currentPage, pageSize) => {
    return apiData.map((item, index) => ({
      id: item.id,
      stt: (currentPage - 1) * pageSize + index + 1,
      voucherNumber: item.voucherNo || item.voucherNumber || '',
      voucherDate: formatDateForDisplay(item.voucherDate),
      increaseDate: formatDateForDisplay(item.increaseDate),
      // Backend đã tính totalOriginalPrice trực tiếp trong SQL từ asset.purchasePrice
      // Handle cả camelCase (mặc định) và PascalCase (nếu có)
      totalOriginalPrice: item.totalOriginalPrice || item.TotalOriginalPrice || 0,
      content: item.note || ''
    }))
  }

  /**
   * Format date từ API (DateOnly string hoặc Date) sang format hiển thị (DD/MM/YYYY)
   * @param {string|Date} dateValue - Giá trị ngày từ API
   * @returns {string} Ngày đã format
   */
  const formatDateForDisplay = (dateValue) => {
    if (!dateValue) return ''
    
    let date
    if (typeof dateValue === 'string') {
      // Nếu là string, parse thành Date
      date = new Date(dateValue)
    } else if (dateValue instanceof Date) {
      date = dateValue
    } else {
      return ''
    }
    
    if (isNaN(date.getTime())) return ''
    
    const day = String(date.getDate()).padStart(2, '0')
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const year = date.getFullYear()
    
    return `${day}/${month}/${year}`
  }

  /**
   * Load vouchers from API
   * @param {Object} params - Tham số để load vouchers
   * @param {number} params.currentPage - Trang hiện tại
   * @param {number} params.pageSize - Kích thước trang
   * @param {Object} params.filters - Bộ lọc
   * @param {string} params.filters.voucherNumber - Số chứng từ
   * @param {string} params.filters.voucherDate - Ngày chứng từ
   * @param {string} params.filters.increaseDate - Ngày ghi tăng
   * @param {string} params.filters.content - Nội dung/ghi chú
   * @returns {Promise<Object>} Kết quả với vouchers và totalRecords
   */
  const loadVouchers = async ({ currentPage, pageSize, filters = {} }) => {
    loading.value = true
    error.value = null
    
    try {
      let response
      const {
        voucherNumber,
        voucherDate,
        increaseDate,
        content,
        totalOriginalPrice: totalOriginalPriceFilterRaw
      } = filters
      const totalOriginalPriceFilter = (totalOriginalPriceFilterRaw || '').trim()
      
      // Kiểm tra có filter backend hỗ trợ không (loại trừ tổng nguyên giá)
      const hasServerFilters = voucherNumber || 
                        voucherDate || 
                        increaseDate || 
                        content
      
      if (hasServerFilters) {
        // Use filter endpoint
        // Tạo searchText từ các filter
        const searchTextParts = []
        if (voucherNumber) searchTextParts.push(voucherNumber)
        if (content) searchTextParts.push(content)
        const searchText = searchTextParts.length > 0 ? searchTextParts.join(' ') : null
        
        // Parse dates
        let fromDate = null
        let toDate = null
        
        // Nếu có voucherDate hoặc increaseDate, dùng làm fromDate/toDate
        if (voucherDate) {
          const parsedDate = parseDateFilter(voucherDate)
          if (parsedDate) {
            fromDate = parsedDate
            toDate = parsedDate // Cùng một ngày
          }
        } else if (increaseDate) {
          const parsedDate = parseDateFilter(increaseDate)
          if (parsedDate) {
            fromDate = parsedDate
            toDate = parsedDate
          }
        }

        response = await increaseVoucherApi.filterVouchers({
          pageNumber: currentPage,
          pageSize: pageSize,
          searchText: searchText,
          fromDate: fromDate,
          toDate: toDate
        })
      } else {
        // Use getAll endpoint
        response = await increaseVoucherApi.getAllVouchers(currentPage, pageSize)
      }

      const result = response.data
      let mappedVouchers = mapApiToFrontend(result.data || [], currentPage, pageSize)
      
      // Frontend filter for totalOriginalPrice (backend chưa hỗ trợ)
      if (totalOriginalPriceFilter) {
        const normalizedFilter = totalOriginalPriceFilter.replace(/[^\d]/g, '')
        if (normalizedFilter !== '') {
          mappedVouchers = mappedVouchers.filter(voucher => {
            const price = voucher.totalOriginalPrice ?? 0
            const priceString = String(price).replace(/[^\d]/g, '')
            return priceString === normalizedFilter
          })
        } else {
          // Nếu input không phải số hợp lệ, trả về rỗng
          mappedVouchers = []
        }
      }
      
      // Backend đã tính totalOriginalPrice trực tiếp trong SQL query bằng subquery
      // Tính từ SUM(asset.purchase_price) của tất cả assets trong voucher details
      // Không cần load chi tiết từng voucher nữa, giúp tăng hiệu suất đáng kể
      
      return {
        vouchers: mappedVouchers,
        totalRecords: totalOriginalPriceFilter ? mappedVouchers.length : (result.totalRecords || 0),
        pageNumber: result.pageNumber || currentPage,
        pageSize: result.pageSize || pageSize
      }
    } catch (err) {
      console.error('Error loading vouchers:', err)
      
      // Handle different error types
      if (err.code === 'ERR_NETWORK' || err.message?.includes('Network Error')) {
        error.value = 'Không thể kết nối đến server. Vui lòng kiểm tra xem backend API có đang chạy không.'
      } else if (err.response) {
        // Server responded with error status
        error.value = err.response.data?.message || err.response.data?.error || `Lỗi ${err.response.status}: ${err.response.statusText}`
      } else if (err.request) {
        // Request was made but no response received
        error.value = 'Không nhận được phản hồi từ server. Vui lòng kiểm tra kết nối mạng.'
      } else {
        error.value = err.message || 'Đã xảy ra lỗi khi tải dữ liệu'
      }
      
      return {
        vouchers: [],
        totalRecords: 0,
        pageNumber: currentPage,
        pageSize: pageSize
      }
    } finally {
      loading.value = false
    }
  }

  /**
   * Parse date filter từ string (có thể là DD/MM/YYYY hoặc YYYY-MM-DD)
   * @param {string} dateStr - Chuỗi ngày
   * @returns {string|null} Ngày format YYYY-MM-DD hoặc null
   */
  const parseDateFilter = (dateStr) => {
    if (!dateStr || dateStr.trim() === '') return null
    
    // Thử parse DD/MM/YYYY
    const ddmmyyyy = dateStr.match(/^(\d{1,2})\/(\d{1,2})\/(\d{4})$/)
    if (ddmmyyyy) {
      const [, day, month, year] = ddmmyyyy
      return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`
    }
    
    // Thử parse YYYY-MM-DD
    const yyyymmdd = dateStr.match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/)
    if (yyyymmdd) {
      return dateStr
    }
    
    return null
  }

  /**
   * Refresh data (reload với filters hiện tại)
   * @param {Object} params - Tham số giống loadVouchers
   * @returns {Promise<Object>} Kết quả với vouchers và totalRecords
   */
  const refreshData = async (params) => {
    return await loadVouchers(params)
  }

  /**
   * Lấy số chứng từ tiếp theo
   * @returns {Promise<string>} Số chứng từ tiếp theo
   */
  const getNextVoucherNo = async () => {
    try {
      const response = await increaseVoucherApi.getNextVoucherNo()
      return response.data?.voucherNo || ''
    } catch (err) {
      console.error('Error getting next voucher number:', err)
      return ''
    }
  }

  /**
   * Lấy thông tin chứng từ theo ID
   * @param {number|string} id - ID của chứng từ
   * @returns {Promise<Object|null>} Thông tin chứng từ
   */
  const getVoucherById = async (id) => {
    try {
      const response = await increaseVoucherApi.getVoucherById(id)
      return response.data || null
    } catch (err) {
      console.error('Error loading voucher:', err)
      return null
    }
  }

  /**
   * Tạo chứng từ mới
   * @param {Object} voucherData - Dữ liệu chứng từ
   * @returns {Promise<Object>} Chứng từ đã tạo
   */
  const createVoucher = async (voucherData) => {
    try {
      const response = await increaseVoucherApi.createVoucher(voucherData)
      return response.data || null
    } catch (err) {
      console.error('Error creating voucher:', err)
      throw err
    }
  }

  /**
   * Cập nhật chứng từ
   * @param {number|string} id - ID của chứng từ
   * @param {Object} voucherData - Dữ liệu chứng từ cập nhật
   * @returns {Promise<Object>} Chứng từ đã cập nhật
   */
  const updateVoucher = async (id, voucherData) => {
    try {
      const response = await increaseVoucherApi.updateVoucher(id, voucherData)
      return response.data || null
    } catch (err) {
      console.error('Error updating voucher:', err)
      throw err
    }
  }

  /**
   * Xóa chứng từ
   * @param {number|string} id - ID của chứng từ
   * @returns {Promise<boolean>} Kết quả xóa
   */
  const deleteVoucher = async (id) => {
    try {
      await increaseVoucherApi.deleteVoucher(id)
      return true
    } catch (err) {
      console.error('Error deleting voucher:', err)
      throw err
    }
  }

  /**
   * Xóa nhiều chứng từ
   * @param {Array<number|string>} ids - Mảng các ID chứng từ
   * @returns {Promise<Object>} Kết quả xóa
   */
  const bulkDeleteVouchers = async (ids) => {
    try {
      const response = await increaseVoucherApi.bulkDeleteVouchers(ids)
      return response.data || { deletedCount: 0, totalCount: ids.length, cannotDeleteCount: 0 }
    } catch (err) {
      console.error('Error bulk deleting vouchers:', err)
      throw err
    }
  }

  /**
   * Xóa chi tiết chứng từ
   * @param {number|string} voucherId - ID của chứng từ
   * @param {number|string} detailId - ID của chi tiết
   * @returns {Promise<boolean>} Kết quả xóa
   */
  const deleteVoucherDetail = async (voucherId, detailId) => {
    try {
      await increaseVoucherApi.deleteVoucherDetail(voucherId, detailId)
      return true
    } catch (err) {
      console.error('Error deleting voucher detail:', err)
      throw err
    }
  }

  return {
    // State
    loading,
    error,
    
    // Methods
    loadVouchers,
    refreshData,
    mapApiToFrontend,
    getNextVoucherNo,
    getVoucherById,
    createVoucher,
    updateVoucher,
    deleteVoucher,
    bulkDeleteVouchers,
    deleteVoucherDetail
  }
}

