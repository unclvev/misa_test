import Http from './http.js'

/**
 * API service cho quản lý chứng từ ghi tăng
 */
const increaseVoucherApi = {
  /**
   * Lấy danh sách chứng từ ghi tăng có phân trang
   * @param {number} pageNumber - Số trang (bắt đầu từ 1)
   * @param {number} pageSize - Số lượng bản ghi mỗi trang
   * @returns {Promise} Response từ API
   */
  getAllVouchers(pageNumber = 1, pageSize = 20) {
    return Http.get('/IncreaseVouchers', {
      params: {
        pageNumber,
        pageSize
      }
    })
  },

  /**
   * Lọc chứng từ ghi tăng với các điều kiện
   * @param {Object} params - Các tham số lọc
   * @param {number} params.pageNumber - Số trang
   * @param {number} params.pageSize - Số lượng bản ghi
   * @param {string} params.searchText - Từ khóa tìm kiếm (số chứng từ, ghi chú)
   * @param {string} params.fromDate - Ngày bắt đầu (format: YYYY-MM-DD)
   * @param {string} params.toDate - Ngày kết thúc (format: YYYY-MM-DD)
   * @returns {Promise} Response từ API
   */
  filterVouchers({ pageNumber = 1, pageSize = 20, searchText = null, fromDate = null, toDate = null, totalOriginalPrice = null }) {
    const params = {
      pageNumber,
      pageSize
    }
    
    if (searchText) params.searchText = searchText
    if (fromDate) params.fromDate = fromDate
    if (toDate) params.toDate = toDate
    if (totalOriginalPrice != null) params.totalOriginalPrice = totalOriginalPrice

    return Http.get('/IncreaseVouchers/filter', { params })
  },

  /**
   * Lấy thông tin chứng từ theo ID
   * @param {number|string} id - ID của chứng từ
   * @returns {Promise} Response từ API
   */
  getVoucherById(id) {
    return Http.get(`/IncreaseVouchers/${id}`)
  },

  /**
   * Tạo chứng từ mới
   * @param {Object} voucherData - Dữ liệu chứng từ
   * @returns {Promise} Response từ API
   */
  createVoucher(voucherData) {
    return Http.post('/IncreaseVouchers', voucherData)
  },

  /**
   * Cập nhật chứng từ
   * @param {number|string} id - ID của chứng từ
   * @param {Object} voucherData - Dữ liệu chứng từ cập nhật
   * @returns {Promise} Response từ API
   */
  updateVoucher(id, voucherData) {
    return Http.put(`/IncreaseVouchers/${id}`, voucherData)
  },

  /**
   * Xóa chứng từ
   * @param {number|string} id - ID của chứng từ
   * @returns {Promise} Response từ API
   */
  deleteVoucher(id) {
    return Http.delete(`/IncreaseVouchers/${id}`)
  },

  /**
   * Xóa nhiều chứng từ
   * @param {Array<number|string>} ids - Mảng các ID chứng từ
   * @returns {Promise} Response từ API
   */
  bulkDeleteVouchers(ids) {
    return Http.delete('/IncreaseVouchers/bulk-delete', { data: ids })
  },

  /**
   * Kiểm tra số chứng từ đã tồn tại chưa
   * @param {string} voucherNo - Số chứng từ
   * @returns {Promise} Response từ API
   */
  checkVoucherNo(voucherNo) {
    return Http.get(`/IncreaseVouchers/check-voucher-no/${voucherNo}`)
  },

  /**
   * Lấy số chứng từ tiếp theo
   * @returns {Promise} Response từ API
   */
  getNextVoucherNo() {
    return Http.get('/IncreaseVouchers/next-voucher-no')
  },

  /**
   * Xóa chi tiết chứng từ (voucher detail)
   * @param {number|string} voucherId - ID của chứng từ
   * @param {number|string} detailId - ID của chi tiết chứng từ
   * @returns {Promise} Response từ API
   */
  deleteVoucherDetail(voucherId, detailId) {
    return Http.delete(`/IncreaseVouchers/${voucherId}/details/${detailId}`)
  }
}

export default increaseVoucherApi

