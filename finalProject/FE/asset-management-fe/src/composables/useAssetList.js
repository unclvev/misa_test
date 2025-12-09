import { ref, computed } from 'vue'

/**
 * Composable để quản lý logic UI của danh sách tài sản
 * Bao gồm: pagination, filtering, selection
 */
export function useAssetList(initialData = []) {
  // State
  const assets = ref([...initialData])
  const filters = ref({
    searchQuery: '',
    assetType: null,
    department: null
  })
  const pagination = ref({
    currentPage: 1,
    pageSize: 20
  })
  const selectedRows = ref([])

  // Computed - Filtered data
  const filteredData = computed(() => {
    let data = [...assets.value]

    // Filter by search query
    if (filters.value.searchQuery) {
      const query = filters.value.searchQuery.toLowerCase()
      data = data.filter(
        (item) =>
          item.assetCode?.toLowerCase().includes(query) ||
          item.assetName?.toLowerCase().includes(query)
      )
    }

    // Filter by asset type
    if (filters.value.assetType) {
      data = data.filter((item) => item.assetType === filters.value.assetType)
    }

    // Filter by department
    if (filters.value.department) {
      data = data.filter((item) => item.department === filters.value.department)
    }

    return data
  })

  // Computed - Paginated data
  // NOTE: API đã trả về đúng data của trang hiện tại (đã được filter và paginate ở server-side)
  // Không cần slice hay filter lại ở client-side
  const paginatedData = computed(() => {
    return [...assets.value]
  })

  // Computed - Asset summary (tổng hợp từ dữ liệu đã filter và paginate)
  const assetSummary = computed(() => {
    return {
      quantity: paginatedData.value.reduce((sum, item) => sum + (item.quantity || 0), 0),
      originalPrice: paginatedData.value.reduce((sum, item) => sum + (item.originalPrice || 0), 0),
      depreciation: paginatedData.value.reduce((sum, item) => sum + (item.depreciation || 0), 0),
      remainingValue: paginatedData.value.reduce((sum, item) => sum + (item.remainingValue || 0), 0)
    }
  })

  // Methods - Search
  const handleSearch = () => {
    pagination.value.currentPage = 1
  }

  // Methods - Pagination
  const goToPage = (page) => {
    if (page === '...' || page < 1) {
      return
    }
    pagination.value.currentPage = page
  }

  const handlePageSizeChange = (newPageSize) => {
    pagination.value.pageSize = typeof newPageSize === 'number' ? newPageSize : parseInt(newPageSize, 10)
    pagination.value.currentPage = 1
  }

  // Methods - Selection
  const clearSelection = () => {
    selectedRows.value = []
  }

  const setSelectedRows = (rows) => {
    selectedRows.value = rows || []
  }

  // Methods - Update assets
  const setAssets = (newAssets) => {
    assets.value = newAssets || []
  }

  const updateAssets = (newAssets) => {
    assets.value = [...(newAssets || [])]
  }

  return {
    // State
    assets,
    filters,
    pagination,
    selectedRows,
    
    // Computed
    filteredData,
    paginatedData,
    assetSummary,
    
    // Methods
    handleSearch,
    goToPage,
    handlePageSizeChange,
    clearSelection,
    setSelectedRows,
    setAssets,
    updateAssets
  }
}
