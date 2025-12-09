<template>
  <MDialog
    :model-value="modelValue"
    :message="message"
    @update:model-value="handleUpdate"
    @confirm="handleConfirm"
    @cancel="handleConfirm"
  >
    <template #footer>
      <MButton variant="main" @click="handleConfirm">Đồng ý</MButton>
    </template>
  </MDialog>
</template>

<script setup>
import { computed } from 'vue'
import MDialog from './MDialog.vue'
import MButton from '../ui/MButton.vue'

const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  },
  count: {
    type: Number,
    default: 0
  }
})

const emit = defineEmits(['update:modelValue', 'confirm'])

const message = computed(() => {
  const count = props.count || 0
  return `${count.toString().padStart(2, '0')} tài sản được chọn không thể xóa. Vui lòng kiểm tra lại tài sản trước khi thực hiện xóa.`
})

const handleUpdate = (value) => {
  emit('update:modelValue', value)
}

const handleConfirm = () => {
  emit('confirm')
  emit('update:modelValue', false)
}
</script>

