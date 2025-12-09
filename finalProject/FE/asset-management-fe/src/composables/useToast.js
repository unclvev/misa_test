/**
 * Composable để hiển thị toast notifications
 * Sử dụng global window.$toast được tạo bởi MToast component
 */
export function useToast() {
  const showSuccess = (message, duration = 3000) => {
    if (window.$toast) {
      window.$toast.success(message, duration)
    } else {
      // Fallback nếu toast chưa được khởi tạo
      console.warn('Toast not initialized yet')
      setTimeout(() => {
        if (window.$toast) {
          window.$toast.success(message, duration)
        }
      }, 100)
    }
  }

  const showError = (message, duration = 3000) => {
    if (window.$toast) {
      window.$toast.error(message, duration)
    } else {
      console.warn('Toast not initialized yet')
      setTimeout(() => {
        if (window.$toast) {
          window.$toast.error(message, duration)
        }
      }, 100)
    }
  }

  const showWarning = (message, duration = 3000) => {
    if (window.$toast) {
      window.$toast.warning(message, duration)
    } else {
      console.warn('Toast not initialized yet')
      setTimeout(() => {
        if (window.$toast) {
          window.$toast.warning(message, duration)
        }
      }, 100)
    }
  }

  const showInfo = (message, duration = 3000) => {
    if (window.$toast) {
      window.$toast.info(message, duration)
    } else {
      console.warn('Toast not initialized yet')
      setTimeout(() => {
        if (window.$toast) {
          window.$toast.info(message, duration)
        }
      }, 100)
    }
  }

  return {
    showSuccess,
    showError,
    showWarning,
    showInfo
  }
}

