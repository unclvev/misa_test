import { ref } from 'vue'
import assetApi from '../api/assetApi.js'

/**
 * Composable để quản lý API calls liên quan đến tài sản
 * Chỉ chứa logic gọi API, không chứa logic UI
 */
export function useAssetApi() {
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
      assetCode: item.assetCode || '',
      assetName: item.assetName || '',
      assetType: item.typeName || '', // Map typeName -> assetType
      department: item.deptName || '', // Map deptName -> department
      quantity: item.quantity || 0,
      originalPrice: item.purchasePrice || 0, // Map purchasePrice -> originalPrice
      depreciation: item.accumulatedDepreciation || 0, // Map accumulatedDepreciation -> depreciation
      remainingValue: item.remainingValue || 0
    }))
  }

  /**
   * Load assets from API
   * @param {Object} params - Tham số để load assets
   * @param {number} params.currentPage - Trang hiện tại
   * @param {number} params.pageSize - Kích thước trang
   * @param {Object} params.filters - Bộ lọc
   * @param {string} params.filters.searchQuery - Từ khóa tìm kiếm
   * @param {number|string} params.filters.assetType - Mã loại tài sản
   * @param {string} params.filters.department - Mã bộ phận
   * @returns {Promise<Object>} Kết quả với assets và totalRecords
   */
  const loadAssets = async ({ currentPage, pageSize, filters = {} }) => {
    loading.value = true
    error.value = null
    
    try {
      let response
      
      const hasFilters = filters.searchQuery || filters.assetType || filters.department
      
      if (hasFilters) {
        // Use filter endpoint
        response = await assetApi.filterAssets({
          pageNumber: currentPage,
          pageSize: pageSize,
          searchText: filters.searchQuery || null,
          departmentCode: filters.department || null,
          typeCode: filters.assetType 
            ? (typeof filters.assetType === 'number' 
                ? filters.assetType 
                : parseInt(filters.assetType)) 
            : null
        })
      } else {
        // Use getAll endpoint
        response = await assetApi.getAllAssets(currentPage, pageSize)
      }

      const result = response.data
      const mappedAssets = mapApiToFrontend(result.data || [], currentPage, pageSize)
      
      return {
        assets: mappedAssets,
        totalRecords: result.totalRecords || 0,
        pageNumber: result.pageNumber || currentPage,
        pageSize: result.pageSize || pageSize
      }
    } catch (err) {
      console.error('Error loading assets:', err)
      
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
        assets: [],
        totalRecords: 0,
        pageNumber: currentPage,
        pageSize: pageSize
      }
    } finally {
      loading.value = false
    }
  }

  /**
   * Refresh data (reload với filters hiện tại)
   * @param {Object} params - Tham số giống loadAssets
   * @returns {Promise<Object>} Kết quả với assets và totalRecords
   */
  const refreshData = async (params) => {
    return await loadAssets(params)
  }

  /**
   * Lấy danh sách tài sản chưa có trong chứng từ (available for voucher)
   * @param {Object} params - Tham số để load assets
   * @param {number} params.pageNumber - Trang hiện tại
   * @param {number} params.pageSize - Kích thước trang
   * @param {Object} params.filters - Bộ lọc
   * @param {string} params.filters.searchText - Từ khóa tìm kiếm
   * @param {string} params.filters.departmentCode - Mã bộ phận
   * @returns {Promise<Object>} Kết quả với assets và totalRecords
   */
  const loadAvailableAssetsForVoucher = async ({ pageNumber, pageSize, filters = {} }) => {
    loading.value = true
    error.value = null
    
    try {
      const response = await assetApi.filterAvailableForVoucher({
        pageNumber,
        pageSize,
        searchText: filters.searchText || null,
        departmentCode: filters.departmentCode || null,
        typeCode: null
      })

      const result = response.data
      // Map API response to frontend format cho popup
      const mappedAssets = (result.data || []).map((asset, index) => ({
        id: asset.id,
        stt: (pageNumber - 1) * pageSize + index + 1,
        assetCode: asset.assetCode || '',
        assetName: asset.assetName || '',
        department: asset.deptName || '',
        originalPrice: asset.purchasePrice || 0
      }))
      
      return {
        assets: mappedAssets,
        totalRecords: result.totalRecords || 0,
        pageNumber: result.pageNumber || pageNumber,
        pageSize: result.pageSize || pageSize
      }
    } catch (err) {
      console.error('Error loading available assets:', err)
      
      if (err.code === 'ERR_NETWORK' || err.message?.includes('Network Error')) {
        error.value = 'Không thể kết nối đến server. Vui lòng kiểm tra xem backend API có đang chạy không.'
      } else if (err.response) {
        error.value = err.response.data?.message || err.response.data?.error || `Lỗi ${err.response.status}: ${err.response.statusText}`
      } else if (err.request) {
        error.value = 'Không nhận được phản hồi từ server. Vui lòng kiểm tra kết nối mạng.'
      } else {
        error.value = err.message || 'Đã xảy ra lỗi khi tải dữ liệu'
      }
      
      return {
        assets: [],
        totalRecords: 0,
        pageNumber,
        pageSize
      }
    } finally {
      loading.value = false
    }
  }

  /**
   * Lấy mã tài sản tiếp theo
   * @returns {Promise<string>} Mã tài sản tiếp theo
   */
  const getNextAssetCode = async () => {
    try {
      const response = await assetApi.getNextAssetCode()
      return response.data?.assetCode || ''
    } catch (err) {
      console.error('Error getting next asset code:', err)
      return ''
    }
  }

  /**
   * Lấy danh sách bộ phận
   * @returns {Promise<Array>} Danh sách bộ phận
   */
  const getDepartments = async () => {
    try {
      const response = await assetApi.getDepartments()
      return response.data || []
    } catch (err) {
      console.error('Error loading departments:', err)
      return []
    }
  }

  /**
   * Lấy danh sách loại tài sản
   * @returns {Promise<Array>} Danh sách loại tài sản
   */
  const getAssetTypes = async () => {
    try {
      const response = await assetApi.getAssetTypes()
      return response.data || []
    } catch (err) {
      console.error('Error loading asset types:', err)
      return []
    }
  }

  /**
   * Lấy thông tin tài sản theo ID
   * @param {number|string} id - ID của tài sản
   * @returns {Promise<Object|null>} Thông tin tài sản
   */
  const getAssetById = async (id) => {
    try {
      const response = await assetApi.getAssetById(id)
      return response.data || null
    } catch (err) {
      console.error('Error loading asset:', err)
      return null
    }
  }

  /**
   * Tạo tài sản mới
   * @param {Object} assetData - Dữ liệu tài sản
   * @returns {Promise<Object>} Tài sản đã tạo
   */
  const createAsset = async (assetData) => {
    try {
      const response = await assetApi.createAsset(assetData)
      return response.data || null
    } catch (err) {
      console.error('Error creating asset:', err)
      throw err
    }
  }

  /**
   * Cập nhật tài sản
   * @param {number|string} id - ID của tài sản
   * @param {Object} assetData - Dữ liệu tài sản cập nhật
   * @returns {Promise<Object>} Tài sản đã cập nhật
   */
  const updateAsset = async (id, assetData) => {
    try {
      const response = await assetApi.updateAsset(id, assetData)
      return response.data || null
    } catch (err) {
      console.error('Error updating asset:', err)
      throw err
    }
  }

  /**
   * Xóa tài sản
   * @param {number|string} id - ID của tài sản
   * @returns {Promise<boolean>} Kết quả xóa
   */
  const deleteAsset = async (id) => {
    try {
      await assetApi.deleteAsset(id)
      return true
    } catch (err) {
      console.error('Error deleting asset:', err)
      throw err
    }
  }

  /**
   * Xóa nhiều tài sản
   * @param {Array<number|string>} ids - Mảng các ID tài sản
   * @returns {Promise<Object>} Kết quả xóa
   */
  const bulkDeleteAssets = async (ids) => {
    try {
      const response = await assetApi.bulkDeleteAssets(ids)
      return response.data || { deleted: 0, total: ids.length, cannotDelete: 0 }
    } catch (err) {
      console.error('Error bulk deleting assets:', err)
      throw err
    }
  }

  return {
    // State
    loading,
    error,
    
    // Methods
    loadAssets,
    refreshData,
    mapApiToFrontend,
    loadAvailableAssetsForVoucher,
    getNextAssetCode,
    getDepartments,
    getAssetTypes,
    getAssetById,
    createAsset,
    updateAsset,
    deleteAsset,
    bulkDeleteAssets
  }
}

