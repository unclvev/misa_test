<template>
  <MDialog
    :model-value="modelValue"
    :message="message"
    @update:model-value="handleUpdate"
    @confirm="handleDelete"
    @cancel="handleCancel"
  >
    <template #footer>
      <MButton variant="outline" @click="handleCancel">Không</MButton>
      <MButton variant="main" @click="handleDelete">Xóa</MButton>
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

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const message = computed(() => {
  const count = props.count || 0
  return `${count.toString().padStart(2, '0')} chứng từ đã được chọn. Bạn có muốn xóa các chứng từ này khỏi danh sách?`
})

const handleUpdate = (value) => {
  emit('update:modelValue', value)
}

const handleDelete = () => {
  emit('confirm')
}

const handleCancel = () => {
  emit('cancel')
}
</script>

