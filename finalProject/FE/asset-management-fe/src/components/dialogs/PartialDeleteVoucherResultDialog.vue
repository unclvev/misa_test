<template>
  <Teleport to="body">
    <transition name="dialog">
      <div v-if="modelValue" class="m-dialog" @click.self="handleBackdropClick">
        <div class="m-dialog__container">
          <div class="m-dialog__content">
            <div class="m-dialog__icon m-dialog__icon--warning">
              <svg width="48" height="48" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                <use href="#icon-warning" />
              </svg>
            </div>
            <div class="m-dialog__body">
              <p class="m-dialog__message">{{ message }}</p>
            </div>
          </div>
          <div class="m-dialog__footer">
            <MButton variant="main" @click="handleConfirm">Đồng ý</MButton>
          </div>
        </div>
      </div>
    </transition>
  </Teleport>
</template>

<script setup>
import { computed, watch, onMounted, onUnmounted } from 'vue'
import MButton from '../ui/MButton.vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  deletedCount: {
    type: Number,
    default: 0
  },
  totalCount: {
    type: Number,
    default: 0
  },
  cannotDeleteCount: {
    type: Number,
    default: 0
  },
  closeOnBackdrop: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:modelValue', 'confirm', 'close'])

const message = computed(() => {
  const deleted = props.deletedCount || 0
  const total = props.totalCount || 0
  const cannotDelete = props.cannotDeleteCount || 0
  
  if (cannotDelete > 0) {
    return `Đã xóa ${deleted}/${total} chứng từ. Không thể xóa ${cannotDelete} chứng từ.`
  }
  return `Đã xóa ${deleted}/${total} chứng từ.`
})

const handleConfirm = () => {
  emit('confirm')
  emit('update:modelValue', false)
  emit('close')
}

const handleBackdropClick = () => {
  if (props.closeOnBackdrop) {
    handleConfirm()
  }
}

const handleEscape = (event) => {
  if (event.key === 'Escape' && props.modelValue) {
    handleConfirm()
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
.m-dialog {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 2000;
  padding: 16px;
}

.m-dialog__container {
  background-color: #ffffff;
  border-radius: 4px;
  width: 100%;
  max-width: 450px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);
  display: flex;
  flex-direction: column;
}

.m-dialog__content {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  padding: 24px;
}

.m-dialog__icon {
  flex-shrink: 0;
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.m-dialog__icon--warning svg {
  width: 48px;
  height: 48px;
  color: #ffa726;
}

.m-dialog__icon--warning svg :deep(path),
.m-dialog__icon--warning svg :deep(circle),
.m-dialog__icon--warning svg :deep(rect) {
  fill: #ffa726;
  stroke: #ffa726;
}

.m-dialog__icon--warning svg :deep(path[fill="rgba(0,0,0,0)"]) {
  fill: #ffa726 !important;
}

.m-dialog__body {
  flex: 1;
  display: flex;
  align-items: center;
  min-height: 48px;
}

.m-dialog__message {
  font-size: 14px;
  font-weight: 400;
  font-family: 'Roboto', sans-serif;
  color: #001031;
  margin: 0;
  line-height: 20px;
}

.m-dialog__footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 8px;
  padding: 12px 24px 24px 24px;
}

.m-dialog__footer :deep(.m-button) {
  min-width: 80px;
  height: 36px;
  padding: 8px 16px;
}

.dialog-enter-active,
.dialog-leave-active {
  transition: opacity 0.3s;
}

.dialog-enter-active .m-dialog__container,
.dialog-leave-active .m-dialog__container {
  transition: transform 0.3s, opacity 0.3s;
}

.dialog-enter-from,
.dialog-leave-to {
  opacity: 0;
}

.dialog-enter-from .m-dialog__container,
.dialog-leave-to .m-dialog__container {
  transform: scale(0.95);
  opacity: 0;
}
</style>

