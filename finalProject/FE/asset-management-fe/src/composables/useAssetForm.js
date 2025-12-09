import { watch } from 'vue'

/**
 * Composable để quản lý logic tính toán và xử lý form tài sản
 */
export function useAssetForm(asset, emit) {
  /**
   * Tính toán Giá trị hao mòn năm
   * Công thức: Hao mòn năm = Tỷ lệ HM x Nguyên giá
   * @param {number} depreciationRate - Tỷ lệ hao mòn (số thập phân, ví dụ: 0.1 = 10%)
   * @param {number} originalPrice - Nguyên giá
   * @returns {number} Giá trị hao mòn năm (làm tròn 2 chữ số thập phân)
   */
  const calculateAnnualDepreciation = (depreciationRate, originalPrice) => {
    const rate = Number(depreciationRate) || 0
    const price = Number(originalPrice) || 0
    
    if (rate > 0 && price > 0) {
      const annualDepreciation = rate * price
      // Làm tròn đến 2 chữ số thập phân
      return Math.round(annualDepreciation * 100) / 100
    }
    return 0
  }

  /**
   * Lấy năm hiện tại
   * @returns {number} Năm hiện tại
   */
  const getCurrentYear = () => {
    return new Date().getFullYear()
  }

  /**
   * Đảm bảo giá trị mặc định cho asset form
   * @param {Object} assetValue - Giá trị asset hiện tại
   * @returns {Object} Asset với các giá trị mặc định đã được điền
   */
  const ensureDefaultValues = (assetValue) => {
    if (!assetValue) return assetValue

    const updatedAsset = { ...assetValue }
    let updated = false

    // Đảm bảo annualDepreciationValue mặc định = 0
    if (
      updatedAsset.annualDepreciationValue === null ||
      updatedAsset.annualDepreciationValue === undefined ||
      updatedAsset.annualDepreciationValue === ''
    ) {
      updatedAsset.annualDepreciationValue = 0
      updated = true
    }

    // Tự động điền năm theo dõi = năm hiện tại
    const currentYear = getCurrentYear()
    if (
      !updatedAsset.trackingYear ||
      updatedAsset.trackingYear === null ||
      updatedAsset.trackingYear === undefined ||
      updatedAsset.trackingYear === ''
    ) {
      updatedAsset.trackingYear = currentYear
      updated = true
    }

    return { updatedAsset, updated }
  }

  /**
   * Tự động tính lại giá trị hao mòn năm khi thay đổi tỷ lệ HM hoặc nguyên giá
   * @param {Function} onUpdate - Callback khi cần cập nhật giá trị
   * @returns {Function} Cleanup function để dừng watcher
   */
  const setupAnnualDepreciationWatcher = (onUpdate) => {
    return watch(
      [() => asset.value.depreciationRate, () => asset.value.originalPrice],
      () => {
        const calculatedValue = calculateAnnualDepreciation(
          asset.value.depreciationRate,
          asset.value.originalPrice
        )
        if (asset.value.annualDepreciationValue !== calculatedValue) {
          onUpdate({ ...asset.value, annualDepreciationValue: calculatedValue })
        }
      },
      { immediate: true }
    )
  }

  /**
   * Setup watcher để đảm bảo giá trị mặc định
   * @param {Function} onUpdate - Callback khi cần cập nhật giá trị
   * @returns {Function} Cleanup function để dừng watcher
   */
  const setupDefaultValuesWatcher = (onUpdate) => {
    return watch(
      () => asset.value,
      (newValue) => {
        if (newValue) {
          const { updatedAsset, updated } = ensureDefaultValues(newValue)
          if (updated) {
            onUpdate(updatedAsset)
          }
        }
      },
      { deep: true, immediate: true }
    )
  }

  return {
    calculateAnnualDepreciation,
    getCurrentYear,
    ensureDefaultValues,
    setupAnnualDepreciationWatcher,
    setupDefaultValuesWatcher
  }
}

