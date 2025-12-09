<template>
  <MDialog
    :model-value="modelValue"
    :message="message"
    @update:model-value="handleUpdate"
    @confirm="handleClose"
    @cancel="handleClose"
  >
    <template #footer>
      <MButton variant="main" @click="handleClose">Đóng</MButton>
    </template>
  </MDialog>
</template>

<script setup>
import MDialog from './MDialog.vue'
import MButton from '../ui/MButton.vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  message: {
    type: String,
    default: 'Không thể xóa tài sản này vì đã có chứng từ phát sinh.'
  }
})

const emit = defineEmits(['update:modelValue', 'close'])

const handleUpdate = (value) => {
  emit('update:modelValue', value)
}

const handleClose = () => {
  emit('close')
  emit('update:modelValue', false)
}
</script>

