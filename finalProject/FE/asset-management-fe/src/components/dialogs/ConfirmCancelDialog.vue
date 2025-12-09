<template>
  <MDialog
    :model-value="modelValue"
    :message="message"
    @update:model-value="handleUpdate"
    @confirm="handleConfirm"
    @cancel="handleCancel"
  >
    <template #footer>
      <MButton variant="outline" @click="handleCancel">Không</MButton>
      <MButton variant="main" @click="handleConfirm">Hủy bỏ</MButton>
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
    default: 'Bạn có muốn hủy bỏ khai báo tài sản này?'
  }
})

const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

const handleUpdate = (value) => {
  emit('update:modelValue', value)
}

const handleConfirm = () => {
  emit('confirm')
}

const handleCancel = () => {
  emit('cancel')
}
</script>

