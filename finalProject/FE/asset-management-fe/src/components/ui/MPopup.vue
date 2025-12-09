<template>
  <Teleport to="body">
    <transition name="popup">
      <div v-if="modelValue" class="m-popup" @click.self="handleBackdropClick">
        <div class="m-popup__container">
          <div class="m-popup__header">
            <h3 class="m-popup__title">{{ title }}</h3>
            <button class="m-popup__close" @click="handleClose">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <use href="#icon-close" />
              </svg>
            </button>
          </div>
          <div class="m-popup__body">
            <slot></slot>
          </div>
          <div class="m-popup__footer">
            <slot name="footer">
              <MButton variant="outline" @click="handleClose">Hủy</MButton>
              <MButton variant="main" @click="handleSave">Lưu</MButton>
            </slot>
          </div>
        </div>
      </div>
    </transition>
  </Teleport>
</template>

<script setup>
import { watch, onMounted, onUnmounted } from 'vue'
import MButton from './MButton.vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: 'Popup'
  },
  closeOnBackdrop: {
    type: Boolean,
    default: true
  }
})

const emit = defineEmits(['update:modelValue', 'close', 'save'])

const handleClose = () => {
  // Emit close event trước, để parent có thể quyết định có đóng hay không
  emit('close')
  // Chỉ đóng nếu parent không ngăn lại (thông qua v-model)
  // Parent có thể set lại modelValue = true trong handler để giữ popup mở
}

const handleBackdropClick = () => {
  if (props.closeOnBackdrop) {
    handleClose()
  }
}

const handleSave = () => {
  emit('save')
}

const handleEscape = (event) => {
  if (event.key === 'Escape' && props.modelValue) {
    handleClose()
  }
}

watch(
  () => props.modelValue,
  (isOpen) => {
    if (isOpen) {
      document.body.style.overflow = 'hidden'
    } else {
      document.body.style.overflow = ''
    }
  }
)

onMounted(() => {
  document.addEventListener('keydown', handleEscape)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleEscape)
  document.body.style.overflow = ''
})
</script>

<style scoped>
.m-popup {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 16px;
}

.m-popup__container {
  background-color: #ffffff;
  border-radius: 4px;
  width: 100%;
  max-width: 900px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
}

.m-popup__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 16px 16px 16px; /* ⚠️ Design Spec: Top padding 16px - Hiện tại 20px */
}

.m-popup__title {
  font-size: 18px;
  font-weight: 600;
  font-family: 'Roboto', sans-serif;
  color: #001031;
  margin: 0;
}

.m-popup__close {
  background: none;
  border: none;
  cursor: pointer;
  padding: 4px;
  color: #666666;
  transition: color 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.m-popup__close:hover {
  color: #212121;
}

.m-popup__body {
  padding: 16px 16px 52px 14px; /* ⚠️ Design Spec: Left/Right 16px - Hiện tại left 14px, bottom 52px */
  overflow-y: auto;
  flex: 1;
}

.m-popup__body > * {
  margin-top: 0;
  margin-bottom: 0;
}

.m-popup__body > *:first-child {
  margin-top: 0;
}

.m-popup__body > *:last-child {
  margin-bottom: 0;
}

.m-popup__footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 10px; /* ✅ Design Spec: Spacing between buttons 10px */
  padding: 16px 36px 16px 0; /* ⚠️ Design Spec: Bottom padding 16px, Right 16px - Hiện tại right 36px */
  background-color: #f5f5f5;
}

.m-popup__footer :deep(.m-button) {
  min-width: 100px;
  width: 0px;
  height: 36px;
  padding: 8px 20px;
}

.popup-enter-active,
.popup-leave-active {
  transition: opacity 0.3s;
}

.popup-enter-active .m-popup__container,
.popup-leave-active .m-popup__container {
  transition: transform 0.3s, opacity 0.3s;
}

.popup-enter-from,
.popup-leave-to {
  opacity: 0;
}

.popup-enter-from .m-popup__container,
.popup-leave-to .m-popup__container {
  transform: scale(0.95);
  opacity: 0;
}
</style>

