<template>
  <Teleport to="body">
    <transition-group name="toast" tag="div" class="toast-container">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        :class="['toast', `toast--${toast.type}`]"
      >
        <div class="toast__icon">
          <svg v-if="toast.type === 'success'" width="24" height="24" viewBox="0 0 48 48" fill="none" xmlns="http://www.w3.org/2000/svg">
            <circle cx="24" cy="24" r="24" fill="#50B83C" fill-opacity="0.2"/>
            <circle cx="24" cy="24" r="20" fill="#50B83C"/>
            <path d="M16 24L22 30L32 18" stroke="white" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </div>
        <div class="toast__message">{{ toast.message }}</div>
        <button class="toast__close" @click="removeToast(toast.id)">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M18 6L6 18M6 6L18 18" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
          </svg>
        </button>
      </div>
    </transition-group>
  </Teleport>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const toasts = ref([])
let toastIdCounter = 0

const addToast = (message, type = 'success', duration = 3000) => {
  const id = ++toastIdCounter
  const toast = {
    id,
    message,
    type
  }
  
  toasts.value.push(toast)
  
  // Auto remove after duration
  if (duration > 0) {
    setTimeout(() => {
      removeToast(id)
    }, duration)
  }
  
  return id
}

const removeToast = (id) => {
  const index = toasts.value.findIndex(t => t.id === id)
  if (index > -1) {
    toasts.value.splice(index, 1)
  }
}

const clearAll = () => {
  toasts.value = []
}

// Expose methods globally
onMounted(() => {
  // Ensure window object exists
  if (typeof window !== 'undefined') {
    window.$toast = {
      success: (message, duration) => addToast(message, 'success', duration),
      error: (message, duration) => addToast(message, 'error', duration),
      warning: (message, duration) => addToast(message, 'warning', duration),
      info: (message, duration) => addToast(message, 'info', duration),
      clear: clearAll
    }
  }
})

defineExpose({
  addToast,
  removeToast,
  clearAll
})
</script>

<style scoped>
.toast-container {
  position: fixed;
  top: 24px;
  right: 24px;
  z-index: 3000;
  display: flex;
  flex-direction: column;
  gap: 12px;
  pointer-events: none;
}

.toast {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background-color: #ffffff;
  border-radius: 4px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
  min-width: 300px;
  max-width: 500px;
  pointer-events: auto;
  border-left: 4px solid #50B83C;
}

.toast--success {
  border-left-color: #50B83C;
}

.toast--error {
  border-left-color: #E53935;
}

.toast--warning {
  border-left-color: #F59E0B;
}

.toast--info {
  border-left-color: #1aa4c8;
}

.toast__icon {
  flex-shrink: 0;
  width: 24px;
  height: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.toast__icon svg {
  width: 24px;
  height: 24px;
}

.toast__message {
  flex: 1;
  font-size: 14px;
  font-weight: 400;
  font-family: 'Roboto', sans-serif;
  color: #001031;
  line-height: 20px;
}

.toast__close {
  flex-shrink: 0;
  width: 20px;
  height: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  background-color: transparent;
  color: #666666;
  cursor: pointer;
  border-radius: 3px;
  padding: 0;
  transition: all 0.2s;
}

.toast__close:hover {
  background-color: #f5f5f5;
  color: #001031;
}

.toast__close svg {
  width: 16px;
  height: 16px;
}

/* Transition animations */
.toast-enter-active {
  transition: all 0.3s ease-out;
}

.toast-leave-active {
  transition: all 0.3s ease-in;
}

.toast-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.toast-leave-to {
  opacity: 0;
  transform: translateX(100%);
}

.toast-move {
  transition: transform 0.3s ease;
}
</style>

