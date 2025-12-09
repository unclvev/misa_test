/**
 * Format number as Vietnamese currency
 * @param {number|string} value - The value to format
 * @returns {string} Formatted currency string
 */
export const formatCurrency = (value) => {
  if (!value && value !== 0) return '0'
  return new Intl.NumberFormat('vi-VN').format(value)
}

