import Http from './http.js'

/**
 * API service cho quản lý tài sản
 */
const assetApi = {
  /**
   * Lấy danh sách tài sản có phân trang
   * @param {number} pageNumber - Số trang (bắt đầu từ 1)
   * @param {number} pageSize - Số lượng bản ghi mỗi trang
   * @returns {Promise} Response từ API
   */
  getAllAssets(pageNumber = 1, pageSize = 20) {
    return Http.get('/Assets', {
      params: {
        pageNumber,
        pageSize
      }
    })
  },

  /**
   * Lọc tài sản với các điều kiện
   * @param {Object} params - Các tham số lọc
   * @param {number} params.pageNumber - Số trang
   * @param {number} params.pageSize - Số lượng bản ghi
   * @param {string} params.searchText - Từ khóa tìm kiếm
   * @param {string} params.departmentCode - Mã bộ phận
   * @param {number} params.typeCode - Mã loại tài sản
   * @returns {Promise} Response từ API
   */
  filterAssets({ pageNumber = 1, pageSize = 20, searchText = null, departmentCode = null, typeCode = null }) {
    const params = {
      pageNumber,
      pageSize
    }
    
    if (searchText) params.searchText = searchText
    if (departmentCode) params.departmentCode = departmentCode
    if (typeCode) params.typeCode = typeCode

    return Http.get('/Assets/filter', { params })
  },

  /**
   * Lấy thông tin tài sản theo ID
   * @param {number|string} id - ID của tài sản
   * @returns {Promise} Response từ API
   */
  getAssetById(id) {
    return Http.get(`/Assets/${id}`)
  },

  /**
   * Tạo tài sản mới
   * @param {Object} assetData - Dữ liệu tài sản
   * @returns {Promise} Response từ API
   */
  createAsset(assetData) {
    return Http.post('/Assets', assetData)
  },

  /**
   * Cập nhật tài sản
   * @param {number|string} id - ID của tài sản
   * @param {Object} assetData - Dữ liệu tài sản cập nhật
   * @returns {Promise} Response từ API
   */
  updateAsset(id, assetData) {
    return Http.put(`/Assets/${id}`, assetData)
  },

  /**
   * Xóa tài sản
   * @param {number|string} id - ID của tài sản
   * @returns {Promise} Response từ API
   */
  deleteAsset(id) {
    return Http.delete(`/Assets/${id}`)
  },

  /**
   * Xóa nhiều tài sản
   * @param {Array<number|string>} ids - Mảng các ID tài sản
   * @returns {Promise} Response từ API
   */
  bulkDeleteAssets(ids) {
    return Http.delete('/Assets/bulk-delete', { data: ids })
  },

  /**
   * Kiểm tra mã tài sản đã tồn tại chưa
   * @param {string} assetCode - Mã tài sản
   * @returns {Promise} Response từ API
   */
  checkAssetCode(assetCode) {
    return Http.get(`/Assets/check-code/${assetCode}`)
  },

  /**
   * Lấy danh sách tất cả bộ phận
   * @returns {Promise} Response từ API
   */
  getDepartments() {
    return Http.get('/Assets/departments')
  },

  /**
   * Lấy danh sách tất cả loại tài sản
   * @returns {Promise} Response từ API
   */
  getAssetTypes() {
    return Http.get('/Assets/asset-types')
  },

  /**
   * Lấy mã tài sản tiếp theo (để preview khi thêm mới)
   * @returns {Promise} Response từ API
   */
  getNextAssetCode() {
    return Http.get('/Assets/next-asset-code')
  },

  /**
   * Lọc tài sản chưa có trong chứng từ phái sinh (chưa có trong asset_increase_voucher_details)
   * @param {Object} params - Các tham số lọc
   * @param {number} params.pageNumber - Số trang
   * @param {number} params.pageSize - Số lượng bản ghi
   * @param {string} params.searchText - Từ khóa tìm kiếm
   * @param {string} params.departmentCode - Mã bộ phận
   * @param {number} params.typeCode - Mã loại tài sản
   * @returns {Promise} Response từ API
   */
  filterAvailableForVoucher({ pageNumber = 1, pageSize = 20, searchText = null, departmentCode = null, typeCode = null }) {
    const params = {
      pageNumber,
      pageSize
    }
    
    if (searchText) params.searchText = searchText
    if (departmentCode) params.departmentCode = departmentCode
    if (typeCode) params.typeCode = typeCode

    return Http.get('/Assets/available-for-voucher', { params })
  }
}

export default assetApi

